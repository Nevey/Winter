using System;
using System.Collections.Generic;
using Game.Gameplay.Actors.Components;
using Game.Network.Data;
using UnityEngine;

namespace Game.Gameplay.Actors
{
    public abstract class Actor : MonoBehaviour
    {
        // Private
        // TODO: Create View/ActorView component
        [SerializeField] private GameObject view;

        // TODO: Create custom editor for such debugging purposes
        [SerializeField] private int clientID; // This is just here for inspector visualization

        /// <summary>
        /// Client ID of the owner
        /// </summary>
        protected int ownerID;

        /// <summary>
        /// List of all ActorComponents registered to this Actor
        /// </summary>
        protected readonly List<ActorComponent> componentsList = new List<ActorComponent>();

        // Public
        public int OwnerID => ownerID;

        protected virtual void Awake()
        {
            // Instantiate ActorView and parent to this transform
            Instantiate(view, transform);
        }

        protected virtual void OnDestroy()
        {

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
            ActorComponent[] components = GetComponentsInChildren<ActorComponent>();

            for (int i = 0; i < components.Length; i++)
            {
                RegisterComponent(components[i]);
            }
        }

        protected abstract void OnInitialized();

        protected void ReceiveData(NetworkComponentData obj)
        {
            Type dataFormatType = obj.GetType();

            for (int i = 0; i < componentsList.Count; i++)
            {
                ActorComponent component = componentsList[i];

                if (component.DataFormatType == dataFormatType)
                {
                    component.ReceiveData(obj);
                }
            }
        }

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
