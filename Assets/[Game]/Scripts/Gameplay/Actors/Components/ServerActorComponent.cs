using Game.Gameplay.Actors.Components.Services;

namespace Game.Gameplay.Actors.Components
{
    public abstract class ServerActorComponent : ActorComponent
    {
        protected override void Awake()
        {
            base.Awake();

            ServerActorComponentService.Instance.RegisterActorComponent(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerActorComponentService.Instance.UnregisterActorComponent(this);
        }
    }
}
