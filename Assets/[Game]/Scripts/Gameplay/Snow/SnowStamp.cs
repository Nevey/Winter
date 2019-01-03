using UnityEngine;

namespace Game.Gameplay.Snow
{
    // TODO: Add support for stamp with textures
    public class SnowStamp : MonoBehaviour
    {
        [SerializeField, Range(0, 500)] private float brushSize;

        [SerializeField, Range(0, 1)] private float brushStrength;

        private void FixedUpdate()
        {
            SnowService.Instance.DrawStamp(transform, brushSize, brushStrength);
        }
    }
}