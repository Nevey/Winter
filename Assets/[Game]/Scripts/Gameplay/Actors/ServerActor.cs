using Game.Gameplay.Actors.Components.Server;
using Game.Gameplay.Actors.Services;

namespace Game.Gameplay.Actors
{
    public class ServerActor : Actor
    {
        protected override void Awake()
        {
            base.Awake();

            ServerActorService.Instance.RegisterActor(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerActorService.Instance.UnregisterActor(this);
        }

        protected override void OnInitialized()
        {
            gameObject.AddComponent<PositionReceiver>();
        }
    }
}
