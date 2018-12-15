using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.Console
{
    public class Console : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
//        [SerializeField] private TextMeshProUGUI log

        private void Start()
        {
            ShowConsole();
        }

        public void ShowConsole()
        {
            FocusInputField();


        }

        public void HideConsole()
        {
            StopCoroutine(ListenToEnterInput());
        }

        private void FocusInputField()
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            inputField.OnPointerClick(new PointerEventData(EventSystem.current));

            StartCoroutine(ListenToEnterInput());
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

            FocusInputField();
        }
    }
}
