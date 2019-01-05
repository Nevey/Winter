using UnityEngine;

namespace Game.Gameplay.Camera
{
    public class DepthRenderCamera : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<UnityEngine.Camera>().depthTextureMode = DepthTextureMode.Depth;
        }
    }
}