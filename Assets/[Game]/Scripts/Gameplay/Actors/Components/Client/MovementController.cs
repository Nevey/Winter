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

        private Vector3 verticalVector;
        private Vector3 targetVerticalVector;
        private Vector3 verticalVectorVelocity;

        private Vector3 verticalEuler;
        private Vector3 targetVerticalEuler;
        private Vector3 verticalEulerVelocity;

        private void Awake()
        {
            verticalEuler = targetVerticalEuler = thirdPersonCamera.transform.forward;
            
            ControlsService.Instance.VerticalInputEvent += OnVerticalInput;
        }

        private void OnDestroy()
        {
            ControlsService.Instance.VerticalInputEvent -= OnVerticalInput;
        }

        private void OnVerticalInput(float inputDelta)
        {
            targetVerticalVector =
                thirdPersonCamera.transform.forward * inputDelta * movementSpeed * Time.deltaTime;

            targetVerticalVector.y = 0f;

            if (inputDelta > 0f)
            {
                targetVerticalEuler = thirdPersonCamera.transform.forward;
                targetVerticalEuler.y = 0f;
            }

            UpdateMovement();
            UpdateRotation();
        }

        private void UpdateMovement()
        {
            verticalVector = Vector3.SmoothDamp(verticalVector, targetVerticalVector,
                ref verticalVectorVelocity, movementSmoothWeight);

            transform.position += verticalVector;
        }

        private void UpdateRotation()
        {
            verticalEuler = Vector3.SmoothDamp(verticalEuler, targetVerticalEuler,
                ref verticalEulerVelocity, rotationSmoothWeight);

            transform.rotation = Quaternion.LookRotation(verticalEuler);
        }
    }
}
