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

        // Public
        public event Action<float> HorizontalInputEvent;
        public event Action<float> VerticalInputEvent;

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
        }
    }
}
