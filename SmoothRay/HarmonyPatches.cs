﻿using ABI_RC.Core.Player;
using HarmonyLib;

namespace NAK.SmoothRay.HarmonyPatches;

internal class PlayerSetupPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerSetup), nameof(PlayerSetup.Start))]
    private static void Post_PlayerSetup_Start(ref PlayerSetup __instance)
    {
        __instance.vrLeftHandTracker.gameObject.AddComponent<SmoothRayer>().ray = __instance.leftRay;
        __instance.vrRightHandTracker.gameObject.AddComponent<SmoothRayer>().ray = __instance.rightRay;
    }
}