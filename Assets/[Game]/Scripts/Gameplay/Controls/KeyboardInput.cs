using System;
using Game.Gameplay.Controls.Services;
using UnityEngine;

namespace Scripts.Gameplay.Controls
{
    public class KeyboardInput : MonoBehaviour
    {
        // Private
        private const string HORIZONTAL_AXIS = "Horizontal";
        private const string VERTICAL_AXIS = "Vertical";
        private const string JUMP_AXIS = "Jump";
        private const string CROUCH_AXIS = "Crouch";

        // Public
        public event Action<float> HorizontalInputEvent;
        public event Action<float> VerticalInputEvent;
        public event Action<float> JumpInputEvent;
        public event Action<float> CrouchInputEvent;

        private void Awake()
        {
            ControlsService.Instance.RegisterKeyboardInput(this);
        }

        private void OnDestroy()
        {
            ControlsService.Instance.UnregisterKeyboardInput(this);
        }

        private void Update()
        {
            HorizontalInputEvent?.Invoke(Input.GetAxis(HORIZONTAL_AXIS));
            VerticalInputEvent?.Invoke(Input.GetAxis(VERTICAL_AXIS));
            JumpInputEvent?.Invoke(Input.GetAxis(JUMP_AXIS));
            CrouchInputEvent?.Invoke(Input.GetAxis(CROUCH_AXIS));
        }
    }
}
