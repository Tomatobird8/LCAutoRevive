using HarmonyLib;
using LCAutoRevive.Utils;

namespace LCAutoRevive.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal class HUDPatcher
{
    [HarmonyPatch(nameof(HUDManager.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(HUDManager __instance)
    {
        __instance.gameOverAnimator.gameObject.AddComponent<HUDHandler>();
    }
}
