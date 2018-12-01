using Game.Gameplay.Actors.Services;

namespace Game.Gameplay.Actors
{
    public class ClientActor : Actor
    {
        protected override void Awake()
        {
            base.Awake();

            ClientActorService.Instance.RegisterActor(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientActorService.Instance.UnregisterActor(this);
        }
    }
}
