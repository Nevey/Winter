using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    [AddComponentMenu("Winter/Client/PositionSyncer")]
    public class PositionSyncer : ClientNetworkComponent<NetworkPositionData>
    {
        private Vector3 targetPosition;

        protected override void Awake()
        {
            base.Awake();

            SendPosition();
        }

        protected override void Update()
        {
            base.Update();

            // TODO: After splitting up sending and receiving, IsMine check should not longer be needed here...
            if (IsMine)
            {
                SendPosition();
            }
        }

        private void SendPosition()
        {
            // TODO: Find a way to skip adding "ownerID" everywhere
            SendData(new NetworkPositionData(ownerID, transform.position));
        }

        // TODO: Split up sending and receiving asap
        protected override void OnReceiveData(NetworkPositionData networkComponentData)
        {
            // TODO: After splitting up sending and receiving, IsMine check should not longer be needed here...
            if (IsMine)
            {
                return;
            }

            targetPosition.x = networkComponentData.x;
            targetPosition.y = networkComponentData.y;
            targetPosition.z = networkComponentData.z;

            transform.position = targetPosition;
        }
    }
}
