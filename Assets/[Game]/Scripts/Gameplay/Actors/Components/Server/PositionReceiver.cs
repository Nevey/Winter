using Game.Network.Data;
using Game.Network.Services;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Server
{
    public class PositionReceiver : ActorComponent
    {
        protected override void Awake()
        {
            base.Awake();

            ServerNetworkService.Instance.PositionReceivedEvent += OnPositionReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerNetworkService.Instance.PositionReceivedEvent -= OnPositionReceived;
        }

        private void OnPositionReceived(int clientID, PositionData positionData)
        {
            if (ownerID != clientID)
            {
                return;
            }

            transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
        }
    }
}
