using Game.Gameplay.Controls.Services;
using Game.Utilities;
using UnityEngine;

namespace Game.Gameplay.Camera
{
    public class SpectatorCamera : MonoBehaviour
    {
        [Header("Camera Look Settings")]
        [SerializeField] private float minRotationX = -90f;
        [SerializeField] private float maxRotationX = 90f;

        [SerializeField] private Vector2 lookSensitivity = new Vector2(150f, 150f);

        [Header("Camera Move Settings")]
        [SerializeField] private float moveSensitivity = 0.1f;
        [SerializeField] private float moveSmoothStrength = 0.1f;

        private Vector2 lookRotation;

        private Vector3 horizontalDelta;
        private Vector3 verticalDelta;
        private Vector3 moveDelta;
        private Vector3 moveVelocity;

        private void Awake()
        {
            horizontalDelta = transform.position;

            ControlsService.Instance.MouseInputEvent += OnMouseInput;
            ControlsService.Instance.HorizontalInputEvent += OnHorizontalInput;
            ControlsService.Instance.VerticalInputEvent += OnVerticalInput;
        }

        private void OnDestroy()
        {
            ControlsService.Instance.MouseInputEvent -= OnMouseInput;
            ControlsService.Instance.HorizontalInputEvent -= OnHorizontalInput;
            ControlsService.Instance.VerticalInputEvent -= OnVerticalInput;
        }

        private void Update()
        {
            moveDelta = Vector3.SmoothDamp(moveDelta, (horizontalDelta + verticalDelta),
                ref moveVelocity, moveSmoothStrength);

            transform.position += moveDelta;
        }

        private void OnMouseInput(Vector2 mouseInput)
        {
            lookRotation.x += mouseInput.y * lookSensitivity.y * Time.deltaTime;
            lookRotation.y += mouseInput.x * lookSensitivity.x * Time.deltaTime;

            lookRotation.x = ClampUtility.Angle(lookRotation.x, minRotationX, maxRotationX);

            Quaternion xQuaternion = Quaternion.AngleAxis(lookRotation.y, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(lookRotation.x, Vector3.left);

            transform.rotation = xQuaternion * yQuaternion;
        }

        private void OnHorizontalInput(float horizontalInput)
        {
            horizontalDelta = transform.right * horizontalInput * moveSensitivity;
        }

        private void OnVerticalInput(float verticalInput)
        {
            verticalDelta = transform.forward * verticalInput * moveSensitivity;
        }
    }
}
