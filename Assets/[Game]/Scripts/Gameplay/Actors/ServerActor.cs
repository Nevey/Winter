using System;
using Game.Gameplay.Actors.Components.Server;
using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors
{
    public abstract class ServerActor : Actor
    {
        protected override void Awake()
        {
            base.Awake();

            ServerActorService.Instance.RegisterActor(this);

            // TODO: Put this in ServerActorService, for better performance?
            ServerNetworkService.Instance.ComponentDataReceivedEvent += OnNetworkComponentDataReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerActorService.Instance.UnregisterActor(this);

            ServerNetworkService.Instance.ComponentDataReceivedEvent -= OnNetworkComponentDataReceived;
        }

        private void OnNetworkComponentDataReceived(NetworkData obj)
        {
            if (obj.ownerID != ownerID)
            {
                return;
            }

            ReceiveData(obj);
        }

        private void ReceiveData(NetworkData networkData)
        {
            Type dataFormatType = networkData.GetType();

            for (int i = 0; i < componentsList.Count; i++)
            {
                NetworkSyncer networkSyncer = componentsList[i] as NetworkSyncer;

                if (networkSyncer == null || networkSyncer.DataFormatType != dataFormatType)
                {
                    continue;
                }

                networkSyncer.HandleData(networkData);
            }
        }
    }
}
