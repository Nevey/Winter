using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors.Components
{
    public abstract class ActorComponent : MonoBehaviour
    {
        protected int ownerID;

        protected Actor owner;

        public abstract System.Type DataFormatType { get; }

        protected virtual void Awake()
        {
            // Do stuff...
        }

        protected virtual void OnDestroy()
        {
            // Do stuff...
        }

        protected virtual void Update()
        {
            // Do stuff...
        }

        public void Initialize(int ownerID)
        {
            this.ownerID = ownerID;
        }

        public abstract void ReceiveData(NetworkComponentData data);
    }
}
