using Game.Network.Data;
using Game.Network.Services;

namespace Game.Gameplay.Actors.Components.Server
{
    public abstract class ServerNetworkComponent<T> : ServerActorComponent where T : NetworkComponentData
    {
        protected override void Awake()
        {
            base.Awake();

            ServerNetworkService.Instance.ComponentDataReceivedEvent += OnComponentDataReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerNetworkService.Instance.ComponentDataReceivedEvent -= OnComponentDataReceived;
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
