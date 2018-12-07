using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Server
{
    public class PositionSyncer : ServerNetworkComponent<NetworkPositionData>
    {
        private Vector3 targetPosition;

        protected override void OnReceiveData(NetworkPositionData networkComponentData)
        {
            targetPosition.x = networkComponentData.x;
            targetPosition.y = networkComponentData.y;
            targetPosition.z = networkComponentData.z;

            transform.position = targetPosition;

            SendData(networkComponentData);
        }
    }
}
