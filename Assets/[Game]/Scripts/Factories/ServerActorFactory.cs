using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Scripts.Gameplay.Players;
using UnityEngine;

namespace Game.Factories
{
    public class ServerActorFactory : MonoBehaviour
    {
        [Header("Player Prefabs")]
        [SerializeField] private ServerPlayer serverPlayerPrefab;

        private void Awake()
        {
            ServerActorService.Instance.RegisterActorFactory(this);
        }

        private void OnDestroy()
        {
            ServerActorService.Instance.UnregisterActorFactory(this);
        }

        public ServerPlayer SpawnPlayer(SpawnData spawnData)
        {
            ServerPlayer serverPlayer = Instantiate(serverPlayerPrefab);
            serverPlayer.Initialize(spawnData.ownerID);

            return serverPlayer;
        }
    }
}
