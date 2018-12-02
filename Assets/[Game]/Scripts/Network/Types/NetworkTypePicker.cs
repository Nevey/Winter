using DarkRift.Client.Unity;
using DarkRift.Server.Unity;
using Game.Network.Services;
using UnityEngine;

namespace Game.Network.Types
{
    public class NetworkTypePicker : MonoBehaviour
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
#if UNITY_EDITOR
            switch (networkType)
            {
                case NetworkType.Server:
                    Instantiate(serverPrefab);
                    break;

                case NetworkType.Client:
                    Instantiate(clientPrefab);
                    break;

                case NetworkType.Spectator:
                    // TODO...
                    break;
            }
#endif

#if SERVER_BUILD
            Instantiate(serverPrefab);
#elif CLIENT_BUILD
            Instantiate(clientPrefab);
#endif
        }
    }
}
