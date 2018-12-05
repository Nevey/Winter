using System.Collections.Generic;
using Game.Services;

namespace Game.Gameplay.Actors.Components.Services
{
    public class ServerActorComponentService : Service<ServerActorComponentService>
    {
        private readonly List<ServerActorComponent> actorComponents = new List<ServerActorComponent>();

        public void RegisterActorComponent(ServerActorComponent actorComponent)
        {
            if (actorComponents.Contains(actorComponent))
            {
                return;
            }

            actorComponents.Add(actorComponent);
        }

        public void UnregisterActorComponent(ServerActorComponent actorComponent)
        {
            if (!actorComponents.Contains(actorComponent))
            {
                return;
            }

            actorComponents.Remove(actorComponent);
        }
    }
}
