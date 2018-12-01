using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using Game.Gameplay.Actors;
using Game.Gameplay.Actors.Services;
using Game.Network;
using Game.Network.Data;
using Game.Network.Services;
using Game.Services;

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

            ServerService.Instance.ClientConnectedEvent += OnClientConnected;
            ServerService.Instance.ClientDisconnectedEvent += OnClientDisconnected;
        }

        private void OnClientConnected(IClient obj)
        {
            // TODO: Create a player service next to this actor service, handle player-only stuff there
            // TODO: Handle d/c'd players

            // TODO: Given position will be based on a system
            // Create spawn data
            SpawnData spawnData = new SpawnData(obj.ID, 0f, 0f, 0f);
            SpawnPlayer(spawnData);
        }

        private void OnClientDisconnected(IClient obj)
        {
            // Remove player, or if d/c was accidental keep actor until reconnected (or remove after a timeout)
        }

        private void SpawnPlayer(SpawnData spawnData)
        {
            ServerActorService.Instance.SpawnActor(spawnData);

            // Send spawn message
            ServerService.Instance.SendMessage(spawnData, Tags.SPAWN, SendMode.Reliable);
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
