using System;
using DarkRift;
using Game.Network;
using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors.Components.Server
{
    public abstract class NetworkSyncer : ActorComponent
    {
        public abstract void HandleData(NetworkData networkData);
    }

    /// <summary>
    /// Used to handle NetworkData server side
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NetworkSyncer<T> : NetworkSyncer where T : NetworkData
    {
        public override Type DataFormatType => typeof(T);

        protected void SendData(T networkComponentData, SendMode sendMode = SendMode.Unreliable,
            Receivers receivers = Receivers.Others)
        {
            ServerNetworkService.Instance.SendNewMessage(networkComponentData,
                Tags.NETWORK_COMPONENT_DATA, ownerID, sendMode, receivers);
        }

        protected abstract void OnReceiveData(T networkData);

        public override void HandleData(NetworkData networkData)
        {
            OnReceiveData((T)networkData);
        }
    }
}
