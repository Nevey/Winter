using Game.Network.Data;
using Game.Network.Services;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Server
{
    public class PositionReceiver : ServerNetworkComponent<NetworkPositionData>
    {
        protected override void OnDataReceived(NetworkPositionData data)
        {
            transform.position = new Vector3(data.x, data.y, data.z);
        }
    }
}
