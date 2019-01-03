using System;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using Game.Network.Data;
using Game.Network.Types;
using Game.Services;
using Game.Utilities;
using Scripts.Gameplay.Players.Services;
using MessageReceivedEventArgs = DarkRift.Client.MessageReceivedEventArgs;

namespace Game.Network.Services
{
    public class ClientNetworkService : Service<ClientNetworkService>
    {
        // TODO: Make simplified version of UnityClient
        // Private
        private NetworkFactory networkFactory;
        
        private UnityClient unityClient;

        // Public
        public NetworkFactory NetworkFactory => networkFactory;

        public DarkRiftClient Client => unityClient.Client;

        public event Action<NetworkData> ComponentDataReceivedEvent;

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch (message.Tag)
                    {
                        case Tags.NETWORK_COMPONENT_DATA:

                            NetworkData networkData =
                                ByteArrayUtility.ByteArrayToObject<NetworkData>(reader.ReadBytes());

                            ComponentDataReceivedEvent?.Invoke(networkData);

                            break;

                        case Tags.SPAWN:

                            SpawnData spawnData =
                                ByteArrayUtility.ByteArrayToObject<SpawnData>(reader.ReadBytes());

                            // Initialize local player or spawn a networked player
                            if (spawnData.ownerID == unityClient.ID)
                            {
                                ClientPlayerService.Instance.InitializeLocalPlayer(spawnData);
                            }
                            else
                            {
                                ClientPlayerService.Instance.SpawnNetworkedPlayer(spawnData);
                            }

                            break;

                        case Tags.DESTROY:

                            DestroyData destroyData = 
                                ByteArrayUtility.ByteArrayToObject<DestroyData>(reader.ReadBytes());

                            ClientPlayerService.Instance.DestroyPlayer(destroyData);

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Send message to server from this client
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tag"></param>
        /// <param name="sendMode"></param>
        /// <typeparam name="T"></typeparam>
        public void SendMessage<T>(T data, ushort tag, SendMode sendMode = SendMode.Unreliable)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(ByteArrayUtility.ObjectToByteArray(data));

                using (Message message = Message.Create(tag, writer))
                {
                    unityClient.Client.SendMessage(message, sendMode);
                }
            }
        }

        public void RegisterNetworkFactory(NetworkFactory networkFactory)
        {
            if (this.networkFactory != null)
            {
                throw Log.Exception("Trying to register NetworkFactory, but it's already registered!");
            }

            this.networkFactory = networkFactory;
        }

        public void UnregisterNetworkFactory(NetworkFactory networkFactory)
        {
            if (this.networkFactory == null)
            {
                return;
            }

            if (this.networkFactory != networkFactory)
            {
                throw Log.Exception(
                    "Trying to unregister NetworkFactory, but it's not registered in the first place!");
            }

            this.networkFactory = null;
        }

        public void RegisterUnityClient(UnityClient unityClient)
        {
            if (this.unityClient != null)
            {
                throw Log.Exception("Trying to register UnityClient, but it's already registered!");
            }

            this.unityClient = unityClient;
            this.unityClient.MessageReceived += OnMessageReceived;
        }

        public void UnregisterUnityClient(UnityClient unityClient)
        {
            if (this.unityClient == null)
            {
                return;
            }

            if (this.unityClient != unityClient)
            {
                throw Log.Exception(
                    "Trying to unregister UnityClient, but it's not registered in the first place!");
            }

            this.unityClient.MessageReceived -= OnMessageReceived;
            this.unityClient = null;
        }
    }
}
