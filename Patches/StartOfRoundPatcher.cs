using GameNetcodeStuff;
using HarmonyLib;
using LCAutoRevive.Network;
using LCAutoRevive.Utils;

namespace LCAutoRevive.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatcher
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        private static void AwakePrefix()
        {
            NetworkHandler.SpawnNetworkHandler();
        }

        [HarmonyPatch("ShipLeaveAutomatically")]
        [HarmonyPrefix]
        internal static bool ShipLeaveAutomaticallyPatch(StartOfRound __instance, ref bool leavingOnMidnight)
        {
            if (!LCAutoRevive.preventShipLeave || NetworkHandler.Instance.AllPlayersPermaDead())
            {
                return true;
            }
            if (!leavingOnMidnight)
            {
                __instance.allPlayersDead = false;
                return false;
            }
            return true;
        }

        [HarmonyPatch("OnPlayerDC")]
        [HarmonyPostfix]
        internal static void OnPlayerDCPostFix(StartOfRound __instance, ref int playerObjectNumber, ref ulong clientId)
        {
            if (__instance.allPlayerObjects[playerObjectNumber].GetComponent<PlayerControllerB>().disconnectedMidGame && __instance.IsServer)
            {
                NetworkHandler.Instance.DisconnectPermaDeadPlayer((int)clientId);
            }
        }

        [HarmonyPatch("ReviveDeadPlayers")]
        [HarmonyPostfix]
        internal static void ReviveDeadPlayersPostfix()
        {
            HUDHandler.Instance.canRevive = false;
            HUDHandler.Instance.isRunning = false;
            HUDHandler.Instance.isPermaDead = false;
            HUDHandler.Instance.reviveCount = 0;
            NetworkHandler.Instance.ResetPermaDeadPlayers();
        }

        [HarmonyPatch("ShipLeave")]
        [HarmonyPostfix]
        internal static void ShipLeavePostfix(StartOfRound __instance)
        {
            if (__instance.shipIsLeaving)
            {
                HUDHandler.Instance.ShipLeave();
            }
        }
    }
}
