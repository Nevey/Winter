using System;
using Game.Gameplay.Controls.Services;
using UnityEngine;

namespace Scripts.Gameplay.Controls
{
    public class MouseInput : MonoBehaviour
    {
        // Private
        private const string MOUSE_AXIS_X = "Mouse X";
        private const string MOUSE_AXIS_Y = "Mouse Y";

        private Vector2 currentMouseInput;

        // Public
        public event Action<Vector2> MouseInputEvent;

        private void Awake()
        {
            currentMouseInput = new Vector2(Input.GetAxis(MOUSE_AXIS_X), Input.GetAxis(MOUSE_AXIS_Y));

            ControlsService.Instance.RegisterMouseInput(this);
        }

        private void OnDestroy()
        {
            ControlsService.Instance.UnregisterMouseInput(this);
        }

        private void Update()
        {
            currentMouseInput.x = Input.GetAxis(MOUSE_AXIS_X);
            currentMouseInput.y = Input.GetAxis(MOUSE_AXIS_Y);

            MouseInputEvent?.Invoke(currentMouseInput);
        }
    }
}
