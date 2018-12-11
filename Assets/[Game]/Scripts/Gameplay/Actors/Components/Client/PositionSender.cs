using Game.Network.Data;

namespace Game.Gameplay.Actors.Components.Client
{
    public class PositionSender : NetworkSender<PositionNetworkData>
    {
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
            // TODO: Find a way to skip adding "ownerID" everywhere
            SendData(new PositionNetworkData(ownerID, transform.position));
        }
    }
}
