using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Server
{
    public class PositionSyncer : NetworkSyncer<PositionNetworkData>
    {
        private Vector3 targetPosition;

        protected override void OnReceiveData(PositionNetworkData networkData)
        {
            // Receive data from a client
            targetPosition.x = networkData.x;
            targetPosition.y = networkData.y;
            targetPosition.z = networkData.z;

            transform.position = targetPosition;

            // Send the data to all other clients
            SendData(networkData);
        }
    }
}
