using DarkRift.Client.Unity;
using DarkRift.Server.Unity;
using Game.Network.Services;
using UnityEngine;

namespace Game.Network.Types
{
    public class NetworkFactory : MonoBehaviour
    {
        // Private
        [Header("Selecting network type is for Editor only")]
        [SerializeField] private NetworkType networkType;

        [Header("References to Server and Client prefabs, we never spawn both")]
        [SerializeField] private XmlUnityServer serverPrefab;

        [SerializeField] private UnityClient clientPrefab;

        // Public
        public NetworkType NetworkType => networkType;

        private void Awake()
        {
            
#if SERVER_BUILD
            networkType = NetworkType.Server;
#elif CLIENT_BUILD
            networkType = NetworkType.Client;
#endif

            switch (networkType)
            {
                case NetworkType.Server:
                    
                    ServerNetworkService.Instance.RegisterNetworkFactory(this);
                    Instantiate(serverPrefab);
                    
#if UNITY_EDITOR
                    ClientNetworkService.Instance.RegisterNetworkFactory(this);
#endif
                    break;

                case NetworkType.Client:
                    
                    ClientNetworkService.Instance.RegisterNetworkFactory(this);
                    Instantiate(clientPrefab);
                    
#if UNITY_EDITOR
                    ServerNetworkService.Instance.RegisterNetworkFactory(this);
#endif
                    break;

                case NetworkType.Spectator:
                    // TODO...
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (networkType)
            {
                case NetworkType.Server:
                    ServerNetworkService.Instance.UnregisterNetworkFactory(this);
#if UNITY_EDITOR
                    ClientNetworkService.Instance.UnregisterNetworkFactory(this);
#endif
                    break;
                
                case NetworkType.Client:
                    ClientNetworkService.Instance.UnregisterNetworkFactory(this);
#if UNITY_EDITOR
                    ServerNetworkService.Instance.UnregisterNetworkFactory(this);
#endif
                    break;
                
                case NetworkType.Spectator:
                    // TODO...
                    break;
            }
        }
    }
}
