using System;

namespace Game.Services
{
    public abstract class Service
    {
        public abstract void SetInstance<T>(T instance) where T : Service;
        public abstract void Initialize();
    }

    public abstract class Service<T> : Service where T : Service
    {
        public static T Instance { get; private set; }

        public override void SetInstance<T1>(T1 instance)
        {
            if (instance.GetType() != typeof(T))
            {
                throw new Exception($"Incorrect types...");
            }

            Instance = instance as T;
        }

        public override void Initialize()
        {
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {

        }
    }
}
