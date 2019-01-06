using UnityEngine;

namespace DS.Example
{
    public class MouseOrbit : MonoBehaviour
    {
        public Transform Target;

        public bool LockCursor = true;
        public float Distance = 5.0f;
        public Vector2 Sensitivity = new Vector2(120, 120);
        public Vector2 YMinMax = new Vector2(-20, 80);
        public Vector2 DistanceMinMax = new Vector2(5, 15);

        private Vector2 _xy;

        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            _xy = new Vector2(angles.y, angles.x);

            if (LockCursor)
                Cursor.lockState = CursorLockMode.Locked;
        }

        void LateUpdate()
        {
            if (Target)
            {
                _xy.x += Input.GetAxis("Mouse X") * Sensitivity.x * Distance * 0.02f;
                _xy.y -= Input.GetAxis("Mouse Y") * Sensitivity.y * 0.02f;

                _xy.y = ClampAngle(_xy.y, YMinMax.x, YMinMax.y);

                Quaternion rotation = Quaternion.Euler(_xy.y, _xy.x, 0);
                Distance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel") * 5, DistanceMinMax.x, DistanceMinMax.y);

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -Distance);
                Vector3 position = rotation * negDistance + Target.position;

                transform.rotation = rotation;
                transform.position = position;
            }
        }

        float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }

}
