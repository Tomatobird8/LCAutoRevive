using GameNetcodeStuff;
using UnityEngine;

namespace LCAutoRevive.Utils
{
    public static class RevivePlayer
    {
        public static void ReiveDeadPlayer(int playerId) // this is mostly copied from zeekerss' ReviveDeadPlayers()
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (player == null)
            {
                return;
            }
            player.ResetPlayerBloodObjects(player.isPlayerDead);
            if (!player.isPlayerDead)
            {
                return;
            }
            player.isClimbingLadder = false;
            if (!player.inSpecialInteractAnimation || player.currentTriggerInAnimationWith == null | !player.currentTriggerInAnimationWith?.GetComponentInChildren<MoveToExitSpecialAnimation>())
            {
                player.clampLooking = false;
                player.gameplayCamera.transform.localEulerAngles = new Vector3(player.gameplayCamera.transform.localEulerAngles.x, 0f, player.gameplayCamera.transform.localEulerAngles.z);
                player.inVehicleAnimation = false;
            }
            player.disableMoveInput = false;
            player.ResetZAndXRotation();
            player.thisController.enabled = true;
            player.health = 100;
            player.hasBeenCriticallyInjured = false;
            player.disableLookInput = false;
            player.disableInteract = false;
            player.nightVisionRadar.enabled = false;
            if (player.isPlayerDead)
            {
                player.isPlayerDead = false;
                player.isPlayerControlled = true;
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
                player.parentedToElevatorLastFrame = false;
                player.overrideGameOverSpectatePivot = null;
                StartOfRound.Instance.SetPlayerObjectExtrapolate(enable: false);
                player.TeleportPlayer(StartOfRound.Instance.GetPlayerSpawnPosition(playerId));
                player.setPositionOfDeadPlayer = false;
                player.DisablePlayerModel(player.gameObject, enable: true, disableLocalArms: true);
                player.helmetLight.enabled = false;
                player.Crouch(crouch: false);
                player.criticallyInjured = false;
                if (player.playerBodyAnimator != null)
                {
                    player.playerBodyAnimator.SetBool("Limp", value: false);
                }
                player.bleedingHeavily = false;
                player.activatingItem = false;
                player.twoHanded = false;
                player.inShockingMinigame = false;
                player.inSpecialInteractAnimation = false;
                player.freeRotationInInteractAnimation = false;
                player.disableSyncInAnimation = false;
                player.inAnimationWithEnemy = null;
                player.holdingWalkieTalkie = false;
                player.speakingToWalkieTalkie = false;
                player.isSinking = false;
                player.isUnderwater = false;
                player.sinkingValue = 0f;
                player.statusEffectAudio.Stop();
                player.DisableJetpackControlsLocally();
                player.health = 100;
                player.mapRadarDotAnimator.SetBool("dead", value: false);
                player.externalForceAutoFade = Vector3.zero;
                player.thisPlayerModel.enabled = true;
                StartOfRound.Instance.allPlayersDead = false;
                if (player.IsOwner)
                {
                    HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
                    player.hasBegunSpectating = false;
                    HUDManager.Instance.RemoveSpectateUI();
                    HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                    HUDManager.Instance.UpdateHealthUI(100, false);
                    player.spectatedPlayerScript = null;
                    player.hinderedMultiplier = 1f;
                    player.isMovementHindered = 0;
                    player.sourcesCausingSinking = 0;
                    player.reverbPreset = StartOfRound.Instance.shipReverb;
                    HUDManager.Instance.HideHUD(false);
                    SoundManager.Instance.earsRingingTimer = 0f;
                    TimeOfDay.Instance.DisableAllWeather(false);
                    StartOfRound.Instance.SetSpectateCameraToGameOverMode(false, player);
                    HUDManager.Instance.audioListenerLowPass.enabled = false;
                }
                else
                {
                    player.thisPlayerModelLOD1.enabled = true;
                    player.thisPlayerModelLOD2.enabled = true;
                }
                
                player.voiceMuffledByEnemy = false;
                SoundManager.Instance.playerVoicePitchTargets[player.playerClientId] = 1f;
                SoundManager.Instance.SetPlayerPitch(1f, (int)player.playerClientId);
                if (player.currentVoiceChatIngameSettings == null)
                {
                    StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                }
                if (player.currentVoiceChatIngameSettings != null)
                {
                    if (player.currentVoiceChatIngameSettings.voiceAudio == null)
                    {
                        player.currentVoiceChatIngameSettings.InitializeComponents();
                    }
                    if (player.currentVoiceChatIngameSettings.voiceAudio == null)
                    {
                        return;
                    }
                    player.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
                }
            }
            RagdollGrabbableObject[] array = Object.FindObjectsOfType<RagdollGrabbableObject>(); // TODO: only remove the body of the revived player
            for (int j = 0; j < array.Length; j++)
            {
                if (array[j].bodyID.Value != playerId)
                {
                    continue;
                }
                if (!array[j].isHeld)
                {
                    if (StartOfRound.Instance.IsServer)
                    {
                        if (array[j].NetworkObject.IsSpawned)
                        {
                            array[j].NetworkObject.Despawn();
                            break;
                        }
                        else
                        {
                            Object.Destroy(array[j].gameObject);
                            break;
                        }
                    }
                }
                else if (array[j].isHeld && array[j].playerHeldBy != null)
                {
                    array[j].playerHeldBy.DropAllHeldItems();
                    break;
                }
            }
            DeadBodyInfo[] array2 = Object.FindObjectsOfType<DeadBodyInfo>(includeInactive: true);
            for (int k = 0; k < array2.Length; k++)
            {
                Object.Destroy(array2[k].gameObject);
            }
            StartOfRound.Instance.livingPlayers = StartOfRound.Instance.connectedPlayersAmount + 1;
            StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }
    }
}
