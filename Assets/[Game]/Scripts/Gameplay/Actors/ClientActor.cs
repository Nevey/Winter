using Game.Gameplay.Actors.Components.Client;
using Game.Gameplay.Actors.Services;
using Game.Network.Services;

namespace Game.Gameplay.Actors
{
    public class ClientActor : Actor
    {
        protected bool IsMine => ClientNetworkService.Instance.Client.ID == ownerID;

        protected override void Awake()
        {
            base.Awake();

            ClientActorService.Instance.RegisterActor(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientActorService.Instance.UnregisterActor(this);
        }

        protected override void OnInitialized()
        {
            if (IsMine)
            {
                AddComponent<PositionSender>();
            }
            else
            {
                AddComponent<PositionReceiver>();
            }
        }
    }
}
