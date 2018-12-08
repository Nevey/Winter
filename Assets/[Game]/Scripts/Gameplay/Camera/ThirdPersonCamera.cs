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
        [SerializeField] private float movementSmoothWeight = 0.05f;
        [SerializeField] private float rotationSmoothWeight = 0.1f;

        [Header("Sensitivity Settings")]
        [SerializeField] private Vector2 lookSensitivity = new Vector2(150f, 150f);

        private Vector2 lookRotation;

        // Position fields
        private Vector3 targetPosition;
        private Vector3 positionVel;

        // Rotation fields
        private Quaternion targetRotation;
        private float xVel;
        private float yVel;
        private float zVel;

        private void Awake()
        {
            if (mainTarget == null)
            {
                Log.Error("No Main Target is set!");
            }

            ControlsService.Instance.LookInputEvent += OnLookInput;
        }

        private void OnDestroy()
        {
            ControlsService.Instance.LookInputEvent -= OnLookInput;
        }

        private void LateUpdate()
        {
            if (mainTarget == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
                ref positionVel, movementSmoothWeight);

            Vector3 euler = transform.eulerAngles;

            euler.x = Mathf.SmoothDampAngle(euler.x, targetRotation.eulerAngles.x, ref xVel,
                rotationSmoothWeight);
            euler.y = Mathf.SmoothDampAngle(euler.y, targetRotation.eulerAngles.y, ref yVel,
                rotationSmoothWeight);
            euler.z = Mathf.SmoothDampAngle(euler.z, targetRotation.eulerAngles.z, ref zVel,
                rotationSmoothWeight);

            transform.rotation = Quaternion.Euler(euler);
        }

        private void OnLookInput(Vector2 obj)
        {
            if (mainTarget == null)
            {
                return;
            }

            lookRotation.x += obj.y * lookSensitivity.y * Time.deltaTime;
            lookRotation.y += obj.x * lookSensitivity.x * Time.deltaTime;

            lookRotation.x = ClampUtility.Angle(lookRotation.x, minRotationX, maxRotationX);

            Quaternion xQuaternion = Quaternion.AngleAxis(lookRotation.y, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(lookRotation.x, Vector3.left);

            targetRotation = xQuaternion * yQuaternion;

            // Create a matrix based on camera pivot position and target rotation
            Matrix4x4 matrix =
                Matrix4x4.TRS(mainTarget.transform.position, transform.rotation, Vector3.one);

            Vector3 offsetPosition = mainTarget.transform.position;
            offsetPosition.z -= 5f;

            targetPosition = matrix.MultiplyPoint3x4(offsetPosition);
        }

        public void OverrideMainTarget(Transform mainTarget)
        {
            this.mainTarget = mainTarget;
        }
    }
}
