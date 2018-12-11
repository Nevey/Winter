using Game.Gameplay.Actors;
using Game.Network.Data;
using Game.Utilities;
using Scripts.Gameplay.Players.Services;

namespace Scripts.Gameplay.Players
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
            Log.Write($"Owner with ID <{ownerID}> Initialized this ClientPlayer");
        }
    }
}
