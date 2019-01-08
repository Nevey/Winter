using Game.Utilities;
using UnityEditor;
using UnityEngine;

namespace Game.Deforming.Editor
{
    [CustomEditor(typeof(DeformBrush))]
    public class DeformBrushEditor : UnityEditor.Editor
    {
        private DeformBrush deformBrush;

        private SerializedProperty brushSize;
        private SerializedProperty brushOpacity;

        private SerializedProperty paints;

        private SerializedProperty drawShader;
        private SerializedProperty alphaMap;

        private void OnEnable()
        {
            deformBrush = (DeformBrush)target;
            deformBrush.Initialize();
            
            brushSize = serializedObject.FindProperty("brushSize");
            brushOpacity = serializedObject.FindProperty("brushOpacity");

            paints = serializedObject.FindProperty("paints");

            drawShader = serializedObject.FindProperty("drawShader");
            alphaMap = serializedObject.FindProperty("alphaMap");
        }

        private void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            GUI.DrawTexture(new Rect(0, 0, 256, 256), (Texture)alphaMap.objectReferenceValue, ScaleMode.ScaleToFit, false, 1);

            EditorGUILayout.PropertyField(drawShader);
            EditorGUILayout.PropertyField(alphaMap);
            
            DrawBrushSettings();
            DrawLayerSettings();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBrushSettings()
        {
            EditorGUILayout.Slider(brushSize, 0, 500, "Brush Size");
            EditorGUILayout.Slider(brushOpacity, 0, 1, "Brush Opacity");
        }

        private void DrawLayerSettings()
        {
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal("box");
            
            if (GUILayout.Button("Add Layer"))
            {
                deformBrush.AddPaint();

                paints = serializedObject.FindProperty("paints");
            }
            
            if (GUILayout.Button("Remove Layer"))
            {
                deformBrush.RemovePaint();

                paints = serializedObject.FindProperty("paints");
            }
            
            EditorGUILayout.EndHorizontal();

            Vector2 offset = new Vector2(20, 30);
            
            Rect lastGUIRect = GUILayoutUtility.GetLastRect();
            lastGUIRect.y += lastGUIRect.height + offset.y;
            lastGUIRect.width = 100;
            lastGUIRect.height = 100;

            for (int i = 0; i < paints.arraySize; i++)
            {
                SerializedProperty paint = paints.GetArrayElementAtIndex(i);

                SerializedProperty mainTextureProperty = paint.FindPropertyRelative("mainTexture");
                SerializedProperty normalMapProperty = paint.FindPropertyRelative("normalMap");

                Rect rect = lastGUIRect;
                rect.y = lastGUIRect.y + (lastGUIRect.height + offset.y) * i;

                Rect labelRect = rect;
                labelRect.y -= 15;
                
                EditorGUI.LabelField(labelRect, "Main Texture");
                
                Texture mainTexture = mainTextureProperty.objectReferenceValue as Texture;
                mainTexture = (Texture) EditorGUI.ObjectField(rect, mainTexture, typeof(Texture), false);
                mainTextureProperty.objectReferenceValue = mainTexture;

                rect.x += rect.width + offset.x;
                labelRect.x = rect.x;
                
                EditorGUI.LabelField(labelRect, "Normal Map");
                
                Texture normalMap = normalMapProperty.objectReferenceValue as Texture;
                normalMap = (Texture) EditorGUI.ObjectField(rect, normalMap, typeof(Texture), false);
                normalMapProperty.objectReferenceValue = normalMap;
                
                GUILayout.Space(lastGUIRect.height + offset.y);
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Apply"))
            {
                deformBrush.enabled = false;
                deformBrush.enabled = true;
            }
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
                
                deformBrush.Draw(textureCoord.Value);
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