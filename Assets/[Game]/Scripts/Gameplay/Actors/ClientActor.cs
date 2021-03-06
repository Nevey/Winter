﻿using System;
using Game.Gameplay.Actors.Components.Client;
using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors
{
    public abstract class ClientActor : Actor
    {
        protected bool IsMine => ClientNetworkService.Instance.Client.ID == ownerID;

        protected override void Awake()
        {
            base.Awake();

            ClientActorService.Instance.RegisterActor(this);

            // TODO: Put this in ClientActorService, for better performance?
            ClientNetworkService.Instance.ComponentDataReceivedEvent += OnNetworkComponentDataReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientActorService.Instance.UnregisterActor(this);

            ClientNetworkService.Instance.ComponentDataReceivedEvent -= OnNetworkComponentDataReceived;
        }

        private void OnNetworkComponentDataReceived(NetworkData obj)
        {
            // If this ClientActor is not supposed to be the receiver of this data, early out...
            if (obj.ownerID != ownerID)
            {
                return;
            }

            ReceiveData(obj);
        }

        public void ReceiveData(NetworkData networkData)
        {
            Type dataFormatType = networkData.GetType();

            for (int i = 0; i < componentsList.Count; i++)
            {
                NetworkReceiver receiver = componentsList[i] as NetworkReceiver;

                if (receiver == null || receiver.DataFormatType != dataFormatType)
                {
                    continue;
                }

                receiver.ReceiveData(networkData);
            }
        }
    }
}
