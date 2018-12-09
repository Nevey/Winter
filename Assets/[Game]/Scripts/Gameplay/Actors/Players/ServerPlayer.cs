using Game.Gameplay.Actors;
using Game.Gameplay.Actors.Components.Server;
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

        protected override void OnInitialized()
        {
            // TODO: Do this via unity prefabs
            AddComponent<ServerPositionSyncer>();
        }
    }
}
