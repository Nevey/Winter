using Game.Network.Data;

namespace Game.Gameplay.Actors.Components.Client
{
    public class RotationSender : NetworkSender<RotationNetworkData>
    {
        protected override void Awake()
        {
            base.Awake();

            SendData(new RotationNetworkData(ownerID, transform.rotation));
        }

        protected override void Update()
        {
            base.Update();

            SendData(new RotationNetworkData(ownerID, transform.rotation));
        }
    }
}
