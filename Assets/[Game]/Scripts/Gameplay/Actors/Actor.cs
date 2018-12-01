using UnityEngine;

namespace Game.Gameplay.Actors
{
    public abstract class Actor : MonoBehaviour
    {
        // TODO: Create View/ActorView component
        // Private
        [SerializeField] private GameObject view;

        /// <summary>
        /// Client ID of the owner
        /// </summary>
        protected int ownerID;

        protected virtual void Awake()
        {
            Instantiate(view, transform);
        }

        protected virtual void OnDestroy()
        {

        }

        public void Initialize(int ownerID)
        {
            this.ownerID = ownerID;
            OnInitialized();
        }

        protected abstract void OnInitialized();
    }
}
