using GameNetcodeStuff;
using HarmonyLib;
using LCAutoRevive.Network;

namespace LCAutoRevive.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix()
        {
            NetworkHandler.CreateAndRegisterPrefab();
        }

        [HarmonyPatch("Disconnect")]
        [HarmonyPostfix]
        private static void DisconnectPostfix()
        {
            NetworkHandler.DespawnNetworkHandler();
        }
    }
}
