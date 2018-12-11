using System;
using DarkRift;
using Game.Network;
using Game.Network.Data;
using Game.Network.Services;
using Game.Utilities;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    /// <summary>
    /// Used to handle sending of NetworkData client side, usually if owned by local client
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NetworkSender<T> : ActorComponent where T : NetworkData
    {
        // Private
        [SerializeField] private bool isStrict = true;

        private bool IsMine => ClientNetworkService.Instance.Client.ID == ownerID;

        // Public
        public override Type DataFormatType => typeof(T);

        public override void Initialize(int ownerID)
        {
            base.Initialize(ownerID);
            
            if (isStrict && !IsMine)
            {
                throw Log.Exception("I'm not owned by a local Actor, I'm not supposed to be here!");
            }
        }

        protected void SendData(T networkData, SendMode sendMode = SendMode.Unreliable)
        {
            ClientNetworkService.Instance.SendMessage(networkData, Tags.NETWORK_COMPONENT_DATA,
                sendMode);
        }
    }
}
