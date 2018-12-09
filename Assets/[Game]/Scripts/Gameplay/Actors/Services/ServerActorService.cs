using System.Collections.Generic;
using DarkRift.Server;
using Game.Factories;
using Game.Network.Data;
using Game.Services;

namespace Game.Gameplay.Actors.Services
{
    /// <summary>
    /// Server-side service which knows about all actors on this instance
    /// </summary>
    public class ServerActorService : Service<ServerActorService>
    {
        // Private
        private readonly List<Actor> actors = new List<Actor>();

        private ServerActorFactory actorFactory;

        // Public
        public List<Actor> Actors => actors;

        public void RegisterActorFactory(ServerActorFactory actorFactory)
        {
            if (this.actorFactory != null)
            {
                return;
            }

            this.actorFactory = actorFactory;
        }

        public void UnregisterActorFactory(ServerActorFactory actorFactory)
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
            actorFactory.SpawnPlayer(spawnData);
        }

        public void DestroyActorsOfClient(IClient client)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                Actor actor = actors[i];

                if (actor.OwnerID == client.ID)
                {
                    actor.Destroy();
                }
            }
        }
    }
}
