using DarkRift.Client.Unity;
using DarkRift.Server.Unity;
using Game.Network.Services;
using UnityEngine;

namespace Game.Network.Types
{
    public class NetworkTypePicker : MonoBehaviour
    {
        // TODO: use scriptable object for such settings
        // TODO: based on windows "server build"
        // Private
        [SerializeField] private NetworkType networkType;

        [SerializeField] private XmlUnityServer serverPrefab;

        [SerializeField] private UnityClient clientPrefab;

        // Public
        public NetworkType NetworkType => networkType;

        private void Awake()
        {
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
        }
    }
}
