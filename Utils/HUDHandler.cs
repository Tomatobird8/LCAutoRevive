using GameNetcodeStuff;
using LCAutoRevive.Compat;
using LCAutoRevive.Network;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LCAutoRevive.Utils
{
    internal class HUDHandler : MonoBehaviour
    {
        public static HUDHandler Instance { get; private set; } = null!;
        internal GameObject deathCountDownTextObject = null!;
        internal GameObject reviveCounterTextObject = null!;
        internal TextMeshProUGUI text = null!;
        internal TextMeshProUGUI reviveText = null!;
        internal bool isRunning = false;
        internal bool canRevive = false;
        internal bool isPermaDead = false;
        internal int reviveCount = 0;
        private Color textColor = new(1f, 0.5647f, 0f, 1f);

        public void Awake()
        {
            Instance = this;
            deathCountDownTextObject = new("ReviveCountDownText");
            deathCountDownTextObject.transform.parent = HUDManager.Instance.gameOverAnimator.transform.Find("SpectateUI");
            text = deathCountDownTextObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset? gameFont = HUDManager.Instance.EndOfRunStatsText.font;
            if (gameFont != null) text.font = gameFont;
            text.color = textColor;
            text.alignment = TextAlignmentOptions.Bottom;
            text.rectTransform.localScale = Vector3.one;
            text.rectTransform.anchoredPosition = new Vector2(0f, -170f);
            text.rectTransform.anchoredPosition3D = new Vector3(0f, -170f, 0f);
            text.rectTransform.anchorMax = new Vector2(1f, 0f);
            text.rectTransform.anchorMin = new Vector2(0f, 0f);
            text.rectTransform.offsetMax = new Vector2(0f, -170f);
            text.rectTransform.offsetMin = new Vector2(0f, -170f);
            text.rectTransform.sizeDelta = new Vector2(0f, 0f);
            text.fontSize = LCAutoRevive.fontSize;
            text.enableWordWrapping = false;
            text.text = EditText("Initiating...", false);
            if (InputUtilsCompat.Enabled && InputUtilsCompat.ReviveKey != null && LCAutoRevive.waitForInput)
            {
                InputUtilsCompat.ReviveKey.performed += OnActionPerformed;
            }
        }

        public void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase || !canRevive || !Application.isFocused)
            {
                return;
            }
            if (LCAutoRevive.reviveLimit > 0 && reviveCount >= LCAutoRevive.reviveLimit)
            {
                return;
            }
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == StartOfRound.Instance.localPlayerController && player.isPlayerDead && !player.isTypingChat)
                {
                    NetworkHandler.Instance.RevivePlayerServerRpc((int)player.playerClientId);
                    canRevive = false;
                    reviveCount++;
                    break;
                }
            }
        }

        public void StartPlayerRevivalCountDown()
        {
            if (!isRunning && !isPermaDead)
            {
                canRevive = false;
                if (reviveCount >= LCAutoRevive.reviveLimit && LCAutoRevive.reviveLimit > 0)
                {
                    text.text = EditText("Out of revives", false);
                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (player == StartOfRound.Instance.localPlayerController)
                        {
                            NetworkHandler.Instance.PermaDeadPlayerServerRpc((int)player.playerClientId);
                            isPermaDead = true;
                            break;
                        }
                    }
                    return;
                }
                StartCoroutine(WaitForPlayerRevival());
            }
        }

        private IEnumerator WaitForPlayerRevival()
        {
            isRunning = true;
            float timeLeft = LCAutoRevive.reviveDelayPenalty >= 0f ? LCAutoRevive.reviveDelay + (LCAutoRevive.reviveDelayPenalty * reviveCount) : LCAutoRevive.reviveDelay;
            float interval = 0.1f;
            while (timeLeft >= 0f)
            {
                yield return new WaitForEndOfFrame();
                if (interval > 0f)
                {
                    interval -= Time.deltaTime;
                    continue;
                }
                if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                {
                    text.text = EditText("",false);
                    isRunning = false;
                    reviveCount = 0;
                    yield break;
                }
                interval = 0.1f;
                timeLeft -= 0.1f + (interval - 0.1f) + Time.deltaTime;
                text.text = EditText($"Reviving... {Mathf.CeilToInt(timeLeft)}", true);
            }
            canRevive = true;
            if (InputUtilsCompat.Enabled && InputUtilsCompat.ReviveKey != null && LCAutoRevive.waitForInput)
            {
                text.text = EditText($"Press {InputUtilsCompat.ReviveKey.controls[0].displayName} to revive", true);
            }
            else
            {
                text.text = EditText("Reviving now", true);
                if (!StartOfRound.Instance.shipIsLeaving && !StartOfRound.Instance.inShipPhase)
                {
                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (player == StartOfRound.Instance.localPlayerController && player.isPlayerDead)
                        {
                            NetworkHandler.Instance.RevivePlayerServerRpc((int)player.playerClientId);
                            canRevive = false;
                            reviveCount++;
                            break;
                        }
                    }
                }
            }

            isRunning = false;
        }

        internal string EditText(string s, bool showRevives)
        {
            string newText = s;

            if (LCAutoRevive.reviveLimit > reviveCount && showRevives)
            {
                newText += $"\nRevives left: {LCAutoRevive.reviveLimit - reviveCount}";
            }
            return newText;
        }

        internal void ShipLeave()
        {
            canRevive = false;
            reviveCount = 0;
            if (text == null)
            {
                return;
            }
            text.text = EditText("",false);
        }
    }
}
