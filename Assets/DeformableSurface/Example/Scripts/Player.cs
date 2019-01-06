#pragma warning disable 0649

using UnityEngine;

namespace DS.Example
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Renderer))]
    public class Player : MonoBehaviour
    {
        #region Public
        public float MoveSpeed = 5;
        public float ShiftForce = 25;
        public float JumpForce = 5;

        //Velocity loss multiplier on offset collision
        [Range(0, 1)] public float VelocityLossOnCollision = 0.5f;
        #endregion

        #region Refs
        [SerializeField] private SurfaceOffsetCollision _offsetCollision;

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Renderer _renderer;
        #endregion

        private Color _noCollisionColor = Color.green;
        private Color _collisionColor = Color.red;
        private float _lastCollisionOffset = 0;


        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<Renderer>();
        }

        void Update()
        {
            Move();
            Jump();

            AdjustOffsetCollisionPos();
            UpdateColor();
        }

        void Move()
        {
            Vector3 direction;

            if (Camera.main != null)
            {
                Vector3 hor = Camera.main.transform.TransformDirection(Vector3.right) * Input.GetAxis("Horizontal");
                Vector3 vert = Camera.main.transform.TransformDirection(Vector3.forward) * Input.GetAxis("Vertical");
                direction = (hor + vert).normalized;
            }
            else direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            float force = MoveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                force += ShiftForce;

            _rigidbody.AddForce(direction * force);
        }

        void Jump()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            _rigidbody.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
        }


        //We need to update the _offsetCollision position in order to get collision at bottom point of the player
        void AdjustOffsetCollisionPos()
        {
            _offsetCollision.transform.position = new Vector3(_collider.bounds.center.x, _collider.bounds.min.y + 0.1f, _collider.bounds.center.z);
        }

        //Update player color based on _offsetCollision.Offset value
        void UpdateColor()
        {
            _lastCollisionOffset = Mathf.Lerp(_lastCollisionOffset, Mathf.Clamp01(_offsetCollision.Offset),
                Time.deltaTime * 2);

            _renderer.material.color = Color.Lerp(_noCollisionColor, _collisionColor, _lastCollisionOffset);
        }


        //This function is called by SurfaceOffsetCollision component
        public void LimitVelocity()
        {
            //Multiply current rigidbody velocity to Offset value that comes from SurfaceOffsetCollision
            Vector3 tarVelocity = _rigidbody.velocity * (1 - _offsetCollision.Offset);

            //Smoothly change the velocity based VelocityLossOnCollision
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, tarVelocity, VelocityLossOnCollision);
        }
    }
}

