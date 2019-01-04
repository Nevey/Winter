using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;
using Game.Network.Types;
using Scripts.Gameplay.Players;
using UnityEngine;

namespace Game.Factories
{
    public class ServerActorFactory : MonoBehaviour
    {
        [Header("Player Prefabs")]
        [SerializeField] private ServerPlayer serverPlayerPrefab;

        private void Start()
        {
#if CLIENT_BUILD
            return;
#elif UNITY_EDITOR
            if (ServerNetworkService.Instance.NetworkFactory.NetworkType != NetworkType.Server)
            {
                return;
            }
#endif
            
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
