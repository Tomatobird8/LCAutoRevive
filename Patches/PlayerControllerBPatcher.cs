using GameNetcodeStuff;
using HarmonyLib;
using LCAutoRevive.Utils;

namespace LCAutoRevive.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        [HarmonyPatch("KillPlayer")]
        [HarmonyPostfix]
        internal static void KillPlayerPostfix(PlayerControllerB __instance)
        {
            if (__instance.IsOwner && __instance.isPlayerDead && __instance.AllowPlayerDeath())
            {
                HUDHandler.Instance.StartPlayerRevivalCountDown();
            }
        }
    }
}
