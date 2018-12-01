using Game.Gameplay.Actors;
using Scripts.Gameplay.Actors.Players.Services;

namespace Scripts.Gameplay.Actors.Players
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
    }
}
