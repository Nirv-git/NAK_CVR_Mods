﻿using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using HarmonyLib;
using UnityEngine;

namespace NAK.BadAnimatorFix.HarmonyPatches;

static class AnimatorPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerSetup), nameof(PlayerSetup.Start))]
    private static void Postfix_PlayerSetup_Start()
    {
        BadAnimatorFixManager.OnPlayerLoaded();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CVRAvatar), nameof(CVRAvatar.Start))]
    private static void Postfix_CVRAvatar_Start(CVRAvatar __instance)
    {
        if (!BadAnimatorFix.EntryCVRAvatar.Value) return;
        AddBadAnimatorFixComponentIfAnimatorExists(__instance.gameObject);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CVRSpawnable), nameof(CVRSpawnable.Start))]
    private static void Postfix_CVRSpawnable_Start(CVRSpawnable __instance)
    {
        if (!BadAnimatorFix.EntryCVRSpawnable.Value) return;
        AddBadAnimatorFixComponentIfAnimatorExists(__instance.gameObject);
    }

    // Set QM stuff
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CVR_MenuManager), nameof(CVR_MenuManager.Start))]
    private static void Postfix_CVR_MenuManager_Start(ref CVR_MenuManager __instance)
    {
        if (!BadAnimatorFix.EntryMenus.Value) return;
        AddBadAnimatorFixComponentIfAnimatorExists(__instance.gameObject);
    }

    // Set MM stuff
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ViewManager), nameof(ViewManager.Start))]
    private static void Postfix_ViewManager_Start(ref ViewManager __instance)
    {
        if (!BadAnimatorFix.EntryMenus.Value) return;
        AddBadAnimatorFixComponentIfAnimatorExists(__instance.gameObject);
    }

    private static void AddBadAnimatorFixComponentIfAnimatorExists(GameObject gameObject)
    {
        Animator[] animators = gameObject.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            if (!animator.TryGetComponent<BadAnimatorFixer>(out _))
            {
                animator.gameObject.AddComponent<BadAnimatorFixer>();
            }
        }
    }
}
