using System.Collections.Generic;
using Game.Factories;
using Game.Network.Data;
using Game.Services;

namespace Game.Gameplay.Actors.Services
{
    public class ClientActorService : Service<ClientActorService>
    {
        // Private
        private readonly List<Actor> actors = new List<Actor>();

        private ClientActorFactory actorFactory;

        // Public
        public List<Actor> Actors => actors;

        public void RegisterActorFactory(ClientActorFactory actorFactory)
        {
            if (this.actorFactory != null)
            {
                return;
            }

            this.actorFactory = actorFactory;
        }

        public void UnregisterActorFactory(ClientActorFactory actorFactory)
        {
            if (this.actorFactory == null || this.actorFactory != actorFactory)
            {
                return;
            }

            this.actorFactory = null;
        }

        public void RegisterActor(Actor actor)
        {
            if (actors.Contains(actor))
            {
                return;
            }

            actors.Add(actor);
        }

        public void UnregisterActor(Actor actor)
        {
            if (!actors.Contains(actor))
            {
                return;
            }

            actors.Remove(actor);
        }

        public void SpawnActor(SpawnData spawnData)
        {
            // TODO: Based on spawn data, spawn specific actor
            actorFactory.SpawnPlayer(spawnData);
        }

        public void DestroyActor(DestroyData destroyData)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                // TODO: Also compare actor ID from DestroyData
                if (actors[i].OwnerID == destroyData.ownerID)
                {
                    actors[i].Destroy();
                }
            }
        }
    }
}
