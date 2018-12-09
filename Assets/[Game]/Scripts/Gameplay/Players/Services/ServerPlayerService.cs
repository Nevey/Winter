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

namespace Scripts.Gameplay.Players.Services
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
            SpawnPlayer(client);
        }

        private void OnClientDisconnected(IClient client)
        {
            ServerActorService.Instance.DestroyActorsOfClient(client);

            // TODO: Tell other players
        }

        // TODO: Make it a "spawn actor" thing so we can spawn more than just players...
        private void SpawnPlayer(IClient client)
        {
            // TODO: Given position will be based on a spawning system
            // Create spawn data
            SpawnData spawnData = new SpawnData(client.ID, Vector3.zero);

            ServerActorService.Instance.SpawnActor(spawnData);

            // Send spawn message to all other clients
            ServerNetworkService.Instance.SendNewMessage(spawnData, Tags.SPAWN, client,
                SendMode.Reliable);

            // Find all other clients, tell newly connected client to spawn other existing players
            for (int i = 0; i < players.Count; i++)
            {
                ServerPlayer player = players[i];

                SpawnData data = new SpawnData(player.OwnerID, player.transform.position);


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
