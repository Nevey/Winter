using DarkRift;
using Game.Network;
using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors.Components.Server
{
    public abstract class ServerNetworkComponent<T> : NetworkComponent<T> where T : NetworkComponentData
    {
        protected void SendData(T networkComponentData, SendMode sendMode = SendMode.Unreliable,
            Receivers receivers = Receivers.Others)
        {
            ServerNetworkService.Instance.SendNewMessage(networkComponentData,
                Tags.NETWORK_COMPONENT_DATA, ownerID, sendMode, receivers);
        }
    }
}
