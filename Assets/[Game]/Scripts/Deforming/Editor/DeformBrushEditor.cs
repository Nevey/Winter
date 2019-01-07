using Game.Utilities;
using UnityEditor;
using UnityEngine;

namespace Game.Building.Editor
{
    [CustomEditor(typeof(DeformBrush))]
    public class DeformBrushEditor : UnityEditor.Editor
    {
        private DeformBrush deformBrush;

        private void OnEnable()
        {
            deformBrush = (DeformBrush)target;
        }

        private void OnSceneGUI()
        {
            Camera sceneViewCamera = GetSceneViewCamera();

            if (sceneViewCamera == null)
            {
                return;
            }
            
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            
            Event e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                Vector2? textureCoord = GetTextureCoord(sceneViewCamera);

                if (textureCoord == null)
                {
                    return;
                }
                
                deformBrush.DrawStuff(textureCoord.Value);
            }
        }
        
        private Camera GetSceneViewCamera()
        {
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                return SceneView.lastActiveSceneView.camera;
            }

            return null;
        }

        private Vector2? GetTextureCoord(Camera camera)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // TODO: Cache meshCollider
            MeshCollider meshCollider = deformBrush.GetComponent<MeshCollider>();

            if (meshCollider.Raycast(mouseRay, out RaycastHit hit, float.MaxValue))
            {
                return hit.textureCoord;
            }

            return null;
        }
        
        private Vector3 GetPointerPosition(Camera camera)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // TODO: Cache meshCollider
            MeshCollider meshCollider = deformBrush.GetComponent<MeshCollider>();
            
            if (meshCollider.Raycast(mouseRay, out RaycastHit hit, float.MaxValue))
                return hit.point;

            // TODO: Check if this is needed?
            Plane plane = new Plane(Vector3.up, deformBrush.transform.position);
            float distance;
            plane.Raycast(mouseRay, out distance);

            return camera.transform.position + mouseRay.direction * distance;
        }
    }
}