using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCAutoRevive.Patches;

namespace LCAutoRevive;

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

    internal static string replacementSymbol = "$";
    internal static string reviveTimerText = "Reviving... $";
    internal static string waitingForInputText = "Press $ to revive";
    internal static string revivingNowText = "Reviving now";
    internal static string revivesLeftText = "Revives left: $";
    internal static string outOfRevivesText = "Out of revives";

    private void Awake()
    {
        Logger = base.Logger;
        if (Instance == null) Instance = this;

        reviveDelay = Config.Bind<float>("General", "ReviveDelay", 15f, "Time until reviving is allowed.").Value;
        reviveDelayPenalty = Config.Bind<float>("General", "ReviveDelayPenalty", 0f, "Increase in revive delay per death. 0 to disable.").Value;
        reviveLimit = Config.Bind<int>("General", "ReviveLimit", 0, "Amount of revives allowed per day. 0 to disable.").Value;
        waitForInput = Config.Bind<bool>("General", "WaitForInput", true, "Should player revival require pressing the revive button after timer is up? Only works when LethalCompanyInputUtils is installed.").Value;
        preventShipLeave = Config.Bind<bool>("General", "PreventShipLeave", true, "Should ship leaving be prevented when all players are dead? Ship will leave anyway if no players have revives left.").Value;

        fontSize = Config.Bind<float>("UI", "FontSize", 24f, "Size of the revive timer text.").Value;
        replacementSymbol = Config.Bind<string>("UI", "ReplacementSymbol", "$", "Which symbol should be replaced with a variable provided for the text?").Value;
        reviveTimerText = Config.Bind<string>("UI", "ReviveTimerText", "Reviving... $", "Text for the revive timer. The variable is a timer which displays seconds left until revive.").Value;
        waitingForInputText = Config.Bind<string>("UI", "WaitingForInputText", "Press $ to revive", "Text for waiting for input to revive. The variable is a string that represents the key needed to press to revive.").Value;
        revivingNowText = Config.Bind<string>("UI", "RevivingNowText", "Reviving now", "Text for reviving now.").Value;
        revivesLeftText = Config.Bind<string>("UI", "RevivesLeftText", "Revives left: $", "Text for how many revives are left. The variable is the amount of revives left.").Value;
        outOfRevivesText = Config.Bind<string>("UI", "OutOfRevivesText", "Out of revives", "Text displayed when you have no revives left.").Value;

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
