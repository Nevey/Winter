using System;
using Game.Services;
using Game.Utilities;
using Scripts.Gameplay.Controls;
using UnityEngine;

namespace Game.Gameplay.Controls.Services
{
    public class ControlsService : Service<ControlsService>
    {
        // Private
        private bool isInputPaused;

        private MouseInput mouseInput;

        private KeyboardInput keyboardInput;

        // Public
        public event Action<Vector2> MouseInputEvent;

        public event Action<float> HorizontalInputEvent;

        public event Action<float> VerticalInputEvent;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #region Handle Input

        private void OnMouseInput(Vector2 vector2)
        {
            if (isInputPaused)
            {
                return;
            }

            MouseInputEvent?.Invoke(vector2);

            Log.Write($"Mouse Input | X: {vector2.x} - Y: {vector2.y}");
        }

        private void OnHorizontalInput(float horizontalInput)
        {
            if (isInputPaused)
            {
                return;
            }

            HorizontalInputEvent?.Invoke(horizontalInput);

            Log.Write($"Horizontal Input | {horizontalInput}");
        }

        private void OnVerticalInput(float verticalInput)
        {
            if (isInputPaused)
            {
                return;
            }

            VerticalInputEvent?.Invoke(verticalInput);

            Log.Write($"Vertical Input | {verticalInput}");
        }

        #endregion

        public void RegisterMouseInput(MouseInput mouseInput)
        {
            if (this.mouseInput != null)
            {
                return;
            }

            this.mouseInput = mouseInput;
            this.mouseInput.MouseInputEvent += OnMouseInput;
        }

        public void UnregisterMouseInput(MouseInput mouseInput)
        {
            if (this.mouseInput == null || this.mouseInput != mouseInput)
            {
                return;
            }

            this.mouseInput.MouseInputEvent -= OnMouseInput;
            this.mouseInput = null;
        }

        public void RegisterKeyboardInput(KeyboardInput keyboardInput)
        {
            if (this.keyboardInput != null)
            {
                return;
            }

            this.keyboardInput = keyboardInput;
            this.keyboardInput.HorizontalInputEvent += OnHorizontalInput;
            this.keyboardInput.VerticalInputEvent += OnVerticalInput;
        }

        public void UnregisterKeyboardInput(KeyboardInput keyboardInput)
        {
            if (this.keyboardInput == null || this.keyboardInput != keyboardInput)
            {
                return;
            }

            this.keyboardInput.HorizontalInputEvent -= OnHorizontalInput;
            this.keyboardInput.VerticalInputEvent -= OnVerticalInput;
            this.keyboardInput = null;
        }

        public void RegisterControllerInput()
        {

        }

        public void UnregisterControllerInput()
        {

        }

        public void PauseInput()
        {
            isInputPaused = true;
        }

        public void UnpauseInput()
        {
            isInputPaused = false;
        }
    }
}
