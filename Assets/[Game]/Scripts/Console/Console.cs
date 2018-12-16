using System.Collections;
using Game.UserInput.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.Console
{
    public class Console : MonoBehaviour
    {
        [SerializeField] private InputField inputField;

        private void Awake()
        {
            InputService.Instance.ToggleConsoleEvent += OnToggleConsole;

            gameObject.SetActive(false);
            HideConsole();
        }

        private void OnDestroy()
        {
            InputService.Instance.ToggleConsoleEvent -= OnToggleConsole;
        }

        public void OnToggleConsole()
        {
            gameObject.SetActive(!gameObject.activeSelf);

            if (gameObject.activeSelf)
            {
                ShowConsole();
            }
            else
            {
                HideConsole();
            }
        }

        private void ShowConsole()
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            inputField.OnPointerClick(new PointerEventData(EventSystem.current));

            StartCoroutine(ListenToEnterInput());
        }

        public void HideConsole()
        {
            StopCoroutine(ListenToEnterInput());
        }

        private IEnumerator ListenToEnterInput()
        {
            bool isEnterPressed = false;

            while (!isEnterPressed)
            {
                yield return null;

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SubmitConsoleInpput();

                    isEnterPressed = true;
                }
            }
        }

        private void SubmitConsoleInpput()
        {
            // try to execute inputField.text
            inputField.text = "";

            ShowConsole();
        }
    }
}
