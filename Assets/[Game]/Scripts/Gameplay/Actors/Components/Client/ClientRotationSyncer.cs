using Game.Network.Data;

namespace Game.Gameplay.Actors.Components.Client
{
    public class ClientRotationSyncer : ClientNetworkComponent<NetworkRotationData>
    {
        protected override void Awake()
        {
            base.Awake();

            SendRotation();
        }

        protected override void Update()
        {
            base.Update();

            SendRotation();
        }

        private void SendRotation()
        {
            // TODO: After splitting up sending and receiving, IsMine check should not longer be needed here...
            if (!IsMine)
            {
                return;
            }

            SendData(new NetworkRotationData(ownerID, transform.rotation));
        }

        protected override void OnReceiveData(NetworkRotationData networkComponentData)
        {
            if (IsMine)
            {
                return;
            }

            transform.rotation = networkComponentData.Rotation;
        }
    }
}
