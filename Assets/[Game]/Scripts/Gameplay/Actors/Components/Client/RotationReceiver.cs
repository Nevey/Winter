using Game.Network.Data;

namespace Game.Gameplay.Actors.Components.Client
{
    public class RotationReceiver : NetworkReceiver<RotationNetworkData>
    {
        protected override void OnReceiveData(RotationNetworkData rotationNetworkData)
        {
            transform.rotation = rotationNetworkData.Rotation;
        }
    }
}
