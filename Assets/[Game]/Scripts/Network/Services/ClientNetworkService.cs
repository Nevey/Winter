using System;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using Game.Network.Data;
using Game.Services;
using Game.Utilities;
using Scripts.Gameplay.Actors.Players.Services;
using MessageReceivedEventArgs = DarkRift.Client.MessageReceivedEventArgs;

namespace Game.Network.Services
{
    public class ClientNetworkService : Service<ClientNetworkService>
    {
        // TODO: Make simplified version of UnityClient
        // Private
        private UnityClient unityClient;

        // Public
        public DarkRiftClient Client => unityClient.Client;

        public event Action<PositionData> PositionReceivedEvent;

        public void RegisterUnityClient(UnityClient unityClient)
        {
            if (this.unityClient != null)
            {
                throw new Exception("Trying to register UnityClient, but it's already registered!");
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
                throw new Exception(
                    "Trying to unregister UnityClient, but it's not registered in the first place!");
            }

            this.unityClient.MessageReceived -= OnMessageReceived;
            this.unityClient = null;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch (message.Tag)
                    {
                        case Tags.SPAWN:
                            // Spawn a client-side actor
                            SpawnData spawnData =
                                ByteArrayUtility.ByteArrayToObject<SpawnData>(reader.ReadBytes());

                            ClientPlayerService.Instance.SpawnPlayer(spawnData);

                            break;

                        case Tags.POSITION:

                            PositionReceivedEvent?.Invoke(
                                ByteArrayUtility.ByteArrayToObject<PositionData>(reader.ReadBytes()));

                            break;

                        case Tags.ROTATION:
                            // Set rotation of client-side actor, actor service
                            // Use a rotation listener component
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
    }
}
