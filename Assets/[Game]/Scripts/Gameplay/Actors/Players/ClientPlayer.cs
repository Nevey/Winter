using Game.Gameplay.Actors;
using Game.Gameplay.Camera;
using Game.Utilities;
using Scripts.Gameplay.Actors.Players.Services;
using UnityEngine;

namespace Scripts.Gameplay.Actors.Players
{
    public class ClientPlayer : ClientActor
    {
        [SerializeField] private ThirdPersonCamera thirdPersonCameraPrefab;

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

            if (IsMine)
            {
//                ThirdPersonCamera thirdPersonCamera = Instantiate(thirdPersonCameraPrefab);
//                thirdPersonCamera.OverrideMainTarget(transform);
            }
        }
    }
}
