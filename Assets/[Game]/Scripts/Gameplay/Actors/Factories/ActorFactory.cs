using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;
using Scripts.Gameplay.Actors.Players;
using UnityEngine;

namespace Game.Gameplay.Actors.Factories
{
    // TODO: Separate Server vs Client code
    public class ActorFactory : MonoBehaviour
    {
        // Private
        [Header("Player Prefabs")]
        [SerializeField] private ServerPlayer serverPlayerPrefab;
        [SerializeField] private ClientPlayer networkedClientPlayerPrefab;
        [SerializeField] private ClientPlayer localClientPlayerPrefab;

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
            ClientPlayer clientPlayerPrefab =
                ClientNetworkService.Instance.Client.ID == spawnData.ownerID
                    ? localClientPlayerPrefab
                    : networkedClientPlayerPrefab;

            ClientPlayer clientPlayer = Instantiate(clientPlayerPrefab);
            clientPlayer.Initialize(spawnData.ownerID);

            return clientPlayer;
        }
    }
}
