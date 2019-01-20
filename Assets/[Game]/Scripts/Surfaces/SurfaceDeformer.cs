using UnityEngine;

namespace Game.Surfaces
{
    public class SurfaceDeformer : MonoBehaviour
    {
        [SerializeField, Range(0, 500)] private float brushSize = 1f;

        [SerializeField, Range(0, 1)] private float brushOpacity = 1f;

        // Default layerMask is "Ground"
        [SerializeField] private LayerMask layerMask = 1 << 9;

        private void FixedUpdate()
        {
            RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, Vector3.down, 10f, layerMask);

            for (int i = 0; i < raycastHits.Length; i++)
            {
                SurfacePainter surfacePainter = raycastHits[i].transform.GetComponent<SurfacePainter>();

                if (surfacePainter == null)
                {
                    continue;
                }
                
                Vector2 textureCoords = new Vector2(raycastHits[i].textureCoord.x, raycastHits[i].textureCoord.y);
                
                surfacePainter.EraseDeform(textureCoords, brushSize, brushOpacity);
            }
        }
    }
}