﻿using ABI_RC.Systems.Camera;
using HarmonyLib;
using UnityEngine;

namespace NAK.PortableCameraAdditions.HarmonyPatches;

internal class PortableCameraPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PortableCamera), nameof(PortableCamera.Start))]
    private static void Postfix_PortableCamera_Start(ref PortableCamera __instance)
    {
        //run mod.Setup() instead of registering full mod with icon
        VisualMods.CameraAdditions mainMod = new VisualMods.CameraAdditions();
        mainMod.Setup(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PortableCamera), nameof(PortableCamera.OnWorldLoaded))]
    private static void Postfix_PortableCamera_OnWorldLoaded(Camera worldCamera)
    {
        VisualMods.CameraAdditions.Instance?.OnWorldLoaded(worldCamera);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PortableCamera), nameof(PortableCamera.UpdateOptionsDisplay))]
    private static void Postfix_PortableCamera_UpdateOptionsDisplay(ref bool ____showExpertSettings)
    {
        VisualMods.CameraAdditions.Instance?.OnUpdateOptionsDisplay(____showExpertSettings);
    }
}