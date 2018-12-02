using Game.Gameplay.Actors.Components;
using UnityEngine;

namespace Game.Gameplay.Actors
{
    public abstract class Actor : MonoBehaviour
    {
        // Private
        // TODO: Create View/ActorView component
        [SerializeField] private GameObject view;

        // TODO: Create custom editor for such debugging purposes
        [SerializeField] private int clientID;

        /// <summary>
        /// Client ID of the owner
        /// </summary>
        protected int ownerID;

        // Public
        public int OwnerID => ownerID;

        protected virtual void Awake()
        {
            Instantiate(view, transform);
        }

        protected virtual void OnDestroy()
        {

        }

        protected T AddComponent<T>() where T : ActorComponent
        {
            T actorComponent = gameObject.AddComponent<T>();
            actorComponent.Initialize(ownerID);

            return actorComponent;
        }

        public void Initialize(int ownerID)
        {
            this.ownerID = ownerID;

            clientID = ownerID;

            OnInitialized();
        }

        public void Destroy()
        {
            MonoBehaviour.Destroy(gameObject);
        }

        protected abstract void OnInitialized();
    }
}
