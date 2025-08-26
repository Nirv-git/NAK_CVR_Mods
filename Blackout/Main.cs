using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using ConsoleColor = System.ConsoleColor;
using System.Linq;
using System.Net;
using System;
using System.Collections.Generic;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Systems.GameEventSystem;


namespace NAK.Blackout;

public class Blackout : MelonMod
{
    internal static bool inVR;
    internal static MelonLogger.Instance Logger;
    internal const string SettingsCategory = nameof(Blackout);

    public static readonly MelonPreferences_Category CategoryBlackout = 
        MelonPreferences.CreateCategory(SettingsCategory);

    public static readonly MelonPreferences_Entry<bool> EntryEnabled = 
        CategoryBlackout.CreateEntry("EntryEnabled", true, "Automatic State Change", description: "Should the screen automatically dim if head is still for enough time?");
    public static readonly MelonPreferences_Entry<bool> EntryAutoSleepState = 
        CategoryBlackout.CreateEntry("EntryAutoSleepState", true, "Auto Sleep State", description: "Should the Sleep state be used during Automatic State Change?");

    public static readonly MelonPreferences_Entry<bool> reduceVolWhenSleep =
        CategoryBlackout.CreateEntry("reduceVolWhenSleep", true, "Reduce Volume when Sleeping", description: "Reduces Master Volume when Sleeping");
    public static readonly MelonPreferences_Entry<bool> resetVolOnLaunch =
        CategoryBlackout.CreateEntry("resetVolOnLaunch", true, "Reset Volume on Game start", description: "Set Master Volume back to Normal level at game launch");
    public static readonly MelonPreferences_Entry<float> masterVolNormal =
        CategoryBlackout.CreateEntry("masterVolNormal", -1f, "Normal Master Volume", description: "Master Volume when not Sleeping");
    public static readonly MelonPreferences_Entry<float> masterVolSleep =
        CategoryBlackout.CreateEntry("masterVolSleep", 10f, "Sleep Master Volume", description: "Master Volume when Sleeping");

    public static readonly MelonPreferences_Entry<float> DrowsyEntryThreshold = 
        CategoryBlackout.CreateEntry("DrowsyEntryThreshold", .4f, "Drowsy Entry Threshold", description: "Head Velocity must stay below this for 'Enter Drowsy Time' to go from Awake > Drowsy");
    public static readonly MelonPreferences_Entry<float> DrowsyExitThreshold =
        CategoryBlackout.CreateEntry("DrowsyExitThreshold", 1.25f, "Drowsy Exit Threshold", description: "Head Velocity above this value will go from Drowsy > Awake");
    public static readonly MelonPreferences_Entry<float> SleepEntryThreshold =
        CategoryBlackout.CreateEntry("SleepEntryThreshold", .2f, "Sleep Entry Threshold", description: "Head Velocity must stay below this for 'Enter Sleep Time' to go from Drowsy > Sleep");
    public static readonly MelonPreferences_Entry<float> SleepExitThreshold =
        CategoryBlackout.CreateEntry("SleepExitThreshold", .5f, "Sleep Exit Threshold", description: "Head Velocity above this value will go from Sleep > Drowsy");

    public static readonly MelonPreferences_Entry<float> EntryDrowsyModeTimer =
       CategoryBlackout.CreateEntry("EntryDrowsyModeTimer", 15f, "Enter Drowsy Time (Minutes)", description: "How many minutes without movement until enter Drowsy mode");
    public static readonly MelonPreferences_Entry<float> EntrySleepModeTimer =
        CategoryBlackout.CreateEntry("EntrySleepModeTimer", 240f, "Enter Sleep Time (Seconds)", description: "How many seconds without movement until enter Sleep mode");
    public static readonly MelonPreferences_Entry<float> DelayStatesWakeup =
        CategoryBlackout.CreateEntry("DelayStatesWakeup", 4f, "Delay between Wakeup (Seconds)", description: "Seconds between changing from Sleep>Drowsy then Drowsy>Awake (Makes sure you don't instantly go from Sleeping to Awake)");
    public static readonly MelonPreferences_Entry<float> PlayerMovementThreshold =
       CategoryBlackout.CreateEntry("PlayerMovementThreshold", 2f, "Movement Threshold", description: "Player Velocity above this value will leave Sleep or Drowsy");

    public static readonly MelonPreferences_Entry<bool> HeadMovementVision =
        CategoryBlackout.CreateEntry("Head Movement gives vision", true, description: "Head movement will brightness sleep/drowsy states based on velocity");
    public static readonly MelonPreferences_Entry<float> HeadMovementVision_Drowsy =
        CategoryBlackout.CreateEntry("Drowsy Brighten", 2f, description: "Drowsy will be brightened by this value (0-1)");
    public static readonly MelonPreferences_Entry<float> HeadMovementVision_Sleep =
        CategoryBlackout.CreateEntry("Sleep Brighten", .3f, description: "Sleep will be brightened by this value (0-1)");
    public static readonly MelonPreferences_Entry<float> HeadMovementVision_Speed =
        CategoryBlackout.CreateEntry("Adjustment Speed", .8f, description: "Speed which the brightness value will change (Lerp)");

     public static readonly MelonPreferences_Entry<bool> joystick_En =
       CategoryBlackout.CreateEntry("joystick_En", true, "Joystick leaves states", description: "Joystick values above the deadband limit will leave Sleep or Drowsy state");
    public static readonly MelonPreferences_Entry<bool> joystick_ResetDrowsyTime =
       CategoryBlackout.CreateEntry("joystick_ResetDrowsyTime", false, "Joystick Resets Drowsy", description: "Joystick values above the deadband limit will reset the Awake > Drowsy timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<bool> joystick_ResetSleepTime =
       CategoryBlackout.CreateEntry("joystick_ResetSleepTime", true, "Joystick Resets Sleep", description: "Joystick values above the deadband limit will reset the Drowsy > Sleep timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<float> joystickDeadBand =
       CategoryBlackout.CreateEntry("joystickDeadBand", .5f, "Joystick DeadBand", description: "Joystick DeadBand");

    public static readonly MelonPreferences_Entry<bool> controllerMove_ResetDrowsyTime =
       CategoryBlackout.CreateEntry("controllerMove_ResetDrowsyTime", false, "Movement Resets Drowsy", description: "Controller Movement above the Velocity limit will reset the Awake > Drowsy timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<bool> controllerMove_ResetSleepTime =
       CategoryBlackout.CreateEntry("controllerMove_ResetSleepTime", true, "Movement Resets Sleep", description: "Controller Movement above the Velocity limit will reset the Drowsy > Sleep timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<float> controllerMove_Velocity =
       CategoryBlackout.CreateEntry("controllerMove_Velocity", .25f, "Controller Movement Velocity", description: "Controller Movement Velocity");

    public static readonly MelonPreferences_Entry<bool> qmResetDrowsyTime =
        CategoryBlackout.CreateEntry("qmResetDrowsyTime", false, "QuickMenu Resets Drowsy", description: "Opening the Quickmenu will reset the Awake > Drowsy timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<bool> qmResetSleepTime =
        CategoryBlackout.CreateEntry("qmResetSleepTime", true, "QuickMenu Resets Sleep", description: "Opening the Quickmenu will reset the Drowsy > Sleep timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<bool> MMResetDrowsyTime =
       CategoryBlackout.CreateEntry("MMResetDrowsyTime", false, "MainMenu Resets Drowsy", description: "Opening the MainMenu will reset the Awake > Drowsy timer (But not leave a state)");
    public static readonly MelonPreferences_Entry<bool> MMResetSleepTime =
        CategoryBlackout.CreateEntry("MMResetSleepTime", true, "MainMenu Resets Sleep", description: "Opening the MainMenu will reset the Drowsy > Sleep timer (But not leave a state)");

    public static readonly MelonPreferences_Entry<bool> debounceWakeup =
        CategoryBlackout.CreateEntry("debounceWakeup", true, "Debounce Wakeup", description: "Require movement over a longer time before changing state");
    public static readonly MelonPreferences_Entry<float> debounceWakeup_DeadBand =
        CategoryBlackout.CreateEntry("debounceWakeup_DeadBand", 1f, "Input Deadband Time ", description: "Second movement must come after this period (Seconds)");
    public static readonly MelonPreferences_Entry<float> debounceWakeup_waitToAwake =
        CategoryBlackout.CreateEntry("debounceWakeup_waitToAwake", 5f, "Input interval Drowsy>Awake", description: "Time after first movement where a second movement will change from Drowsy>Awake (Seconds)");
    public static readonly MelonPreferences_Entry<float> debounceWakeup_waitToDrowsy =
        CategoryBlackout.CreateEntry("debounceWakeup_waitToDrowsy", 2.5f, "Input interval Sleep>Drowsy", description: "Time after first movement where a second movement will change from Sleep>Drowsy (Seconds)");

    public static readonly MelonPreferences_Entry<float> flux_Drowsy_HDRClamp =
        CategoryBlackout.CreateEntry("flux_Drowsy_HDRClamp", 0.28f, "Drowsy HDRClamp", description: "Clamps down the brightest whites on the screen");
    public static readonly MelonPreferences_Entry<float> flux_Drowsy_Hue =
        CategoryBlackout.CreateEntry("flux_Drowsy_Hue", 0.10f, "Drowsy Hue", description: "Hue that Colorize will apply");
    public static readonly MelonPreferences_Entry<float> flux_Drowsy_Colorize =
        CategoryBlackout.CreateEntry("flux_Drowsy_Colorize", 0.2f, "Drowsy Colorize", description: "Colorize tint amount");
    public static readonly MelonPreferences_Entry<float> flux_Drowsy_Brightness =
        CategoryBlackout.CreateEntry("flux_Drowsy_Brightness", 0.6f, "Drowsy Brightness", description: "Brightness");
    public static readonly MelonPreferences_Entry<float> flux_Drowsy_Desat =
        CategoryBlackout.CreateEntry("flux_Drowsy_Desat", .3f, "Drowsy Desaturation", description: "Desaturation");

    public static readonly MelonPreferences_Entry<float> flux_Sleep_HDRClamp =
        CategoryBlackout.CreateEntry("flux_Sleep_HDRClamp", 0.1f, "Sleep HDRClamp", description: "Clamps down the brightest whites on the screen");
    public static readonly MelonPreferences_Entry<float> flux_Sleep_Hue =
        CategoryBlackout.CreateEntry("flux_Sleep_Hue", 0.10f, "Sleep Hue", description: "Hue that Colorize will apply");
    public static readonly MelonPreferences_Entry<float> flux_Sleep_Colorize =
        CategoryBlackout.CreateEntry("flux_Sleep_Colorize", 0.75f, "Sleep Colorize", description: "Colorize tint amount");
    public static readonly MelonPreferences_Entry<float> flux_Sleep_Brightness =
        CategoryBlackout.CreateEntry("flux_Sleep_Brightness", 0.1f, "Sleep Brightness", description: "Brightness");
    public static readonly MelonPreferences_Entry<float> flux_Sleep_Desat =
        CategoryBlackout.CreateEntry("flux_Sleep_Desat", .5f, "Sleep Desaturation", description: "Desaturation");

    public static readonly MelonPreferences_Entry<bool> EntryHudMessages =
        CategoryBlackout.CreateEntry("EntryHudMessages", true, "Hud Messages", description: "Notify on state change.");
    public static readonly MelonPreferences_Entry<bool> EntryDropFPSOnSleep =
       CategoryBlackout.CreateEntry("EntryDropFPSOnSleep", false, "Limit FPS While Sleep", description: "Limits FPS to 5 while in Sleep State. This only works in Desktop, as SteamVR/HMD handles VR FPS.");
    public static readonly MelonPreferences_Entry<bool> parmDriving =
        CategoryBlackout.CreateEntry("parmDriving", true, "Avatar Parameter Integration", description: "Will set the following parameters on your avatar to True/False based on awake state: BlackoutModDrowsy, BlackoutModSleep");

    //public static readonly MelonPreferences_Entry<bool> scaleDistance =
    //    CategoryBlackout.CreateEntry("scaleDistance", true, "Scale Velocity", description: "Scale Head Velocity based on Avatar Height");

    //public static readonly MelonPreferences_Entry<bool> debugText_BTKUI =
    //CategoryBlackout.CreateEntry("debugText_BTKUI", false, "Show Debug info in BTKUI", description: "Show Debug info in BTKUI");


    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        EntryEnabled.OnEntryValueChangedUntyped.Subscribe(OnUpdateEnabled);
        foreach (var entry in CategoryBlackout.Entries)
        {
            if (entry != EntryEnabled && !entry.OnEntryValueChangedUntyped.GetSubscribers().Any())
            {
                entry.OnEntryValueChangedUntyped.Subscribe(OnUpdateSettings);
            }
        }

        //debugText_BTKUI.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
        //    BlackoutController.Instance.DebugToggle(newValue);
        //});

        //UIExpansionKit addon
        if (MelonMod.RegisteredMelons.Any(it => it.Info.Name == "UI Expansion Kit"))
        {
            Logger.Msg("Initializing UIExpansionKit support.");
            UIExpansionKitAddon.Init();
        }

        //BTKUILib addon
        if (MelonMod.RegisteredMelons.Any(it => it.Info.Name == "BTKUILib"))
        {
            Logger.Msg("Initializing BTKUILib support.");
            BTKUIAddon.Init();
        }

        MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
    }

    private System.Collections.IEnumerator WaitForLocalPlayer()
    {
        AssetsHandler.loadAssets();

        while (PlayerSetup.Instance == null)
            yield return null;

        inVR = MetaPort.Instance.isUsingVr;
        PlayerSetup.Instance.gameObject.AddComponent<BlackoutController>();

        //update BlackoutController settings after it initializes
        while (BlackoutController.Instance == null)
            yield return null;

        UpdateAllSettings();
        OnEnabled();

        if(masterVolNormal.Value < 0f ) masterVolNormal.Value = MetaPort.Instance.settings.GetSettingsFloat("AudioMaster");
        else if (resetVolOnLaunch.Value)
        { //Set to default after game launch
            MetaPort.Instance.settings.SetSettingsInt("AudioMaster", (int)masterVolNormal.Value);
            ViewManager.Instance.OnSingleSettingUpdated("AudioMaster", masterVolNormal.Value.ToString());
        }

        CVRGameEventSystem.QuickMenu.OnOpen.AddListener(() =>
        {
            try
            {
                BlackoutController.Instance.QMresetTimer();
            }
            catch (Exception e) { Blackout.Logger.Error(e); }
        });

        CVRGameEventSystem.MainMenu.OnOpen.AddListener(() =>
        {
            try
            {
                BlackoutController.Instance.MMresetTimer();
            }
            catch (Exception e) { Blackout.Logger.Error(e); }
        });

        CVRGameEventSystem.VRModeSwitch.OnPostSwitch.AddListener((a) =>
        {
            try
            {
                if (Blackout.inVR != MetaPort.Instance.isUsingVr)
                {
                    Blackout.Logger.Msg("VRMode change detected! Reinitializing Blackout Instance...");
                    Blackout.inVR = MetaPort.Instance.isUsingVr;
                    BlackoutController.Instance.SetupBlackoutInstance();
                    BlackoutController.Instance.ChangeBlackoutState(BlackoutController.BlackoutState.Awake);
                }
            }
            catch (Exception e) { Blackout.Logger.Error(e); }
        });
   
    }

    private void OnEnabled()
    {
        if (!BlackoutController.Instance) return;
        if (EntryEnabled.Value)
        {
            BlackoutController.Instance.OnEnable();
        }
        else
        {
            BlackoutController.Instance.OnDisable();
        }
        BlackoutController.Instance.AutomaticStateChange = EntryEnabled.Value;
    }

    private void UpdateAllSettings()
    {
        if (!BlackoutController.Instance) return;
        BlackoutController.Instance.AutoSleepState = EntryAutoSleepState.Value;
        
        BlackoutController.Instance.scaleDistance = true;//scaleDistance.Value;
        
        BlackoutController.Instance.masterVolNormal = masterVolNormal.Value;
        BlackoutController.Instance.masterVolSleep = masterVolSleep.Value;
        BlackoutController.Instance.reduceVolWhenSleep = reduceVolWhenSleep.Value;

        BlackoutController.Instance.DrowsyEntryThreshold = DrowsyEntryThreshold.Value;
        BlackoutController.Instance.DrowsyExitThreshold = DrowsyExitThreshold.Value;
        BlackoutController.Instance.SleepEntryThreshold = SleepEntryThreshold.Value;
        BlackoutController.Instance.SleepExitThreshold = SleepExitThreshold.Value;
        BlackoutController.Instance.DrowsyModeTimer = EntryDrowsyModeTimer.Value;
        BlackoutController.Instance.SleepModeTimer = EntrySleepModeTimer.Value;
        BlackoutController.Instance.DelayStatesWakeup = DelayStatesWakeup.Value;
        BlackoutController.Instance.PlayerMovementThreshold = PlayerMovementThreshold.Value;

        BlackoutController.Instance.HeadMovementVision = HeadMovementVision.Value;
        BlackoutController.Instance.HeadMovementVision_Drowsy = HeadMovementVision_Drowsy.Value;
        BlackoutController.Instance.HeadMovementVision_Sleep = HeadMovementVision_Sleep.Value;
        BlackoutController.Instance.HeadMovementVision_Speed = HeadMovementVision_Speed.Value;

        BlackoutController.Instance.joystick_En = joystick_En.Value;
        BlackoutController.Instance.joystick_ResetDrowsyTime = joystick_ResetDrowsyTime.Value;
        BlackoutController.Instance.joystick_ResetSleepTime = joystick_ResetSleepTime.Value;
        BlackoutController.Instance.joystickDeadBand = joystickDeadBand.Value;

        BlackoutController.Instance.controllerMove_ResetDrowsyTime = controllerMove_ResetDrowsyTime.Value;
        BlackoutController.Instance.controllerMove_ResetSleepTime = controllerMove_ResetSleepTime.Value;
        BlackoutController.Instance.controllerMove_Velocity = controllerMove_Velocity.Value;

        BlackoutController.Instance.qmResetDrowsyTime = qmResetDrowsyTime.Value;
        BlackoutController.Instance.qmResetSleepTime = qmResetSleepTime.Value;
        BlackoutController.Instance.MMResetDrowsyTime = MMResetDrowsyTime.Value;
        BlackoutController.Instance.MMResetSleepTime = MMResetSleepTime.Value;

        BlackoutController.Instance.debounceWakeup = debounceWakeup.Value;
        BlackoutController.Instance.debounceWakeup_DeadBand = debounceWakeup_DeadBand.Value;
        BlackoutController.Instance.debounceWakeup_waitToAwake = debounceWakeup_waitToAwake.Value;
        BlackoutController.Instance.debounceWakeup_waitToDrowsy = debounceWakeup_waitToDrowsy.Value;

        BlackoutController.Instance.Drowsy_HDR = flux_Drowsy_HDRClamp.Value;
        BlackoutController.Instance.Drowsy_Hue = flux_Drowsy_Hue.Value;
        BlackoutController.Instance.Drowsy_Colorize = flux_Drowsy_Colorize.Value;
        BlackoutController.Instance.Drowsy_Brightness = flux_Drowsy_Brightness.Value;
        BlackoutController.Instance.Drowsy_Desat = flux_Drowsy_Desat.Value;

        BlackoutController.Instance.Sleep_HDR = flux_Sleep_HDRClamp.Value;
        BlackoutController.Instance.Sleep_Hue = flux_Sleep_Hue.Value;
        BlackoutController.Instance.Sleep_Colorize = flux_Sleep_Colorize.Value;
        BlackoutController.Instance.Sleep_Brightness = flux_Sleep_Brightness.Value;
        BlackoutController.Instance.Sleep_Desat = flux_Sleep_Desat.Value;

        BlackoutController.Instance.HudMessages = EntryHudMessages.Value;
        BlackoutController.Instance.DropFPSOnSleep = EntryDropFPSOnSleep.Value;
        BlackoutController.Instance.parmDriving = parmDriving.Value;

        BlackoutController.Instance.AdjustDimStrength();
    }

    private void OnUpdateEnabled(object arg1, object arg2) => OnEnabled();
    private void OnUpdateSettings(object arg1, object arg2) => UpdateAllSettings();
}