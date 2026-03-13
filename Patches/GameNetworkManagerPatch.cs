using HarmonyLib;

namespace LCAutoRevive.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix()
        {
            Network.NetworkHandler.CreateAndRegisterPrefab();
        }

        private static void DisconnectPostfix()
        {
            Network.NetworkHandler.DespawnNetworkHandler();
        }
    }
}
