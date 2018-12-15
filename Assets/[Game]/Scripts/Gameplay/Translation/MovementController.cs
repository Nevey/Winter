using System;
using Game.Gameplay.Camera;
using Game.UserInput.Services;
using UnityEngine;

namespace Game.Gameplay.Translation
{
    public class MovementController : MonoBehaviour
    {
        [Header("Required References")]
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;

        [Header("Movement Settings")]
        [SerializeField] private float horizontalMovementSpeed = 0.5f;
        [SerializeField] private float verticalMovementSpeed = 1f;
        [SerializeField] private float movementSmoothWeight = 0.2f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSmoothWeight = 0.1f;
        [SerializeField] private float maxRotationSpeed = 500f;

        private Vector2 inputDirection = Vector2.zero;

        // Horizontal movement fields
        private Vector3 horizontalVector;
        private Vector3 targetHorizontalVector;
        private Vector3 horizontalVectorVelocity;

        // Vertical movement fields
        private Vector3 verticalVector;
        private Vector3 targetVerticalVector;
        private Vector3 verticalVectorVelocity;

        // Rotation fields
        private Vector3 targetVerticalEuler;
        private float rotationVelocity;

        private void Awake()
        {
            InputService.Instance.HorizontalInputEvent += OnHorizontalInput;
            InputService.Instance.VerticalInputEvent += OnVerticalInput;
        }

        private void OnDestroy()
        {
            InputService.Instance.HorizontalInputEvent -= OnHorizontalInput;
            InputService.Instance.VerticalInputEvent -= OnVerticalInput;
        }

        private void OnHorizontalInput(float inputHorizontal)
        {
            targetHorizontalVector = thirdPersonCamera.transform.right * inputHorizontal
                                                                       * horizontalMovementSpeed
                                                                       * Time.deltaTime;

            inputDirection.x = inputHorizontal;
        }

        private void OnVerticalInput(float inputVertical)
        {
            targetVerticalVector = thirdPersonCamera.transform.forward * inputVertical
                                                                       * verticalMovementSpeed
                                                                       * Time.deltaTime;

            targetVerticalVector.y = 0f;

            inputDirection.y = inputVertical;

            UpdateMovement();
            UpdateRotation();
        }

        private void UpdateMovement()
        {
            verticalVector = Vector3.SmoothDamp(verticalVector, targetVerticalVector,
                ref verticalVectorVelocity, movementSmoothWeight);

            horizontalVector = Vector3.SmoothDamp(horizontalVector, targetHorizontalVector,
                ref horizontalVectorVelocity, movementSmoothWeight);

            Vector3 positionStep = horizontalVector + verticalVector;
            positionStep = Vector3.ClampMagnitude(positionStep, verticalMovementSpeed * Time.deltaTime);

            transform.position += positionStep;
        }

        private void UpdateRotation()
        {
            Quaternion desiredRotation = Math.Abs(inputDirection.magnitude) < 0.01f
                ? transform.rotation
                : thirdPersonCamera.transform.rotation;

            float angle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y,
                desiredRotation.eulerAngles.y, ref rotationVelocity, rotationSmoothWeight,
                maxRotationSpeed);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
