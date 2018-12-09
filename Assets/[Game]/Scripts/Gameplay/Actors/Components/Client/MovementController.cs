using Game.Gameplay.Camera;
using Game.Gameplay.Controls.Services;
using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    public class MovementController : MonoBehaviour
    {
        [Header("Required References")]
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;

        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 1f;
        [SerializeField] private float movementSmoothWeight = 0.2f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSmoothWeight = 0.1f;
        [SerializeField] private float maxRotationSpeed = 500f;

        private Vector3 horizontalVector;
        private Vector3 targetHorizontalVector;
        private Vector3 horizontalVectorVelocity;

        private Vector3 verticalVector;
        private Vector3 targetVerticalVector;
        private Vector3 verticalVectorVelocity;

        private Vector3 targetVerticalEuler;
        private float rotationVelocity;


        private void Awake()
        {
            ControlsService.Instance.HorizontalInputEvent += OnHorizontalInput;
            ControlsService.Instance.VerticalInputEvent += OnVerticalInput;
        }

        private void OnDestroy()
        {
            ControlsService.Instance.HorizontalInputEvent -= OnHorizontalInput;
            ControlsService.Instance.VerticalInputEvent -= OnVerticalInput;
        }

        private void OnHorizontalInput(float obj)
        {
            targetHorizontalVector =
                thirdPersonCamera.transform.right * obj * movementSpeed * Time.deltaTime;
        }

        private void OnVerticalInput(float inputDelta)
        {
            targetVerticalVector =
                thirdPersonCamera.transform.forward * inputDelta * movementSpeed * Time.deltaTime;

            targetVerticalVector.y = 0f;

            targetVerticalEuler =
                inputDelta > 0f ? thirdPersonCamera.transform.eulerAngles : Vector3.zero;

            UpdateMovement();
            UpdateRotation();
        }

        private void UpdateMovement()
        {
            verticalVector = Vector3.SmoothDamp(verticalVector, targetVerticalVector,
                ref verticalVectorVelocity, movementSmoothWeight);

            horizontalVector = Vector3.SmoothDamp(horizontalVector, targetHorizontalVector,
                ref horizontalVectorVelocity, movementSmoothWeight);

            transform.position += verticalVector + horizontalVector;

            // TODO: Clamp magnitude
        }

        private void UpdateRotation()
        {
            Vector3 targetRotation = targetVerticalEuler;

            Quaternion desiredRotation = targetRotation == Vector3.zero
                ? transform.rotation
                : Quaternion.Euler(targetRotation);

            float angle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y,
                desiredRotation.eulerAngles.y, ref rotationVelocity, rotationSmoothWeight,
                maxRotationSpeed);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
