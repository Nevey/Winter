using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;
using Game.Network.Types;
using Scripts.Gameplay.Players;
using UnityEngine;

namespace Game.Factories
{
    public class ClientActorFactory : MonoBehaviour
    {
        [Header("Player Prefabs")]
        [SerializeField] private ClientPlayer networkedClientPlayerPrefab;
        [SerializeField] private ClientPlayer localClientPlayerPrefab;

        private void Awake()
        {
#if SERVER_BUILD
            return;
#elif UNITY_EDITOR
            if (ClientNetworkService.Instance.NetworkFactory.NetworkType != NetworkType.Client)
            {
                return;
            }
#endif
            
            ClientActorService.Instance.RegisterActorFactory(this);

            SpawnLocalPlayer();
        }

        private void OnDestroy()
        {
            ClientActorService.Instance.UnregisterActorFactory(this);
        }

        private ClientPlayer SpawnLocalPlayer()
        {
            ClientPlayer clientPlayer = Instantiate(localClientPlayerPrefab);

            return clientPlayer;
        }

        public ClientPlayer SpawnNetworkedPlayer(SpawnData spawnData)
        {
            ClientPlayer clientPlayer = Instantiate(networkedClientPlayerPrefab);
            clientPlayer.Initialize(spawnData.ownerID);

            return clientPlayer;
        }
    }
}
