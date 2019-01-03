using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using Game.Network.Data;
using Game.Network.Types;
using Game.Services;
using Game.Utilities;

namespace Game.Network.Services
{
    public class ServerNetworkService : Service<ServerNetworkService>
    {
        // Private
        private NetworkFactory networkFactory;
        
        private DarkRiftServer server;

        private readonly List<IClient> clients = new List<IClient>();

        // Public
        public NetworkFactory NetworkFactory => networkFactory;

        public event Action<IClient> ClientConnectedEvent;

        public event Action<IClient> ClientDisconnectedEvent;

        public event Action<NetworkData> ComponentDataReceivedEvent;

        private void OnClientMessageReceived(object sender, MessageReceivedEventArgs e)
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
                            // Do nothing as clients cannot spawn actors
                            // ... or ...
                            // Spawn an actor server side and tell other clients
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Send message to all other clients except the sending client
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <param name="sendMode"></param>
        /// <param name="receivers"></param>
        /// <typeparam name="T"></typeparam>
        private void SendMessage<T>(T data, Message message, IClient sender,
            SendMode sendMode = SendMode.Reliable, Receivers receivers = Receivers.Others)
        {
            byte[] byteArray = ByteArrayUtility.ObjectToByteArray(data);

            SendMessage(byteArray, message, sender, sendMode, receivers);
        }

        /// <summary>
        /// Send message to all other clients except the sending client
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <param name="sendMode"></param>
        /// <param name="receivers"></param>
        private void SendMessage(byte[] data, Message message, IClient sender,
            SendMode sendMode = SendMode.Reliable, Receivers receivers = Receivers.Others)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(data);
                message.Serialize(writer);
            }

            switch (receivers)
            {
                case Receivers.All:

                    foreach (IClient client in server.ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, sendMode);
                    }

                    break;

                case Receivers.Others:

                    foreach (IClient client in server.ClientManager.GetAllClients().Where(c => c != sender))
                    {
                        client.SendMessage(message, sendMode);
                    }

                    break;

                case Receivers.Self:

                    sender.SendMessage(message, sendMode);

                    break;
            }
        }

        public void SendNewMessage<T>(T data, ushort tag, IClient sender,
            SendMode sendMode = SendMode.Unreliable, Receivers receivers = Receivers.Others)
        {
            using (Message message = Message.CreateEmpty(tag))
            {
                SendMessage(data, message, sender, sendMode, receivers);
            }
        }

        public void SendNewMessage<T>(T data, ushort tag, int clientID,
            SendMode sendMode = SendMode.Unreliable, Receivers receivers = Receivers.Others)
        {
            IClient sender = clients.FirstOrDefault(t => t.ID == clientID);

            SendNewMessage(data, tag, sender, sendMode, receivers);
        }

        public void RegisterServer(DarkRiftServer server)
        {
            this.server = server;

            this.server.ClientManager.ClientConnected += OnClientConnected;
            this.server.ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        public void UnregisterServer(DarkRiftServer server)
        {
            if (this.server == null || this.server != server)
            {
                return;
            }

            this.server.ClientManager.ClientConnected -= OnClientConnected;
            this.server.ClientManager.ClientDisconnected -= OnClientDisconnected;
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

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (clients.Contains(e.Client))
            {
                throw Log.Exception(
                    $"Client with ID {e.Client.ID} connected but is already connected!");
            }

            e.Client.MessageReceived += OnClientMessageReceived;
            clients.Add(e.Client);

            ClientConnectedEvent?.Invoke(e.Client);
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            if (!clients.Contains(e.Client))
            {
                throw Log.Exception(
                    $"Client with ID {e.Client.ID} disconnected but is not connected in the first place!");
            }

            e.Client.MessageReceived -= OnClientMessageReceived;
            clients.Remove(e.Client);

            ClientDisconnectedEvent?.Invoke(e.Client);
        }
    }
}
