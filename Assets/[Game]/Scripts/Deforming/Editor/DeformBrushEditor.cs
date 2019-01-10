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
        private SerializedProperty defaultTexture;
        private SerializedProperty defaultNormalMap;
        private SerializedProperty defaultTextureTiling;
        private SerializedProperty paints;
        private SerializedProperty selectedPaintIndex;
        private SerializedProperty brushShader;
        private SerializedProperty paintedSurfaceShader;

        private void OnEnable()
        {
            deformBrush = (DeformBrush)target;
            
            brushSize = serializedObject.FindProperty("brushSize");
            brushOpacity = serializedObject.FindProperty("brushOpacity");
            defaultTexture = serializedObject.FindProperty("defaultTexture");
            defaultNormalMap = serializedObject.FindProperty("defaultNormalMap");
            defaultTextureTiling = serializedObject.FindProperty("defaultTextureTiling");
            paints = serializedObject.FindProperty("paints");
            selectedPaintIndex = serializedObject.FindProperty("selectedPaintIndex");
            brushShader = serializedObject.FindProperty("brushShader");
            paintedSurfaceShader = serializedObject.FindProperty("paintedSurfaceShader");
        }

        private void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawShaderProperties();
            DrawBrushProperties();
            DrawDefaultTextureProperties();
            DrawLayerProperties();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawShaderProperties()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shader Properties");
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(paintedSurfaceShader);
            EditorGUILayout.PropertyField(brushShader);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawBrushProperties()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Brush Properties");
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Slider(brushSize, 0, 500, "Brush Size");
            EditorGUILayout.Slider(brushOpacity, 0, 1, "Brush Opacity");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawDefaultTextureProperties()
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Default Texture Properties");
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.ObjectField(defaultTexture, typeof(Texture));
            EditorGUILayout.ObjectField(defaultNormalMap, typeof(Texture));
            EditorGUILayout.PropertyField(defaultTextureTiling);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                deformBrush.SetDefaultTextures();
            }
        }

        private void DrawLayerProperties()
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Paint Layer Properties");
            
            EditorGUILayout.BeginHorizontal("box");

            if (paints.arraySize < DeformBrush.MAX_PAINTS)
            {
                if (GUILayout.Button("Add Layer"))
                {
                    deformBrush.AddPaint();
                    paints = serializedObject.FindProperty("paints");
                }
            }

            if (paints.arraySize > 0)
            {
                if (GUILayout.Button("Remove Layer"))
                {
                    deformBrush.RemovePaint();
                    paints = serializedObject.FindProperty("paints");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            Vector2 offset = new Vector2(20, 30);
            
            Rect lastGuiRect = GUILayoutUtility.GetLastRect();
            lastGuiRect.x += 5;
            lastGuiRect.y += lastGuiRect.height + offset.y;
            lastGuiRect.width = 100;
            lastGuiRect.height = 100;

            for (int i = 0; i < paints.arraySize; i++)
            {
                SerializedProperty paint = paints.GetArrayElementAtIndex(i);

                SerializedProperty paintTextureProperty = paint.FindPropertyRelative("paintTexture");
                SerializedProperty normalMapProperty = paint.FindPropertyRelative("normalMap");
                SerializedProperty alphaMapProperty = paint.FindPropertyRelative("alphaMap");
                SerializedProperty tilingProperty = paint.FindPropertyRelative("tiling");

                Rect rect = lastGuiRect;
                rect.y = lastGuiRect.y + (lastGuiRect.height + offset.y) * i;

                Rect labelRect = rect;
                labelRect.y -= 15;

                Rect boxRect = rect;
                boxRect.x -= 5;
                boxRect.y -= 15;
                boxRect.width = lastGuiRect.width * 3 + 10;
                boxRect.height += 20;

                GUI.color = selectedPaintIndex.intValue == i ? Color.blue : Color.white;
                GUI.Box(boxRect, "");
                GUI.color = Color.white;
                
                EditorGUI.LabelField(labelRect, "Paint Texture");
                
                Texture paintTexture = paintTextureProperty.objectReferenceValue as Texture;
                paintTexture = (Texture) EditorGUI.ObjectField(rect, paintTexture, typeof(Texture), false);
                paintTextureProperty.objectReferenceValue = paintTexture;

                rect.x += rect.width + offset.x;
                labelRect.x = rect.x;
                
                EditorGUI.LabelField(labelRect, "Normal Map");
                
                Texture normalMap = normalMapProperty.objectReferenceValue as Texture;
                normalMap = (Texture) EditorGUI.ObjectField(rect, normalMap, typeof(Texture), false);
                normalMapProperty.objectReferenceValue = normalMap;

                rect.x += rect.width + offset.x;
                labelRect.x = rect.x;
                
                EditorGUI.LabelField(labelRect, "Alpha Map");
                
                RenderTexture alphaMap = alphaMapProperty.objectReferenceValue as RenderTexture;
                alphaMap = (RenderTexture) EditorGUI.ObjectField(new Rect(rect.x, rect.y, 50, 50), alphaMap, typeof(RenderTexture));
                alphaMapProperty.objectReferenceValue = alphaMap;

                Rect tilingRect = rect;
                tilingRect.y += tilingRect.height * 0.5f + 20;
                tilingRect.width = 50;
                tilingRect.height = 30;

                labelRect.x = tilingRect.x;
                labelRect.y = tilingRect.y - 15;
                
                EditorGUI.LabelField(labelRect, "Tiling");
                
                int tiling = tilingProperty.intValue;
                tiling = EditorGUI.IntField(tilingRect, tiling);
                tilingProperty.intValue = tiling;

                Rect buttonRect = rect;
                buttonRect.x += 70;
                buttonRect.y -= 15;
                buttonRect.width = 35;
                buttonRect.height += 20;

                if (selectedPaintIndex.intValue != i)
                {
                    if (GUI.Button(buttonRect, "Pick"))
                    {
                        selectedPaintIndex.intValue = i;
                    }
                }
                
                GUILayout.Space(lastGuiRect.height + offset.y);
            }
            
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                deformBrush.UpdatePaintSettings();
            }
        }

        private void OnSceneGUI()
        {
            Handles.BeginGUI();

            for (int i = 0; i < paints.arraySize; i++)
            {
                if (paints.GetArrayElementAtIndex(i).FindPropertyRelative("alphaMap").objectReferenceValue != null)
                {
                    Texture alphaMap = (Texture)paints.GetArrayElementAtIndex(i).FindPropertyRelative("alphaMap")
                        .objectReferenceValue;
                    
                    GUI.DrawTexture(new Rect(5, 5 + 74 * i, 64, 64), alphaMap, ScaleMode.ScaleToFit, false, 1);
                }
            }
            
            Handles.EndGUI();
            
            Camera sceneViewCamera = GetSceneViewCamera();

            if (sceneViewCamera == null)
            {
                return;
            }
            
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            
            Event e = Event.current;

            if (e.type == EventType.MouseDrag)
            {
                Vector2? textureCoord = GetTextureCoord(sceneViewCamera);

                if (textureCoord == null || e.button != 0)
                {
                    return;
                }

                if (e.shift)
                {
                    deformBrush.Erase(textureCoord.Value);
                }
                else
                {
                    deformBrush.Draw(textureCoord.Value);
                }
                
                Repaint();
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
    }
}