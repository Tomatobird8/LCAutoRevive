using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using LCAutoRevive.Compat;
using LCAutoRevive.Patches;

namespace LCAutoRevive
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.SoftDependency)]
    public class LCAutoRevive : BaseUnityPlugin
    {
        public static LCAutoRevive Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal static float reviveDelay;
        internal static bool waitForInput;
        internal static bool preventShipLeave;

        private void Awake()
        {
            Logger = base.Logger;
            if (Instance == null) Instance = this;
            
            if (InputUtilsCompat.Enabled)
            {
                
            }

            reviveDelay = Config.Bind<float>("General", "ReviveDelay", 15f, "How long should the player be dead until revived?").Value;
            waitForInput = Config.Bind<bool>("General", "WaitForInput", true, "Should player revival require pressing the revive button after timer is up?").Value;
            preventShipLeave = Config.Bind<bool>("General", "PreventShipLeave", true, "Should ship leaving be prevented when all players die?").Value;

            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll(typeof(GameNetworkManagerPatch));
            Harmony.PatchAll(typeof(HUDPatcher));
            Harmony.PatchAll(typeof(PlayerControllerBPatcher));
            Harmony.PatchAll(typeof(StartOfRoundPatcher));

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
