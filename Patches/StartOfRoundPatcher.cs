using GameNetcodeStuff;
using HarmonyLib;
using LCAutoRevive.Network;
using LCAutoRevive.Utils;

namespace LCAutoRevive.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatcher
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPrefix]
    private static void AwakePrefix()
    {
        NetworkHandler.SpawnNetworkHandler();
    }

    [HarmonyPatch(nameof(StartOfRound.ShipLeaveAutomatically))]
    [HarmonyPrefix]
    internal static bool ShipLeaveAutomatically_Prefix(StartOfRound __instance, bool leavingOnMidnight)
    {
        LCAutoRevive.Logger.LogDebug($"leavingOnMidnight: {leavingOnMidnight} AllPlayersPermaDead(): {NetworkHandler.Instance.AllPlayersPermaDead()} StartOfRound.Instance.allPlayersDead: {__instance.allPlayersDead}");
        if (!LCAutoRevive.preventShipLeave || NetworkHandler.Instance.AllPlayersPermaDead())
        {
            return true;
        }
        if (!leavingOnMidnight)
        {
            LCAutoRevive.Logger.LogDebug("Setting allPlayersDead to false.");
            __instance.allPlayersDead = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(StartOfRound.OnPlayerDC))]
    [HarmonyPostfix]
    internal static void OnPlayerDC_PostFix(StartOfRound __instance, int playerObjectNumber, ulong clientId)
    {
        if (__instance.allPlayerObjects[playerObjectNumber].GetComponent<PlayerControllerB>().disconnectedMidGame && __instance.IsServer)
        {
            NetworkHandler.Instance.DisconnectPermaDeadPlayer((int)clientId);
        }
    }

    [HarmonyPatch(nameof(StartOfRound.ReviveDeadPlayers))]
    [HarmonyPostfix]
    internal static void ReviveDeadPlayers_Postfix()
    {
        HUDHandler.Instance.canRevive = false;
        HUDHandler.Instance.isRunning = false;
        HUDHandler.Instance.isPermaDead = false;
        HUDHandler.Instance.reviveCount = 0;
        NetworkHandler.Instance.ResetPermaDeadPlayers();
    }

    [HarmonyPatch(nameof(StartOfRound.ShipLeave))]
    [HarmonyPostfix]
    internal static void ShipLeave_Postfix(StartOfRound __instance)
    {
        if (__instance.shipIsLeaving)
        {
            HUDHandler.Instance.ShipLeave();
        }
    }
}
