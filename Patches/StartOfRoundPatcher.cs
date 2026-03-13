using HarmonyLib;
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
            Network.NetworkHandler.SpawnNetworkHandler();
        }

        [HarmonyPatch("ShipLeaveAutomatically")]
        [HarmonyPrefix]
        internal static bool ShipLeaveAutomaticallyPatch(StartOfRound __instance, ref bool leavingOnMidnight)
        {
            if (!LCAutoRevive.preventShipLeave)
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

        [HarmonyPatch("ReviveDeadPlayers")]
        [HarmonyPostfix]
        internal static void ReviveDeadPlayersPostfix()
        {
            HUDHandler.Instance.canRevive = false;
            HUDHandler.Instance.isRunning = false;
        }
    }
}
