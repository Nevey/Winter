using Game.Network.Data;
using Game.Network.Services;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    /// <summary>
    /// Listens for position updates from the ClientNetworkService, should only be added to an
    /// actor which local client has no authority of
    /// </summary>
    public class PositionReceiver : ActorComponent
    {
        protected override void Awake()
        {
            base.Awake();

            ClientNetworkService.Instance.PositionReceivedEvent += OnPositionReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientNetworkService.Instance.PositionReceivedEvent -= OnPositionReceived;
        }

        private void OnPositionReceived(NetworkPositionData networkPositionData)
        {
            // TODO: Also/Only compare actor ID...
            if (networkPositionData.clientID != ownerID)
            {
                return;
            }

            transform.position = new Vector3(networkPositionData.x, networkPositionData.y, networkPositionData.z);
        }
    }
}
