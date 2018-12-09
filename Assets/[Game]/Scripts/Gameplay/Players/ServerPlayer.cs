using Game.Gameplay.Actors;
using Scripts.Gameplay.Players.Services;

namespace Scripts.Gameplay.Players
{
    public class ServerPlayer : ServerActor
    {
        protected override void Awake()
        {
            base.Awake();

            ServerPlayerService.Instance.RegisterPlayer(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServerPlayerService.Instance.UnregisterPlayer(this);
        }

        protected override void OnInitialized()
        {

        }
    }
}
