using GameNetcodeStuff;
using HarmonyLib;
using LCAutoRevive.Network;

namespace LCAutoRevive.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix()
    {
        NetworkHandler.CreateAndRegisterPrefab();
    }

    [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
    [HarmonyPostfix]
    private static void Disconnect_Postfix()
    {
        NetworkHandler.DespawnNetworkHandler();
    }
}
