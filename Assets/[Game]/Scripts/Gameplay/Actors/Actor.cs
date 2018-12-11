using System.Collections.Generic;
using Game.Gameplay.Actors.Components;
using Game.Gameplay.Actors.Components.Client;
using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors
{
    public abstract class Actor : MonoBehaviour
    {
        // Private
        // TODO: Create custom editor for such debugging purposes
        [SerializeField] private int clientID; // This is just here for inspector visualization

        /// <summary>
        /// List of all ActorReceiverComponents registered to this Actor, they handle received data
        /// </summary>
        protected readonly List<ActorComponent> componentsList = new List<ActorComponent>();

        /// <summary>
        /// Client ID of the owner
        /// </summary>
        protected int ownerID;

        // Public
        public int OwnerID => ownerID;

        protected virtual void Awake()
        {
            // Do stuff...
        }

        protected virtual void OnDestroy()
        {
            // Do stuff...
        }

        protected T AddComponent<T>() where T : ActorComponent
        {
            T component = gameObject.AddComponent<T>();

            RegisterComponent(component);

            return component;
        }

        protected bool RemoveComponent<T>(T component) where T : ActorComponent
        {
            if (!componentsList.Contains(component))
            {
                return false;
            }

            UnregisterComponent(component);

            Destroy(component);

            return true;
        }

        private void RegisterComponent<T>(T component) where T : ActorComponent
        {
            component.Initialize(ownerID);

            componentsList.Add(component);
        }

        private void UnregisterComponent<T>(T component) where T : ActorComponent
        {
            componentsList.Remove(component);
        }

        private void RegisterExistingComponents()
        {
            ActorComponent[] actorComponents = GetComponentsInChildren<ActorComponent>();

            for (int i = 0; i < actorComponents.Length; i++)
            {
                RegisterComponent(actorComponents[i]);
            }
        }

        protected abstract void OnInitialized();

        public void Initialize(int ownerID)
        {
            this.ownerID = ownerID;

            clientID = ownerID;

            RegisterExistingComponents();

            OnInitialized();
        }

        public void Destroy()
        {
            MonoBehaviour.Destroy(gameObject);
        }
    }
}
