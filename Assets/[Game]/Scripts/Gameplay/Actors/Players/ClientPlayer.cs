using Game.Gameplay.Actors;
using Game.Gameplay.Actors.Components.Client;

namespace Scripts.Gameplay.Actors.Players
{
    public class ClientPlayer : ClientActor
    {
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
