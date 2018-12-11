using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    public class PositionReceiver : NetworkReceiver<PositionNetworkData>
    {
        private Vector3 targetPosition;

        protected override void OnReceiveData(PositionNetworkData positionNetworkData)
        {
            targetPosition.x = positionNetworkData.x;
            targetPosition.y = positionNetworkData.y;
            targetPosition.z = positionNetworkData.z;

            transform.position = targetPosition;
        }
    }
}
