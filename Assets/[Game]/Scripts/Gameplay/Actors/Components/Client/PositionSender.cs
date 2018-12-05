using Game.Network;
using Game.Network.Data;
using Game.Network.Services;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    /// <summary>
    /// Should only be added to an actor which local client has authority of
    /// </summary>
    public class PositionSender : ClientActorComponent
    {
        private NetworkPositionData networkPositionData;

        protected override void Awake()
        {
            base.Awake();

            SendPosition();
        }

        protected override void Update()
        {
            base.Update();

            SendPosition();
        }

        private void SendPosition()
        {
            networkPositionData.clientID = ownerID;
            networkPositionData.x = transform.position.x;
            networkPositionData.y = transform.position.y;
            networkPositionData.z = transform.position.z;

            ClientNetworkService.Instance.SendMessage(networkPositionData, Tags.POSITION);
        }
    }
}
