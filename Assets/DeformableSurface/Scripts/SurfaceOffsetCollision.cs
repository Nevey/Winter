using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DS
{
    public class SurfaceOffsetCollision : MonoBehaviour
    {
        #region Public
        public float Offset { get; private set; }

        public UnityEvent OnEnter;
        public UnityEvent OnStay;
        public UnityEvent OnExit;
        #endregion

        #region Private
        private float _lastOffset = -1;
        #endregion


        void Awake()
        {
            Offset = _lastOffset;
        }

        void Update()
        {
            DeformableSurface[] colDs = DeformableSurface.Instances
                .Where(ds => ds.MeshRenderer.bounds.Contains(transform.position)).ToArray();

            if (!colDs.Any())
                return;
            
            Offset = colDs.First().GetCollisionOffset(transform.position);

            if (_lastOffset < 0 && Offset >= 0)
                OnEnter.Invoke();

            if (Offset >= 0f)
                OnStay.Invoke();

            if (_lastOffset >= 0 && Offset < 0)
                OnExit.Invoke();

            _lastOffset = Offset;
        }
    }
}

