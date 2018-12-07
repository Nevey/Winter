using DarkRift;
using Game.Network;
using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors.Components.Client
{
    public abstract class ClientNetworkComponent<T> : NetworkComponent<T> where T : NetworkComponentData
    {
        protected bool IsMine => ClientNetworkService.Instance.Client.ID == ownerID;

        protected void SendData(T networkComponentData, SendMode sendMode = SendMode.Unreliable)
        {
            ClientNetworkService.Instance.SendMessage(networkComponentData,
                Tags.NETWORK_COMPONENT_DATA, sendMode);
        }
    }
}
