using UnityEngine;

namespace Game.Gameplay.Actors.Components
{
    public class ActorComponent : MonoBehaviour
    {
        protected int ownerID;

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
    }
}
