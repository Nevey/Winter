using System;
using Game.Network.Data;

namespace Game.Gameplay.Actors.Components
{
    public abstract class NetworkComponent<T> : ActorComponent where T : NetworkComponentData
    {
        public override Type DataFormatType => typeof(T);

        protected abstract void OnReceiveData(T networkComponentData);

        public override void ReceiveData(NetworkComponentData data)
        {
            OnReceiveData((T)data);
        }
    }
}
