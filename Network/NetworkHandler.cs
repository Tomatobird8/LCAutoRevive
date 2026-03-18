using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace LCAutoRevive.Network
{
    internal class NetworkHandler : NetworkBehaviour
    {
        private static GameObject? prefab = null;
        public static NetworkHandler Instance { get; private set; } = null!;

        internal static List<int> PermaDeadPlayers = [];

        internal static bool allPlayersDead;

        public static void CreateAndRegisterPrefab()
        {
            if (prefab != null)
            {
                return;
            }
            prefab = new GameObject(MyPluginInfo.PLUGIN_GUID + " Prefab");
            prefab.hideFlags |= HideFlags.HideAndDontSave;
            NetworkObject networkObject = prefab.AddComponent<NetworkObject>();
            var fieldInfo = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo!.SetValue(networkObject, GetHash(MyPluginInfo.PLUGIN_GUID + " Prefab"));
            prefab.AddComponent<NetworkHandler>();
            NetworkManager.Singleton.AddNetworkPrefab(prefab);

            LCAutoRevive.Logger.LogInfo("Prefab changes done.");
        }

        public static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                Instantiate(prefab)?.GetComponent<NetworkObject>().Spawn();
                LCAutoRevive.Logger.LogInfo("Spawned network handler.");
            }
        }

        public static void DespawnNetworkHandler()
        {
            if (Instance != null && Instance.gameObject.GetComponent<NetworkObject>().IsSpawned && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
            {
                Instance.gameObject.GetComponent<NetworkObject>().Despawn();
                LCAutoRevive.Logger.LogInfo("Despawned network handler.");
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public void ResetPermaDeadPlayers()
        {
            PermaDeadPlayers = [];
            allPlayersDead = false;
        }

        public int GetPermaDeadPlayerCount(bool onPlayerDC)
        {
            return onPlayerDC ? PermaDeadPlayers.Count + 1 : PermaDeadPlayers.Count;
        }

        public bool AllPlayersPermaDead()
        {
            return allPlayersDead;
        }

        public void CheckIfAllPlayersDead(bool onPlayerDC)
        {
            if (GetPermaDeadPlayerCount(onPlayerDC) >= GameNetworkManager.Instance.connectedPlayers && !allPlayersDead)
            {
                allPlayersDead = true;
                AllPlayersDeadClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RevivePlayerServerRpc(int playerid)
        {
            RevivePlayerClientRpc(playerid);
        }

        [ClientRpc]
        public void RevivePlayerClientRpc(int playerid)
        {
            Utils.RevivePlayer.ReiveDeadPlayer(playerid);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PermaDeadPlayerServerRpc(int playerid)
        {
            if (!PermaDeadPlayers.Contains(playerid))
            {
                PermaDeadPlayers.Add(playerid);
                LCAutoRevive.Logger.LogDebug($"Player {playerid} added to the permanently dead players list.");
            }
            CheckIfAllPlayersDead(false);
        }

        public void DisconnectPermaDeadPlayer(int playerid)
        {
            if (PermaDeadPlayers.Contains(playerid))
            {
                bool removed = PermaDeadPlayers.Remove(playerid);
                LCAutoRevive.Logger.LogInfo($"Player {playerid} removed from list? {removed}");
                CheckIfAllPlayersDead(false);
            }
            else
            {
                CheckIfAllPlayersDead(true);
            }
        }

        [ClientRpc]
        public void AllPlayersDeadClientRpc()
        {
            allPlayersDead = true;
            StartOfRound.Instance.ShipLeaveAutomatically(false);
        }

        protected internal static uint GetHash(string value)
        {
            return value?.Aggregate(17u, (current, c) => unchecked((current * 31) ^ c)) ?? 0u;
        }
    }
}
