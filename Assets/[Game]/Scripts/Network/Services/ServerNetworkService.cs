using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using Game.Network.Data;
using Game.Services;
using Game.Utilities;

namespace Game.Network.Services
{
    public class ServerNetworkService : Service<ServerNetworkService>
    {
        // Private
        private DarkRiftServer server;

        private readonly List<IClient> clients = new List<IClient>();

        // Public
        public event Action<IClient> ClientConnectedEvent;

        public event Action<IClient> ClientDisconnectedEvent;

        public event Action<int, PositionData> PositionReceivedEvent;

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (clients.Contains(e.Client))
            {
                throw new Exception(
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
                throw new Exception(
                    $"Client with ID {e.Client.ID} disconnected but is not connected in the first place!");
            }

            e.Client.MessageReceived -= OnClientMessageReceived;
            clients.Remove(e.Client);

            ClientDisconnectedEvent?.Invoke(e.Client);
        }

        private void OnClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch (message.Tag)
                    {
                        case Tags.SPAWN:
                            // Do nothing as clients cannot spawn actors
                            // ... or ...
                            // Spawn an actor server side and tell other clients
                            break;

                        case Tags.POSITION:

                            PositionData positionData =
                                ByteArrayUtility.ByteArrayToObject<PositionData>(reader.ReadBytes());

                            // Fire PositionReceivedEvent
                            PositionReceivedEvent?.Invoke(e.Client.ID, positionData);

                            // Send position data to other clients
                            SendMessage(positionData, message, e.Client, SendMode.Unreliable);

                            break;

                        case Tags.ROTATION:
                            // Send rotation data to other clients
                            SendMessage(reader.ReadBytes(), message, e.Client, SendMode.Unreliable);
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
        /// <typeparam name="T"></typeparam>
        private void SendMessage<T>(T data, Message message, IClient sender,
            SendMode sendMode = SendMode.Reliable)
        {
            byte[] byteArray = ByteArrayUtility.ObjectToByteArray(data);

            SendMessage(byteArray, message, sender, sendMode);
        }

        /// <summary>
        /// Send message to all other clients except the sending client
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        /// <param name="sendMode"></param>
        private void SendMessage(byte[] data, Message message, IClient sender,
            SendMode sendMode = SendMode.Reliable)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(data);
                message.Serialize(writer);
            }

            foreach (IClient client in server.ClientManager.GetAllClients().Where(c => c != sender))
            {
                client.SendMessage(message, sendMode);
            }
        }

        /// <summary>
        /// Send message to all clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="sendMode"></param>
        /// <typeparam name="T"></typeparam>
        private void SendMessage<T>(T data, Message message, SendMode sendMode = SendMode.Reliable)
        {
            byte[] byteArray = ByteArrayUtility.ObjectToByteArray(data);

            SendMessage(byteArray, message, sendMode);
        }

        /// <summary>
        /// Send message to all clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="sendMode"></param>
        /// <typeparam name="T"></typeparam>
        private void SendMessage(byte[] data, Message message, SendMode sendMode = SendMode.Reliable)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(data);
                message.Serialize(writer);
            }

            foreach (IClient client in server.ClientManager.GetAllClients())
            {
                client.SendMessage(message, sendMode);
            }
        }

        public void RegisterServer(DarkRiftServer server)
        {
            this.server = server;

            this.server.ClientManager.ClientConnected += OnClientConnected;
            this.server.ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        public void SendMessage<T>(T data, ushort tag, SendMode sendMode)
        {
            using (Message message = Message.CreateEmpty(tag))
            {
                SendMessage(data, message, sendMode);
            }
        }
    }
}