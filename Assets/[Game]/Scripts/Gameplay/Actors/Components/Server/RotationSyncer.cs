using Game.Network.Data;

namespace Game.Gameplay.Actors.Components.Server
{
    public class RotationSyncer : NetworkSyncer<RotationNetworkData>
    {
        protected override void OnReceiveData(RotationNetworkData networkData)
        {
            // Receive data from a client
            transform.rotation = networkData.Rotation;

            // Send the data to all other clients
            SendData(networkData);
        }
    }
}
