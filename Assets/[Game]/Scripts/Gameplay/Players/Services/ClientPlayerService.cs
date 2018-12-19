using System.Collections.Generic;
using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;
using Game.Services;

namespace Scripts.Gameplay.Players.Services
{
    public class ClientPlayerService : Service<ClientPlayerService>
    {
        // Private
        private readonly List<ClientPlayer> players = new List<ClientPlayer>();

        // Public
        public List<ClientPlayer> Players => players;

        public void SpawnPlayer(SpawnData spawnData)
        {
            ClientActorService.Instance.SpawnActor(spawnData);
        }

        public void DestroyPlayer(DestroyData destroyData)
        {
            ClientActorService.Instance.DestroyActor(destroyData);

            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i].OwnerID == destroyData.ownerID)
                {
                    players.RemoveAt(i);
                }
            }
        }

        public void RegisterPlayer(ClientPlayer player)
        {
            players.Add(player);
        }

        public void UnregisterPlayer(ClientPlayer player)
        {
            players.Remove(player);
        }
    }
}
