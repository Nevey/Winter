using Game.Gameplay.Actors.Components.Services;

namespace Game.Gameplay.Actors.Components
{
    public abstract class ClientActorComponent : ActorComponent
    {
        protected override void Awake()
        {
            base.Awake();

            ClientActorComponentService.Instance.RegisterActorComponent(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientActorComponentService.Instance.UnregisterActorComponent(this);
        }
    }
}
