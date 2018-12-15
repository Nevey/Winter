using Game.UserInput;
using UnityEngine;

namespace Scripts.UserInput.Factories
{
    public class InputFactory : MonoBehaviour
    {
        // TODO: Add a way to automatically detect control type based on input tester
        [SerializeField] private InputType inputType;

        [SerializeField] private MouseInput mouseInputPrefab;

        [SerializeField] private KeyboardInput keyboardInputPrefab;

        private void Awake()
        {
            switch (inputType)
            {
                case InputType.KeyboardMouse:
                    Instantiate(mouseInputPrefab, transform);
                    Instantiate(keyboardInputPrefab, transform);
                    break;

                case InputType.Controller:
                    break;
            }
        }
    }
}
