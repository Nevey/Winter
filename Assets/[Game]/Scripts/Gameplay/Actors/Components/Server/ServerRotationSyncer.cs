using Game.Network.Data;

namespace Game.Gameplay.Actors.Components.Server
{
    public class ServerRotationSyncer : ServerNetworkComponent<NetworkRotationData>
    {
        protected override void OnReceiveData(NetworkRotationData networkComponentData)
        {
            // Receive data from a client
            transform.rotation = networkComponentData.Rotation;

            // Send the data to all other clients
            SendData(networkComponentData);
        }
    }
}
