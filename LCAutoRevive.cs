using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
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
        internal static float fontSize;
        internal static float reviveDelayPenalty;
        internal static int reviveLimit;

        private void Awake()
        {
            Logger = base.Logger;
            if (Instance == null) Instance = this;

            reviveDelay = Config.Bind<float>("General", "ReviveDelay", 15f, "Time until reviving is allowed.").Value;
            reviveDelayPenalty = Config.Bind<float>("General", "ReviveDelayPenalty", 0f, "Increase in revive delay per death. 0 to disable.").Value;
            reviveLimit = Config.Bind<int>("General", "ReviveLimit", 0, "Amount of revives allowed per day. 0 to disable.").Value;
            waitForInput = Config.Bind<bool>("General", "WaitForInput", true, "Should player revival require pressing the revive button after timer is up? Only works when LethalCompanyInputUtils is installed.").Value;
            preventShipLeave = Config.Bind<bool>("General", "PreventShipLeave", true, "Should ship leaving be prevented when all players are dead? Ship will leave anyway if no players have revives left.").Value;
            fontSize = Config.Bind<float>("General", "FontSize", 24f, "Size of the revive timer text.").Value;


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
