using Game.Network;
using Game.Network.Data;
using Game.Network.Services;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    /// <summary>
    /// Should only be added to an actor which local client has authority of
    /// </summary>
    public class PositionSender : ActorComponent
    {
        private PositionData positionData;

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
            positionData.clientID = ownerID;
            positionData.x = transform.position.x;
            positionData.y = transform.position.y;
            positionData.z = transform.position.z;

            ClientNetworkService.Instance.SendMessage(positionData, Tags.POSITION);
        }
    }
}
