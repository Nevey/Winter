using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;
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
            ClientActorService.Instance.RegisterActorFactory(this);
        }

        private void OnDestroy()
        {
            ClientActorService.Instance.UnregisterActorFactory(this);
        }

        public ClientPlayer SpawnPlayer(SpawnData spawnData)
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
