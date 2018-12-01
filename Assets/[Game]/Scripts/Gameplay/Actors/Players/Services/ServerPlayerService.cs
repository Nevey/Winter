using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using Game.Gameplay.Actors;
using Game.Gameplay.Actors.Services;
using Game.Network;
using Game.Network.Data;
using Game.Network.Services;
using Game.Services;
using UnityEngine;

namespace Scripts.Gameplay.Actors.Players.Services
{
    public class ServerPlayerService : Service<ServerPlayerService>
    {
        // Private
        private readonly List<ServerPlayer> players = new List<ServerPlayer>();

        // Public
        public List<ServerPlayer> Players => players;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ServerNetworkService.Instance.ClientConnectedEvent += OnClientConnected;
            ServerNetworkService.Instance.ClientDisconnectedEvent += OnClientDisconnected;
        }

        private void OnClientConnected(IClient client)
        {
            // TODO: Create a player service next to this actor service, handle player-only stuff there
            // TODO: Handle d/c'd players

            SpawnPlayer(client);
        }

        private void OnClientDisconnected(IClient obj)
        {
            // Remove player, or if d/c was accidental keep actor until reconnected (or remove after a timeout)
        }

        private void SpawnPlayer(IClient client)
        {
            // TODO: Given position will be based on a system
            // Create spawn data
            SpawnData spawnData = new SpawnData(client.ID, 0f, 0f, 0f);

            ServerActorService.Instance.SpawnActor(spawnData);

            // Send spawn message
            ServerNetworkService.Instance.SendNewMessage(spawnData, Tags.SPAWN, client,
                SendMode.Reliable, Receivers.Others);

            // Find all other players, tell newly connected client to spawn other existing players
            for (int i = 0; i < players.Count; i++)
            {
                ServerPlayer player = players[i];

                Vector3 position = player.transform.position;

                SpawnData data = new SpawnData(player.OwnerID, position.x, position.y, position.z);

                ServerNetworkService.Instance.SendNewMessage(data, Tags.SPAWN, client,
                    SendMode.Reliable, Receivers.Self);
            }
        }

        public void RegisterPlayer(ServerPlayer player)
        {
            players.Add(player);
        }

        public void UnregisterPlayer(ServerPlayer player)
        {
            players.Remove(player);
        }
    }
}
