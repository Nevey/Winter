using Game.Gameplay.Controls.Services;
using Game.Utilities;
using UnityEngine;

namespace Game.Gameplay.Camera
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform mainTarget;

        [Header("Look Settings")]
        [SerializeField] private Vector2 defaultLookRotation = new Vector2(35f, 0f);
        [SerializeField] private float minRotationX = -90f;
        [SerializeField] private float maxRotationX = 90f;
        [SerializeField] private float lookSmoothWeight = 0.1f;

        [Header("Sensitivity Settings")]
        [SerializeField] private Vector2 lookSensitivity = new Vector2(150f, 150f);

        private Vector2 lookRotation;
        private Vector2 targetLookRotation;
        private Vector2 lookVelocity;

        // Position fields
        private Vector3 targetPosition;
        private Vector3 positionVel;

        // Rotation fields
        private Quaternion targetRotation;
        private float xVel;
        private float yVel;
        private float zVel;

        private Vector3 offset;

        private void Awake()
        {
            if (mainTarget == null)
            {
                Log.Error("No Main Target is set!");
            }

            offset = transform.position - mainTarget.transform.position;
            lookRotation = targetLookRotation = defaultLookRotation;

            // Automatically un-parent to make sure we'll never have to fight any global rotation
            // being set due to parenting
            if (transform.parent != null)
            {
                transform.parent = null;
            }

            ControlsService.Instance.LookInputEvent += OnLookInput;
        }

        private void OnDestroy()
        {
            ControlsService.Instance.LookInputEvent -= OnLookInput;
        }

        private void OnLookInput(Vector2 input)
        {
            if (mainTarget == null)
            {
                return;
            }

            targetLookRotation.x += input.y * lookSensitivity.y * Time.deltaTime;
            targetLookRotation.y += input.x * lookSensitivity.x * Time.deltaTime;

            targetLookRotation.x = ClampUtility.Angle(targetLookRotation.x, minRotationX, maxRotationX);

            lookRotation = Vector2.SmoothDamp(lookRotation, targetLookRotation, ref lookVelocity,
                lookSmoothWeight);

            Quaternion xQuaternion = Quaternion.AngleAxis(lookRotation.y, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(lookRotation.x, Vector3.left);

            transform.rotation = xQuaternion * yQuaternion;

            // Create a matrix based on camera pivot position and target rotation
            Matrix4x4 matrix =
                Matrix4x4.TRS(mainTarget.transform.position, transform.rotation, Vector3.one);

            transform.position = matrix.MultiplyPoint3x4(offset);
        }
    }
}
