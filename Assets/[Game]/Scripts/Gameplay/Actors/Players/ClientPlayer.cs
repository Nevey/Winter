using Game.Gameplay.Actors;
using Game.Gameplay.Actors.Components.Client;
using Scripts.Gameplay.Actors.Players.Services;

namespace Scripts.Gameplay.Actors.Players
{
    public class ClientPlayer : ClientActor
    {
        protected override void Awake()
        {
            base.Awake();

            ClientPlayerService.Instance.RegisterPlayer(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClientPlayerService.Instance.UnregisterPlayer(this);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (IsMine)
            {
                AddComponent<InputMovement>();
            }
        }
    }
}
