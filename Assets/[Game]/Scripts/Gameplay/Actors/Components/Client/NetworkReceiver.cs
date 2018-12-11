using System;
using Game.Network.Data;
using Game.Network.Services;
using Game.Utilities;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    public abstract class NetworkReceiver : ActorComponent
    {
        public abstract void ReceiveData(NetworkData networkData);
    }

    /// <summary>
    /// Used to handle received NetworkData client side, usually if not owned by local client
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NetworkReceiver<T> : NetworkReceiver where T : NetworkData
    {
        // Private
        [SerializeField] private bool isStrict = true;

        private bool IsMine => ClientNetworkService.Instance.Client.ID == ownerID;

        // Public
        public override Type DataFormatType => typeof(T);

        public override void Initialize(int ownerID)
        {
            base.Initialize(ownerID);

            if (isStrict && IsMine)
            {
                throw Log.Exception("I'm owned by a local Actor, I'm not supposed to be here!");
            }
        }

        protected abstract void OnReceiveData(T networkData);

        public override void ReceiveData(NetworkData networkData)
        {
            OnReceiveData((T)networkData);
        }
    }

}
