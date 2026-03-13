using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace LCAutoRevive.Input
{
    public class ReviveKey : LcInputActions
    {
        [InputAction(KeyboardControl.G, Name = "Revive Self")]
        public InputAction? ReviveButton { get; set; }
        public static readonly ReviveKey Instance = new();
    }
}
