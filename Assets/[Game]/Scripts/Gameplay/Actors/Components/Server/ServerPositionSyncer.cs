using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Server
{
    public class ServerPositionSyncer : ServerNetworkComponent<NetworkPositionData>
    {
        private Vector3 targetPosition;

        protected override void OnReceiveData(NetworkPositionData networkComponentData)
        {
            // Receive data from a client
            targetPosition.x = networkComponentData.x;
            targetPosition.y = networkComponentData.y;
            targetPosition.z = networkComponentData.z;

            transform.position = targetPosition;

            // Send the data to all other clients
            SendData(networkComponentData);
        }
    }
}
