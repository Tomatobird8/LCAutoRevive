using GameNetcodeStuff;
using HarmonyLib;
using LCAutoRevive.Utils;

namespace LCAutoRevive.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatcher
{
    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPostfix]
    internal static void KillPlayer_Postfix(PlayerControllerB __instance)
    {
        if (__instance.IsOwner && __instance.isPlayerDead && __instance.AllowPlayerDeath())
        {
            HUDHandler.Instance.StartPlayerRevivalCountDown();
        }
    }
}
