using Game.Gameplay.Actors.Components.Server;
using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors
{
    public class ServerActor : Actor
    {
        protected override void Awake()
        {
            base.Awake();

            ServerActorService.Instance.RegisterActor(this);

            // TODO: Put this in ServerActorService, for better performance?
            ServerNetworkService.Instance.ComponentDataReceivedEvent += OnNetworkComponentDataReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerActorService.Instance.UnregisterActor(this);

            ServerNetworkService.Instance.ComponentDataReceivedEvent -= OnNetworkComponentDataReceived;
        }

        protected override void OnInitialized()
        {

        }

        private void OnNetworkComponentDataReceived(NetworkComponentData obj)
        {
            ReceiveData(obj);
        }
    }
}
