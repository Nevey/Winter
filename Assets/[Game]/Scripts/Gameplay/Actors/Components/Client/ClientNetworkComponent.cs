using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors.Components.Client
{
    // TODO: Got to sync the ID of this component somehow...
    public abstract class ClientNetworkComponent<T> : ClientActorComponent where T : NetworkComponentData
    {
        protected override void Awake()
        {
            base.Awake();

            ClientNetworkService.Instance.ComponentDataReceivedEvent += OnComponentDataReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientNetworkService.Instance.ComponentDataReceivedEvent -= OnComponentDataReceived;
        }

        private void OnComponentDataReceived(NetworkComponentData obj)
        {
            T data = (T)obj;

            if (data.componentID != ID)
            {
                return;
            }

            OnDataReceived(data);
        }

        protected abstract void OnDataReceived(T data);
    }
}
