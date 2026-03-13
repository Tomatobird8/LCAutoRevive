using HarmonyLib;
using LCAutoRevive.Utils;
using TMPro;
using UnityEngine;

namespace LCAutoRevive.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDPatcher
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        internal static void SpectateUIStartPatch(HUDManager __instance)
        {
            __instance.gameOverAnimator.gameObject.AddComponent<HUDHandler>();
        }
    }
}
