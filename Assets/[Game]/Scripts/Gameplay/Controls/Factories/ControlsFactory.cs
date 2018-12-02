using Game.Gameplay.Controls;
using UnityEngine;

namespace Scripts.Gameplay.Controls.Factories
{
    public class ControlsFactory : MonoBehaviour
    {
        // TODO: Add a way to automatically detect control type based on input tester
        [SerializeField] private ControlType controlType;

        [SerializeField] private MouseInput mouseInputPrefab;

        [SerializeField] private KeyboardInput keyboardInputPrefab;

        private void Awake()
        {
            switch (controlType)
            {
                case ControlType.KeyboardMouse:
                    Instantiate(mouseInputPrefab, transform);
                    Instantiate(keyboardInputPrefab, transform);
                    break;

                case ControlType.Controller:
                    break;
            }
        }
    }
}
