using System.Collections.Generic;
using Game.Services;

namespace Game.Gameplay.Actors.Components.Services
{
    public class ClientActorComponentService : Service<ClientActorComponentService>
    {
        private readonly List<ClientActorComponent> actorComponents = new List<ClientActorComponent>();

        public void RegisterActorComponent(ClientActorComponent actorComponent)
        {
            if (actorComponents.Contains(actorComponent))
            {
                return;
            }

            actorComponent.ID = actorComponents.Count;
            actorComponents.Add(actorComponent);
        }

        public void UnregisterActorComponent(ClientActorComponent actorComponent)
        {
            if (!actorComponents.Contains(actorComponent))
            {
                return;
            }

            actorComponents.Remove(actorComponent);
        }
    }
}
