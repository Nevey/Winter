using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Scripts.Gameplay.Actors.Players;
using UnityEngine;

namespace Game.Gameplay.Actors.Factories
{
    public class ActorFactory : MonoBehaviour
    {
        // Private
        [SerializeField] private ServerPlayer serverPlayerPrefab;

        [SerializeField] private ClientPlayer clientPlayerPrefab;

        private void Awake()
        {
            ServerActorService.Instance.RegisterActorFactory(this);
            ClientActorService.Instance.RegisterActorFactory(this);
        }

        private void OnDestroy()
        {
            ServerActorService.Instance.UnregisterActorFactory(this);
            ClientActorService.Instance.UnregisterActorFactory(this);
        }

        public ServerPlayer SpawnServerPlayer(SpawnData spawnData)
        {
            ServerPlayer serverPlayer = Instantiate(serverPlayerPrefab);
            serverPlayer.Initialize(spawnData.ownerID);

            return serverPlayer;
        }

        public ClientPlayer SpawnClientPlayer(SpawnData spawnData)
        {
            ClientPlayer clientPlayer = Instantiate(clientPlayerPrefab);
            clientPlayer.Initialize(spawnData.ownerID);

            return clientPlayer;
        }
    }
}
