using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using MelonLoader;
using System.Text;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Systems.InputManagement;

namespace NAK.Blackout;

/*

    Functionality heavily inspired by VRSleeper on Booth: https://booth.pm/ja/items/2151940

    There are three states of "blackout":

    0 - Awake (no effect)
    1 - Drowsy (partial effect)
    2 - Sleep (full effect)

    After staying still for DrowsyModeTimer (minutes), you enter DrowsyMode.
    This mode dims the screen to your selected dimming strength.
    After continuing to stay still for SleepModeTimer (seconds), you enter SleepMode.
    This mode overrenders mostly everything with black.

    Slight movement while in SleepMode will place you in DrowsyMode until SleepModeTimer is reached again.
    Hard movement once entering DrowsyMode will fully wake you and return complete vision.

*/

public class BlackoutController : MonoBehaviour
{
    public static BlackoutController Instance;

    // The current state of the player's consciousness.
    public BlackoutState CurrentState = BlackoutState.Awake;

    // Should the states automatically change based on time?
    public bool AutomaticStateChange = true;
    // Should the sleep state be automatically transitioned to? Some may prefer drowsy state only due to dimming.
    public bool AutoSleepState = true;

    // The available states of consciousness.
    public enum BlackoutState
    {
        Awake = 0,
        Drowsy,
        Sleeping,
    }

    public float Drowsy_HDR = 0.8f;
    public float Drowsy_Hue = 0f;
    public float Drowsy_Colorize = 0f;
    public float Drowsy_Brightness = 0.5f;
    public float Drowsy_Desat = 0.2f;
    //
    public float Sleep_HDR = 0.4f;
    public float Sleep_Hue = 0f;
    public float Sleep_Colorize = 0f;
    public float Sleep_Brightness = 0.05f;
    public float Sleep_Desat = 0.5f;

    public bool scaleDistance = true;
    
    public bool reduceVolWhenSleep = true;
    public float masterVolNormal = 100f;
    public float masterVolSleep = 10f;

    public float DrowsyEntryThreshold = .75f;
    public float DrowsyExitThreshold = 1.5f;
    public float SleepEntryThreshold = .375f;
    public float SleepExitThreshold = .75f;
    // The amount of time the player must remain still to enter drowsy state (in minutes).
    public float DrowsyModeTimer = 5f;
    // The amount of time the player must remain in drowsy state before entering sleep state (in seconds).
    public float SleepModeTimer = 240f;
    public float DelayStatesWakeup = 4f;
    public float PlayerMovementThreshold = 5f;

    public bool HeadMovementVision = true;
    public float HeadMovementVision_Drowsy = .5f;
    public float HeadMovementVision_Sleep = .3f;
    public float HeadMovementVision_Speed = .8f;

    public bool joystick_En = true;
    public float joystickDeadBand = .5f;
    public bool joystick_ResetDrowsyTime = false;
    public bool joystick_ResetSleepTime = true;

    public bool controllerMove_ResetDrowsyTime = false;
    public bool controllerMove_ResetSleepTime = true;
    public float controllerMove_Velocity = 0.75f;

    public bool qmResetDrowsyTime = true;
    public bool qmResetSleepTime = true;
    public bool MMResetDrowsyTime = true;
    public bool MMResetSleepTime = true;

    public bool debounceWakeup = true;
    public float debounceWakeup_DeadBand = .5f;
    public float debounceWakeup_waitToDrowsy = 2.5f;
    public float debounceWakeup_waitToAwake = 5f;

    // Whether to display HUD messages.
    public bool HudMessages = true;
    // Whether to lower the frame rate while in sleep mode.
    public bool DropFPSOnSleep = false;
    public bool parmDriving = true;

    ////////////////////

    private Camera activeModeCam;
    private Vector3 headVelocity = Vector3.zero;
    private Vector3 headLocalVelocity = Vector3.zero;
    private Vector3 lastHeadPos = Vector3.zero;
    private Vector3 lastHeadLocalPos = Vector3.zero;
    private Vector3 last_leftConPos = Vector3.zero;
    private Vector3 last_rightConPos = Vector3.zero;
    private Vector3 controllerVelocity = Vector3.zero;

    private bool joystickActive = false;
    private float curTime = 0f;
    private float lastAwakeTime = 0f;
    private Animator blackoutAnimator;
    private int targetFPS;

    private float avatarScale = 1f;
    private float waitToAwake = 0f;
    private float waitToAwakeDeadband = 0f;
    private float waitToDrowsy = 0f;
    private float waitToDrowsyDeadband = 0f;

    private float waitDelayStates = 0f;

    public float wakeTill = 0f;
    ////////////////////

    private System.Object audioRoutine = null;
    private System.Object debugRoutine = null;

    public void ChangeBlackoutStateFromInt(int state) => ChangeBlackoutState((BlackoutState)state);

    // Changes the player's state of consciousness.
    public void ChangeBlackoutState(BlackoutState newState)
    {
        if (!blackoutAnimator) return;
        if (newState == CurrentState) return;

        lastAwakeTime = curTime;

        FindScale();

        // Update the blackout animator based on the new state.
        switch (newState)
        {
            case BlackoutState.Awake:
                blackoutAnimator.SetBool("BlackoutState.Drowsy", false);
                blackoutAnimator.SetBool("BlackoutState.Sleeping", false);
                if (AutomaticStateChange)
                {
                    if (audioRoutine != null) MelonCoroutines.Stop(audioRoutine); audioRoutine = MelonCoroutines.Start(SetAudioTo(masterVolNormal, 3f));
                }
                if (parmDriving)
                {
                    SetParam("BlackoutModDrowsy", 0f);
                    SetParam("BlackoutModSleep", 0f);
                }
                break;
            case BlackoutState.Drowsy:
                blackoutAnimator.SetBool("BlackoutState.Drowsy", true);
                blackoutAnimator.SetBool("BlackoutState.Sleeping", false);
                if (AutomaticStateChange)
                {
                    if (audioRoutine != null) MelonCoroutines.Stop(audioRoutine); audioRoutine = MelonCoroutines.Start(SetAudioTo(masterVolNormal, 3f));
                }
                if (parmDriving)
                {
                    SetParam("BlackoutModDrowsy", 1f);
                    SetParam("BlackoutModSleep", 0f);
                }
                break;
            case BlackoutState.Sleeping:
                blackoutAnimator.SetBool("BlackoutState.Sleeping", true);
                blackoutAnimator.SetBool("BlackoutState.Drowsy", false);
                if (AutomaticStateChange && reduceVolWhenSleep)
                {
                    if (audioRoutine != null) MelonCoroutines.Stop(audioRoutine);  audioRoutine = MelonCoroutines.Start(SetAudioTo(masterVolSleep, 10f));
                }
                if (parmDriving)
                {
                    SetParam("BlackoutModDrowsy", 0f);
                    SetParam("BlackoutModSleep", 1f);
                }
                break;
            default:
                break;
        }

        // Update the current state and send a HUD message if enabled.
        BlackoutState prevState = CurrentState;
        CurrentState = newState;
        SendHUDMessage($"Exiting {prevState} and entering {newState} state.");
        ChangeTargetFPS();
        AdjustDimStrength();
    }


    public void AdjustDimStrength()
    {
        //blackoutAnimator.SetFloat("BlackoutSetting.DrowsyStrength", DrowsyDimStrength * multiplier);
        blackoutAnimator.SetFloat("Drowsy_HDR", Mathf.Min(Drowsy_HDR + (visionMagnitude * HeadMovementVision_Drowsy), 1f));
        blackoutAnimator.SetFloat("Drowsy_Hue", Drowsy_Hue);
        blackoutAnimator.SetFloat("Drowsy_Colorize", Drowsy_Colorize);
        blackoutAnimator.SetFloat("Drowsy_Brightness", Mathf.Min(Drowsy_Brightness + (visionMagnitude * HeadMovementVision_Drowsy), 1f));
        blackoutAnimator.SetFloat("Drowsy_Desat", Drowsy_Desat);
        //
        blackoutAnimator.SetFloat("Sleep_HDR", Mathf.Min(Sleep_HDR + (visionMagnitude * HeadMovementVision_Sleep), 1f));
        blackoutAnimator.SetFloat("Sleep_Hue", Sleep_Hue);
        blackoutAnimator.SetFloat("Sleep_Colorize", Sleep_Colorize);
        blackoutAnimator.SetFloat("Sleep_Brightness", Mathf.Min(Sleep_Brightness + (visionMagnitude * HeadMovementVision_Sleep), 1f));
        blackoutAnimator.SetFloat("Sleep_Desat", Sleep_Desat);

    }

    public float visionMagnitude = 0f;
    public float visionMagnitude_Last = 0f;

    private void CalculateDimmingMultiplier()
    {
        if (HeadMovementVision && (CurrentState == BlackoutState.Drowsy || CurrentState == BlackoutState.Sleeping))
        {
            float wakeThreshold = CurrentState == BlackoutState.Drowsy ? (DrowsyEntryThreshold + DrowsyExitThreshold) / 2 : (SleepEntryThreshold + SleepExitThreshold) / 2;
            float normalizedMagnitude = headLocalVelocity.magnitude / wakeThreshold;
            float targetMagnitude = normalizedMagnitude;
            if (targetMagnitude > visionMagnitude) //Change quicker on rising movement
                visionMagnitude = Mathf.Lerp(visionMagnitude, targetMagnitude, (HeadMovementVision_Speed * 3f) * Time.deltaTime);
            else
                visionMagnitude = Mathf.Lerp(visionMagnitude, targetMagnitude, HeadMovementVision_Speed * Time.deltaTime);
        }
        else
        {
            visionMagnitude = 0f;
        }

        if (visionMagnitude != visionMagnitude_Last)
        {
            visionMagnitude_Last = visionMagnitude;
            AdjustDimStrength();
            return;
        }

    }

    // Initialize the BlackoutInstance object.
    private void Start()
    {
        Instance = this;
        // Get the blackout asset and instantiate it.
        GameObject blackoutGO = Instantiate(AssetsHandler.fluxPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //blackoutGO.transform.GetChild(1).gameObject.layer = CVRLayers.UIInternal;
        blackoutGO.transform.GetChild(1).gameObject.SetActive(false);
        blackoutGO.name = "BlackoutInstance";

        // Get the blackout animator component.
        blackoutAnimator = blackoutGO.GetComponent<Animator>();
        if (!blackoutAnimator)
        {
            Blackout.Logger.Error("Blackout: Could not find blackout animator component!");
            return;
        }
        SetupBlackoutInstance();

        //we dont want this to ever disable
        Camera.onPreRender += OnPreRender;
        Camera.onPostRender += OnPostRender;

        //DebugToggle(Blackout.debugText_BTKUI.Value);
    }

    //Automatic State Change
    private void Update()
    { 
        if(AutomaticStateChange || HeadMovementVision)
        {
            //get the current position of the player's head
            Vector3 curHeadPos = activeModeCam.transform.position;
            Vector3 curHeadLocalPos = activeModeCam.transform.localPosition;

            //calculate the player's head velocity by taking the difference in position
            //Adjust for the avatar's scale to keep the same relative movement
            headVelocity = (curHeadPos - lastHeadPos) / Time.deltaTime;
            headLocalVelocity = (curHeadLocalPos - lastHeadLocalPos) / Time.deltaTime / avatarScale;

            //store the current head position for use in the next frame
            lastHeadPos = curHeadPos;
            lastHeadLocalPos = curHeadLocalPos;

            CalculateDimmingMultiplier();
        }


        if (AutomaticStateChange)
        {  
            joystickActive = (Math.Abs(CVRInputManager.Instance.lookVector.x) > joystickDeadBand / 2 || Math.Abs(CVRInputManager.Instance.floatDirection) > joystickDeadBand ||
                Math.Abs(CVRInputManager.Instance.movementVector.x) > joystickDeadBand || Math.Abs(CVRInputManager.Instance.movementVector.z) > joystickDeadBand);

            if(Blackout.inVR)
            {
                Vector3 cur_leftConPos = PlayerSetup.Instance.vrRayLeft.transform.position;
                Vector3 cur_rightConPos = PlayerSetup.Instance.vrRayRight.transform.position;

                controllerVelocity = Vector3.Max((cur_leftConPos - last_leftConPos), (cur_rightConPos - last_rightConPos)) / Time.deltaTime / avatarScale;

                last_leftConPos = cur_leftConPos;
                last_rightConPos = cur_rightConPos;
            }
            else
                controllerVelocity = Vector3.zero;


            curTime = Time.time;
            //handle current state
            switch (CurrentState)
            {
                case BlackoutState.Awake:
                    HandleAwakeState();
                    break;
                case BlackoutState.Drowsy:
                    HandleDrowsyState();
                    break;
                case BlackoutState.Sleeping:
                    HandleSleepingState();
                    break;
                default:
                    break;
            }
        }    
    }

    public void OnEnable()
    {
        curTime = Time.time;
        lastAwakeTime = curTime;
    }

    public void OnDisable()
    {
        ChangeBlackoutState(BlackoutState.Awake); //Disable this?
    }

    private void OnPreRender(Camera cam)
    {
        if (cam == activeModeCam) return;
        blackoutAnimator.transform.localScale = Vector3.zero;
    }

    private void OnPostRender(Camera cam)
    {
        blackoutAnimator.transform.localScale = Vector3.one;
    }

    public void SetupBlackoutInstance()
    {
        activeModeCam = PlayerSetup.Instance.GetActiveCamera().GetComponent<Camera>();
        blackoutAnimator.transform.parent = activeModeCam.transform;
        blackoutAnimator.transform.localPosition = Vector3.zero;
        blackoutAnimator.transform.localRotation = Quaternion.identity;
        blackoutAnimator.transform.localScale = Vector3.one;
    }

    private float GetNextStateTimer()
    {
        switch (CurrentState)
        {
            case BlackoutState.Awake:
                return (lastAwakeTime + DrowsyModeTimer * 60 - curTime);
            case BlackoutState.Drowsy:
                return (lastAwakeTime + SleepModeTimer - curTime);
            case BlackoutState.Sleeping:
                return 0f;
            default:
                return 0f;
        }
    }

    //broken, needs to run next frame
    private void SendHUDMessage(string message)
    {
        Blackout.Logger.Msg(message);
        if (!CohtmlHud.Instance || !HudMessages) return;

        StringBuilder secondmessage = new StringBuilder();
        if (AutomaticStateChange)
        {
            if (CurrentState == BlackoutState.Drowsy && !AutoSleepState)
            {
                secondmessage = new StringBuilder("AutoSleepState is disabled. Staying in Drowsy State.");
            }
            else
            {
                secondmessage = new StringBuilder(GetNextStateTimer().ToString() + " seconds till next state change.");
            }
        }

        CohtmlHud.Instance.ViewDropTextImmediate("Blackout", message, secondmessage.ToString());
    }

    private void ChangeTargetFPS()
    {
        if (!DropFPSOnSleep) return;

        //store target FPS to restore, i check each time just in case it changed
        targetFPS = MetaPort.Instance.settings.GetSettingInt("GraphicsFramerateTarget", 0);

        Application.targetFrameRate = (CurrentState == BlackoutState.Sleeping) ? 5 : targetFPS;
    }

    private void HandleAwakeState()
    {
        //small movement should reset sleep timer
        if (headLocalVelocity.magnitude > DrowsyEntryThreshold || headVelocity.magnitude > PlayerMovementThreshold || (joystick_ResetDrowsyTime && joystickActive) ||
            (controllerMove_ResetDrowsyTime && controllerVelocity.magnitude > controllerMove_Velocity))
        {
            lastAwakeTime = curTime;
        }
        //enter drowsy mode after few minutes
        if (curTime > lastAwakeTime + DrowsyModeTimer * 60 && wakeTill < curTime)
        {
            ChangeBlackoutState(BlackoutState.Drowsy); 
        }
    }

    private void HandleDrowsyState()
    {
        //hard movement should exit drowsy state
        if ((headLocalVelocity.magnitude > DrowsyExitThreshold || headVelocity.magnitude > PlayerMovementThreshold || (joystick_En && joystickActive)) && curTime > waitDelayStates )
        {
            //Blackout.Logger.Msg($"Curtime:{curTime:F2} | headLocalVel:{headLocalVelocity.magnitude:F2} | headVel:{headVelocity.magnitude:F2} | waitToAwakeDeadband:{waitToAwakeDeadband:F2} | waitToAwake{waitToAwake:F2} | joy:{joystickActive}");
            if (!debounceWakeup || (curTime > waitToAwakeDeadband && curTime < waitToAwake))
            {//Make sure motion continues before advancing up a state
                ChangeBlackoutState(BlackoutState.Awake); 
                return;
            }
            else if (curTime > waitToAwake)
            {
                waitToAwake = curTime + debounceWakeup_waitToAwake; //Add option to customize
                waitToAwakeDeadband = curTime + debounceWakeup_DeadBand;
            }
        }
        //small movement should reset sleep timer
        if (headLocalVelocity.magnitude > SleepEntryThreshold || headVelocity.magnitude > PlayerMovementThreshold || (joystick_ResetSleepTime && joystickActive) || 
            (controllerMove_ResetSleepTime && controllerVelocity.magnitude > controllerMove_Velocity))
        {
            lastAwakeTime = curTime;
        }
        //enter full sleep mode
        if (AutoSleepState && curTime > lastAwakeTime + SleepModeTimer && wakeTill < curTime)
        {
            ChangeBlackoutState(BlackoutState.Sleeping);
        }
        //CalculateDimmingMultiplier();
    }

    private void HandleSleepingState()
    {
        //small movement should enter drowsy state
        if (headLocalVelocity.magnitude > SleepExitThreshold || headVelocity.magnitude > PlayerMovementThreshold || (joystick_En && joystickActive))
        { //Make sure motion continues before advancing up a state
            //Blackout.Logger.Msg($"Curtime:{curTime:F2} | headLocalVel:{headLocalVelocity.magnitude:F2} | headVel:{headVelocity.magnitude:F2} | waitToDrowsyDeadband:{waitToDrowsyDeadband:F2} | waitToDrowsy{waitToDrowsy:F2}| joy:{joystickActive}");
            if (!debounceWakeup || (curTime > waitToDrowsyDeadband && curTime < waitToDrowsy))
            {
                waitDelayStates = curTime + DelayStatesWakeup;
                ChangeBlackoutState(BlackoutState.Drowsy);
            }
            else if (curTime > waitToDrowsy)
            {
                waitToDrowsy = curTime + debounceWakeup_waitToDrowsy;
                waitToDrowsyDeadband = curTime + debounceWakeup_DeadBand;
            }
        }
    }

    public void QMresetTimer()
    {
        if ((qmResetDrowsyTime && CurrentState == BlackoutState.Awake) || (qmResetSleepTime && CurrentState == BlackoutState.Drowsy))
            lastAwakeTime = curTime;
    }

    public void MMresetTimer()
    {
        if ((MMResetDrowsyTime && CurrentState == BlackoutState.Awake) || (MMResetSleepTime && CurrentState == BlackoutState.Drowsy))
            lastAwakeTime = curTime;
    }

    private void FindScale()
    {
        if ((!PlayerSetup.Instance._avatar.GetComponent<Animator>()?.avatar?.isHuman ?? true) || PlayerSetup.Instance._avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head) == null)
        {
            Blackout.Logger.Msg("Animator is null, or is not Humanoid, defaulting to 1f for scale");
            avatarScale = 1f;
            return;
        }

        Animator anim = PlayerSetup.Instance._avatar.GetComponent<Animator>();
        if (scaleDistance)
        {
            try
            {
                var height = Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position, anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position) +
                    Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position, anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position) +
                    Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position, anim.GetBoneTransform(HumanBodyBones.Hips).position) +
                    Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.Hips).position, anim.GetBoneTransform(HumanBodyBones.Spine).position) +
                    Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.Spine).position, anim.GetBoneTransform(HumanBodyBones.Chest).position) +
                    Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.Chest).position, anim.GetBoneTransform(HumanBodyBones.Neck).position) +
                    Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.Neck).position, anim.GetBoneTransform(HumanBodyBones.Head).position);
                Blackout.Logger.Msg($"Height:{height:F2}");
                avatarScale = height;
            }
            catch (Exception ex) { avatarScale = 1f; Blackout.Logger.Error("Error Measuring Height defaulting to 1\n" + ex.ToString()); }

        }
        else
            avatarScale = 1f;
    }


    public IEnumerator SetAudioTo(float value, float duration)
    {
        float starting = MetaPort.Instance.settings.GetSettingsFloat("AudioMaster");
        if (starting == value)
            yield break;
        Blackout.Logger.Msg($"AudioMaster Starting:{starting:F2}");
        float startime = Time.time;
        float endTime = Time.time + duration;

        CohtmlHud.Instance.ViewDropText("Blackout", "Master Audio Changing", $"Changing from {starting:F0} to {(int)value}");

        while (Time.time < endTime)
        {
            float newValue = Mathf.Lerp(starting, value, (Time.time - startime) / (endTime - startime));
            MetaPort.Instance.settings.SetSettingsInt("AudioMaster", (int)newValue);
            ViewManager.Instance.OnSingleSettingUpdated("AudioMaster", newValue.ToString());
            Blackout.Logger.Msg($"AudioMaster:{newValue:F2}");
            yield return new WaitForSeconds(.5f);
        }
        MetaPort.Instance.settings.SetSettingsInt("AudioMaster", (int)value);
        ViewManager.Instance.OnSingleSettingUpdated("AudioMaster", value.ToString());
        Blackout.Logger.Msg($"AudioMaster-Final:{value:F2} | Readback {MetaPort.Instance.settings.GetSettingsFloat("AudioMaster")}");
        CohtmlHud.Instance.ViewDropTextImmediate("Blackout", "Master Audio Changed", $"Master Audio Set to {(int)value}");

    }

    private void SetParam(string name, float value)
    {
        MelonLogger.Msg($"Setting {name} to {value}");
        PlayerSetup.Instance.animatorManager.SetAnimatorParameter(name, value);
    }


//    public void DebugToggle(bool state)
//    {
//        if (debugRoutine != null)
//        {
//            MelonLoader.MelonCoroutines.Stop(debugRoutine);
//            BTKUIAddon.debug.CategoryName = $"";
//            BTKUIAddon.debug2.CategoryName = $"";
//            BTKUIAddon.debug3.CategoryName = $"";
//            BTKUIAddon.debug4.CategoryName = $"";
//            BTKUIAddon.debug5.CategoryName = $"";
//            BTKUIAddon.debug6.CategoryName = $"";
//        }
//        if (state)
//            debugRoutine = MelonLoader.MelonCoroutines.Start(DebugText());
//    }

//    private IEnumerator DebugText()
//    {
//        while (true)
//        {
//            BTKUIAddon.debug.CategoryName = $"Curtime:{curTime:F2} LastAwake:{lastAwakeTime:F2} wakeTill:{wakeTill:F2} waitDelayStates:{waitDelayStates:F2}";
//            BTKUIAddon.debug2.CategoryName = $"drowsyMagnitude:{visionMagnitude}"; 
//            BTKUIAddon.debug3.CategoryName = $"HeadLocalVel:{headLocalVelocity.magnitude:F2} HeadVel:{headVelocity.magnitude:F2} controllerVelocity:{controllerVelocity.magnitude:F2}";
//            BTKUIAddon.debug4.CategoryName = $"DrowsyDeadband:{waitToDrowsyDeadband:F2} waitToDrowsy{waitToDrowsy:F2} | AwakeDeadband:{waitToAwakeDeadband:F2} waitToAwake{waitToAwake:F2}";
//            BTKUIAddon.debug5.CategoryName = $"joystick:{joystickActive} | look.x{CVRInputManager.Instance.lookVector.x:F2} | float:{CVRInputManager.Instance.floatDirection:F2} | move.x:{CVRInputManager.Instance.movementVector.x:F2} | move.y:{CVRInputManager.Instance.movementVector.y:F2} | move.z:{CVRInputManager.Instance.movementVector.z:F2}";
//            BTKUIAddon.debug6.CategoryName = $"";
//            yield return new WaitForSeconds(.2f);
//        }
//    }
}
