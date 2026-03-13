using UnityEngine.InputSystem;

namespace LCAutoRevive.Compat
{
    internal static class InputUtilsCompat
    {
        public static bool Enabled =>
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils");

        public static InputAction? ReviveKey =>
            Input.ReviveKey.Instance.ReviveButton;
    }
}
