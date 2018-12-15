using System;
using Game.UserInput.Services;
using UnityEngine;

namespace Game.UserInput
{
    public class KeyboardInput : MonoBehaviour
    {
        // Private
        private const string HORIZONTAL_AXIS = "Horizontal";
        private const string VERTICAL_AXIS = "Vertical";
        private const string JUMP_AXIS = "Jump";
        private const string CROUCH_AXIS = "Crouch";
        private const string SPECTATOR_CAMERA_UP = "Spectator Camera Up";
        private const string SPECTATOR_CAMERA_DOWN = "Spectator Camera Down";
        private const string TOGGLE_CONSOLE = "Toggle Console";

        // Public
        public event Action<float> HorizontalInputEvent;
        public event Action<float> VerticalInputEvent;
        public event Action<float> JumpInputEvent;
        public event Action<float> CrouchInputEvent;
        public event Action<float> SpectatorCameraUpEvent;
        public event Action<float> SpectatorCameraDownEvent;
        public event Action<float> ToggleConsoleEvent;

        private void Awake()
        {
            InputService.Instance.RegisterKeyboardInput(this);
        }

        private void OnDestroy()
        {
            InputService.Instance.UnregisterKeyboardInput(this);
        }

        private void Update()
        {
            // TODO: Instead of continuously sending events, pick specific events which don't need to
            // be sent if input is zero
            HorizontalInputEvent?.Invoke(Input.GetAxis(HORIZONTAL_AXIS));
            VerticalInputEvent?.Invoke(Input.GetAxis(VERTICAL_AXIS));
            JumpInputEvent?.Invoke(Input.GetAxis(JUMP_AXIS));
            CrouchInputEvent?.Invoke(Input.GetAxis(CROUCH_AXIS));
            SpectatorCameraUpEvent?.Invoke(Input.GetAxis(SPECTATOR_CAMERA_UP));
            SpectatorCameraDownEvent?.Invoke(Input.GetAxis(SPECTATOR_CAMERA_DOWN));
            ToggleConsoleEvent?.Invoke(Input.GetAxis(TOGGLE_CONSOLE));
        }
    }
}
