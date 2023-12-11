using BTKUILib;
using BTKUILib.UIObjects;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace NAK.Blackout;

public  class BTKUIAddon
{
    //public static Category debug;
    //public static Category debug2;
    //public static Category debug3;
    //public static Category debug4;
    //public static Category debug5;
    //public static Category debug6;

    public static void loadAssets()
    {
        Assembly l_assembly = Assembly.GetExecutingAssembly();
        string l_assemblyName = l_assembly.GetName().Name;
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Awake", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HandDrawn_Sun.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Drowsy", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HandDrawn_Sunset.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Sleeping", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HandDrawn_Moon.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Drowsy_Visual", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Drowsy_Visual.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Sleeping_Visual", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Sleeping_Visual.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Drowsy_Time", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Drowsy_Time.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Sleeping_Time", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Sleeping_Time.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Player-Movement", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Player-Movement.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Debounce", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Debounce.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Debounce_Drowsy", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Debounce_Drowsy.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Debounce_Sleep", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Debounce_Sleep.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-Joystick", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.Joystick.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-SettingsBlackout", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.SettingsBlackout.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-DelayStatesWakeup", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.DelayStatesWakeup.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-DrowsyEntryThreshold", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.DrowsyEntryThreshold.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-DrowsyExitThreshold", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.DrowsyExitThreshold.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-SleepEntryThreshold", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.SleepEntryThreshold.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-SleepExitThreshold", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.SleepExitThreshold.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-HeadMovementVision_Drowsy", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HeadMovementVision_Drowsy.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-HeadMovementVision_Sleep", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HeadMovementVision_Sleep.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-HeadMovementVision_Speed", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HeadMovementVision_Speed.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-HandMovement", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.HandMovement.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-SleepTimerPlus", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.SleepTimerPlus.png"));
        QuickMenuAPI.PrepareIcon("Blackout", "Blackout-SleepTimerPlusPlus", Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".Icons.SleepTimerPlusPlus.png"));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Init()
    {
        loadAssets();

        //Add myself to the Misc Menu
        Page miscPage = QuickMenuAPI.MiscTabPage;

        Category miscCategory = miscPage.AddCategory(Blackout.SettingsCategory, "Blackout");

        AddMelonToggle(ref miscCategory, Blackout.EntryEnabled);
        //Add my own page to not clog up Misc Menu
        Page blackoutPage = miscCategory.AddPage("Blackout Settings", "Blackout-SettingsBlackout", "Configure the settings for Blackout.", "Blackout");

        miscCategory.AddButton("30 Min Sleep Timer", "Blackout-SleepTimerPlus", "Don't change states downward for 30min").OnPress += () =>
        {
            BlackoutController.Instance.wakeTill = Time.time + 60 * 30;
        };
        miscCategory.AddButton("60 Min Sleep Timer", "Blackout-SleepTimerPlusPlus", "Don't change states downward for 60min").OnPress += () =>
        {
            BlackoutController.Instance.wakeTill = Time.time + 60 * 60;
        };


        blackoutPage.MenuTitle = "Blackout Settings";
        blackoutPage.MenuSubtitle = "Dim screen after set time of sitting still, or configure with manual control. Should be nice for VR sleeping.";
        //debug = blackoutPage.AddCategory("");
        //debug2 = blackoutPage.AddCategory("");
        //debug3 = blackoutPage.AddCategory("");
        //debug4 = blackoutPage.AddCategory("");
        //debug5 = blackoutPage.AddCategory("");
        //debug6 = blackoutPage.AddCategory("");


        {
            Category blackoutCategory = blackoutPage.AddCategory("Blackout");
            
            //manual state changing
            var state_Awake = blackoutCategory.AddButton("Awake State", "Blackout-Awake", "Enter the Awake State.");
            state_Awake.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Awake);
            var state_Drowsy = blackoutCategory.AddButton("Drowsy State", "Blackout-Drowsy", "Enter the Drowsy State.");
            state_Drowsy.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Drowsy);
            var state_Sleeping = blackoutCategory.AddButton("Sleeping State", "Blackout-Sleeping", "Enter the Sleeping State.");
            state_Sleeping.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Sleeping);
        }

        {
            Category drowsySleepSettCategory = blackoutPage.AddCategory("");
            AddMelonToggle(ref drowsySleepSettCategory, Blackout.EntryEnabled);

            Page drowsyVisSettPage = drowsySleepSettCategory.AddPage("Drowsy Visual Settings", "Blackout-Drowsy_Visual", "Configure the Visual settings for Drowsy.", "Blackout");
            Page SleepVisSettPage = drowsySleepSettCategory.AddPage("Sleep Visual Settings", "Blackout-Sleeping_Visual", "Configure the Visual settings for Sleep.", "Blackout");

            AddMelonSlider(ref drowsyVisSettPage, Blackout.flux_Drowsy_HDRClamp, .1f, 1f);
            AddMelonSlider(ref drowsyVisSettPage, Blackout.flux_Drowsy_Hue, 0f, 1f);
            AddMelonSlider(ref drowsyVisSettPage, Blackout.flux_Drowsy_Colorize, 0f, 1f);
            AddMelonSlider(ref drowsyVisSettPage, Blackout.flux_Drowsy_Brightness, 0f, 1f);
            AddMelonSlider(ref drowsyVisSettPage, Blackout.flux_Drowsy_Desat, 0f, 1f);
            Category drowsyVisSettCategory = drowsyVisSettPage.AddCategory("");
            var state_Awake_drowsyVisSett = drowsyVisSettCategory.AddButton("Awake State", "Blackout-Awake", "Enter the Awake State.");
            state_Awake_drowsyVisSett.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Awake);
            var state_Drowsy_drowsyVisSett = drowsyVisSettCategory.AddButton("Drowsy State", "Blackout-Drowsy", "Enter the Drowsy State.");
            state_Drowsy_drowsyVisSett.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Drowsy);

            AddMelonSlider(ref SleepVisSettPage, Blackout.flux_Sleep_HDRClamp, .1f, 1f);
            AddMelonSlider(ref SleepVisSettPage, Blackout.flux_Sleep_Hue, 0f, 1f);
            AddMelonSlider(ref SleepVisSettPage, Blackout.flux_Sleep_Colorize, 0f, 1f);
            AddMelonSlider(ref SleepVisSettPage, Blackout.flux_Sleep_Brightness, 0f, 1f);
            AddMelonSlider(ref SleepVisSettPage, Blackout.flux_Sleep_Desat, 0f, 1f);
            Category SleepVisSettCategory = SleepVisSettPage.AddCategory("");
            var state_Awake_SleepVisSett = SleepVisSettCategory.AddButton("Awake State", "Blackout-Awake", "Enter the Awake State.");
            state_Awake_SleepVisSett.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Awake);
            var state_Sleeping_SleepVisSett = SleepVisSettCategory.AddButton("Sleeping State", "Blackout-Sleeping", "Enter the Sleeping State.");
            state_Sleeping_SleepVisSett.OnPress += () => BlackoutController.Instance?.ChangeBlackoutState(BlackoutController.BlackoutState.Sleeping);

            AddMelonToggle(ref drowsySleepSettCategory, Blackout.EntryAutoSleepState);
        }

        {
            Category cat = blackoutPage.AddCategory("Audio");
            AddMelonToggle(ref cat, Blackout.reduceVolWhenSleep);
            AddMelonToggle(ref cat, Blackout.resetVolOnLaunch);
        }

        AddMelonSlider(ref blackoutPage, Blackout.masterVolNormal, 0f, 1f, 100);
        AddMelonSlider(ref blackoutPage, Blackout.masterVolSleep, 0f, 1f, 100);


        {
            Category cat = blackoutPage.AddCategory("Movement & Timer Settings");
            AddMelonInput(ref cat, "Blackout-DrowsyEntryThreshold", Blackout.DrowsyEntryThreshold); //Velocity
            AddMelonInput(ref cat, "Blackout-DrowsyExitThreshold", Blackout.DrowsyExitThreshold); //Velocity
            AddMelonInput(ref cat, "Blackout-SleepEntryThreshold", Blackout.SleepEntryThreshold); //Velocity
            AddMelonInput(ref cat, "Blackout-SleepExitThreshold", Blackout.SleepExitThreshold); //Velocity

            AddMelonInput(ref cat, "Blackout-Drowsy_Time", Blackout.EntryDrowsyModeTimer); //Minutes
            AddMelonInput(ref cat, "Blackout-Sleeping_Time", Blackout.EntrySleepModeTimer); //Seconds
            AddMelonInput(ref cat, "Blackout-DelayStatesWakeup", Blackout.DelayStatesWakeup); //Second
            AddMelonInput(ref cat, "Blackout-Player-Movement", Blackout.PlayerMovementThreshold); //Velocity
            //AddMelonToggle(ref cat, Blackout.scaleDistance);

        }

        {
            Category cat = blackoutPage.AddCategory("Head Movement Vision");
            AddMelonToggle(ref cat, Blackout.HeadMovementVision);
            AddMelonInput(ref cat, "Blackout-HeadMovementVision_Drowsy", Blackout.HeadMovementVision_Drowsy); 
            AddMelonInput(ref cat, "Blackout-HeadMovementVision_Sleep", Blackout.HeadMovementVision_Sleep);
            AddMelonInput(ref cat, "Blackout-HeadMovementVision_Speed", Blackout.HeadMovementVision_Speed);

        }

        {
            Category cat = blackoutPage.AddCategory("Joystick");
            AddMelonToggle(ref cat, Blackout.joystick_ResetDrowsyTime);
            AddMelonToggle(ref cat, Blackout.joystick_ResetSleepTime);
            AddMelonToggle(ref cat, Blackout.joystick_En);
            AddMelonInput(ref cat, "Blackout-Joystick", Blackout.joystickDeadBand); //Angle    
        }

        {
            Category cat = blackoutPage.AddCategory("VR Controller Movement");
            AddMelonToggle(ref cat, Blackout.controllerMove_ResetDrowsyTime);
            AddMelonToggle(ref cat, Blackout.controllerMove_ResetSleepTime);
            AddMelonInput(ref cat, "Blackout-HandMovement", Blackout.controllerMove_Velocity);    
        }

        {
            Category cat = blackoutPage.AddCategory("Menu Activity");
            AddMelonToggle(ref cat, Blackout.qmResetDrowsyTime);
            AddMelonToggle(ref cat, Blackout.qmResetSleepTime);
            AddMelonToggle(ref cat, Blackout.MMResetDrowsyTime);
            AddMelonToggle(ref cat, Blackout.MMResetSleepTime);
        }


        {
            Category cat = blackoutPage.AddCategory("Activity Debounce");
            AddMelonToggle(ref cat, Blackout.debounceWakeup);
            AddMelonInput(ref cat, "Blackout-Debounce", Blackout.debounceWakeup_DeadBand); //Seconds
            AddMelonInput(ref cat, "Blackout-Debounce_Drowsy", Blackout.debounceWakeup_waitToAwake); //Seconds
            AddMelonInput(ref cat, "Blackout-Debounce_Sleep", Blackout.debounceWakeup_waitToDrowsy); //Seconds
        }

        {
            Category cat = blackoutPage.AddCategory("Misc Settings");
            //hud messages
            AddMelonToggle(ref cat, Blackout.EntryHudMessages);
            //lower fps while sleep (desktop)
            AddMelonToggle(ref cat, Blackout.EntryDropFPSOnSleep);
            AddMelonToggle(ref cat, Blackout.parmDriving);
        }

    }

    private static void AddMelonToggle(ref Category category, MelonLoader.MelonPreferences_Entry<bool> entry)
    {
        category.AddToggle(entry.DisplayName, entry.Description, entry.Value).OnValueUpdated += b => entry.Value = b;
    }

    private static void AddMelonSlider(ref Page page, MelonLoader.MelonPreferences_Entry<float> entry, float min, float max, float mult = 1f)
    {
        page.AddSlider(entry.DisplayName, entry.Description, entry.Value/mult, min, max).OnValueUpdated += f => entry.Value = f * mult;
    }

    private static void AddMelonInput(ref Category category, string icon, MelonLoader.MelonPreferences_Entry<float> entry)
    {
        category.AddButton(entry.DisplayName, icon, entry.Description).OnPress += () =>
        {
            QuickMenuAPI.OpenNumberInput(entry.Description, entry.Value, (action) =>
            {
                entry.Value = action;
            });
        };
    }

    private static void AddMelonInput(ref Category category, string icon, MelonLoader.MelonPreferences_Entry<int> entry)
    {
        category.AddButton(entry.DisplayName, icon, entry.Description + "(int)").OnPress += () =>
        {
            QuickMenuAPI.OpenNumberInput(entry.Description, entry.Value, (action) =>
            {
                entry.Value = (int)action;
            });
        };
    }
}