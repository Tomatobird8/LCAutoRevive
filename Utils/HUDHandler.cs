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
        GameObject deathCounterTextObject = null!;
        internal bool isRunning = false;
        internal bool canRevive = false;

        public void Awake()
        {
            Instance = this;
            deathCounterTextObject = new("ReviveCountDownText");
            deathCounterTextObject.transform.parent = HUDManager.Instance.gameOverAnimator.transform.Find("SpectateUI");
            TextMeshProUGUI t = deathCounterTextObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset? gameFont = HUDManager.Instance.EndOfRunStatsText.font;
            if (gameFont != null) t.font = gameFont;
            t.color = new Color(1f, 0.5647f, 0f, 1f);
            t.alignment = TextAlignmentOptions.Bottom;
            t.rectTransform.localScale = Vector3.one;
            t.rectTransform.anchoredPosition = new Vector2(0f, -170f);
            t.rectTransform.anchoredPosition3D = new Vector3(0f, -170f, 0f);
            t.rectTransform.anchorMax = new Vector2(1f, 0f);
            t.rectTransform.anchorMin = new Vector2(0f, 0f);
            t.rectTransform.offsetMax = new Vector2(0f, -170f);
            t.rectTransform.offsetMin = new Vector2(0f, -170f);
            t.rectTransform.sizeDelta = new Vector2(0f, 0f);
            t.fontSize = 24f;
            t.enableWordWrapping = false;
            t.text = "Initiating...";
            if (InputUtilsCompat.Enabled && InputUtilsCompat.ReviveKey != null && LCAutoRevive.waitForInput)
            {
                InputUtilsCompat.ReviveKey.performed += OnActionPerformed;
            }
        }

        public void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (!StartOfRound.Instance.shipIsLeaving && !StartOfRound.Instance.inShipPhase && canRevive && Application.isFocused)
            {
                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player == StartOfRound.Instance.localPlayerController && player.isPlayerDead && !player.isTypingChat)
                    {
                        NetworkHandler.Instance.RevivePlayerServerRpc((int)player.playerClientId);
                        canRevive = false;
                    }
                }
            }
        }

        public void StartPlayerRevivalCountDown()
        {
            if (!isRunning)
            {
                canRevive = false;
                StartCoroutine(WaitForPlayerRevival());
            }
        }

        private IEnumerator WaitForPlayerRevival()
        {
            isRunning = true;
            TextMeshProUGUI t = deathCounterTextObject.GetComponent<TextMeshProUGUI>();
            float timeLeft = LCAutoRevive.reviveDelay >= 0f ? LCAutoRevive.reviveDelay : 0f;
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
                    t.text = "";
                    isRunning = false;
                    yield break;
                }
                interval = 0.1f;
                timeLeft -= 0.1f + (interval - 0.1f) + Time.deltaTime;
                t.text = $"Reviving... {Mathf.CeilToInt(timeLeft)}";
            }
            canRevive = true;
            if (InputUtilsCompat.Enabled && InputUtilsCompat.ReviveKey != null && LCAutoRevive.waitForInput)
            {
                t.text = $"Press {InputUtilsCompat.ReviveKey.controls[0].displayName} to revive";
            }
            else
            {
                t.text = $"Reviving now";
                if (!StartOfRound.Instance.shipIsLeaving && !StartOfRound.Instance.inShipPhase)
                {
                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (player == StartOfRound.Instance.localPlayerController && player.isPlayerDead)
                        {
                            NetworkHandler.Instance.RevivePlayerServerRpc((int)player.playerClientId);
                            canRevive = false;
                        }
                    }
                }
            }

            isRunning = false;
        }
    }
}
