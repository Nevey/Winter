using Game.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Deforming.Editor
{
    [CustomEditor(typeof(SurfacePainter))]
    public class SurfacePainterEditor : UnityEditor.Editor
    {
        private SurfacePainter surfacePainter;

        private SerializedProperty brushSize;
        private SerializedProperty brushOpacity;
        
        private SerializedProperty defaultTexture;
        private SerializedProperty defaultNormalMap;
        private SerializedProperty defaultTextureTiling;
        
        private SerializedProperty paintType;
        
        private SerializedProperty selectedPaintIndex;
        
        private SerializedProperty brushShader;
        private SerializedProperty paintedSurfaceShader;

        private SerializedProperty surfaceData;

        private void OnEnable()
        {
            surfacePainter = (SurfacePainter)target;
            surfacePainter.CreateNewSurfaceData();
            
            brushSize = serializedObject.FindProperty("brushSize");
            brushOpacity = serializedObject.FindProperty("brushOpacity");
            
            defaultTexture = serializedObject.FindProperty("defaultTexture");
            defaultNormalMap = serializedObject.FindProperty("defaultNormalMap");
            defaultTextureTiling = serializedObject.FindProperty("defaultTextureTiling");
            
            paintType = serializedObject.FindProperty("paintType");
            
            selectedPaintIndex = serializedObject.FindProperty("selectedPaintIndex");
            
            brushShader = serializedObject.FindProperty("brushShader");
            paintedSurfaceShader = serializedObject.FindProperty("paintedSurfaceShader");

            surfaceData = serializedObject.FindProperty("surfaceData");
        }

        private void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSaveButton();
            DrawShaderProperties();
            DrawBrushProperties();
            DrawDefaultTextureProperties();
            DrawDeformTextureProperties();
            DrawPaintTypeProperties();
            DrawSurfacePaintProperties();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSaveButton()
        {
            if (GUILayout.Button("SAVE"))
            {
                AssetUtility.CreateAsset<SurfaceData>(surfaceData.objectReferenceValue, "Assets/[Game]/Data/SurfaceData.asset");
//                string path = EditorUtility.SaveFilePanel(
//                    "Save Surface Data",
//                    "",
//                    "SurfaceData.asset",
//                    "asset");
//
//                if (path.Length > 0)
//                {
//                    AssetUtility.CreateAsset<SurfaceData>(surfaceData.objectReferenceValue, path);
//                }
            }
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
                surfacePainter.SetDefaultTextures();
            }
        }

        private void DrawDeformTextureProperties()
        {
            SerializedObject surfaceDataObject = new SerializedObject(surfaceData.objectReferenceValue);
            SerializedProperty deformPaint = surfaceDataObject.FindProperty("deformPaint");
            
            if (deformPaint == null)
            {
                surfacePainter.CheckDeformSetup();
            }
            
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Deform Texture Properties");
            EditorGUILayout.BeginVertical("box");

            SerializedProperty deformTextureProperty = deformPaint.FindPropertyRelative("deformTexture");
            SerializedProperty dispMapProperty = deformPaint.FindPropertyRelative("dispMap");
            SerializedProperty tilingProperty = deformPaint.FindPropertyRelative("tiling");
            
            EditorGUILayout.PropertyField(deformTextureProperty);
            EditorGUILayout.PropertyField(dispMapProperty);
            EditorGUILayout.PropertyField(tilingProperty);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            
            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
                surfacePainter.SetDeformTextures();
            }
        }

        private void DrawPaintTypeProperties()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Brush Type");
            EditorGUILayout.BeginVertical("box");
            
            EditorGUI.BeginChangeCheck();

            paintType.intValue = GUILayout.Toolbar(paintType.intValue, new[] {"SURFACE", "DEFORMABLE"});

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawSurfacePaintProperties()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Surface Paint Properties");
            
            DrawAddRemovePaintButtons();
            
            EditorGUILayout.EndVertical();
            
            Vector2 offset = new Vector2(20, 30);
            
            Rect lastGuiRect = GUILayoutUtility.GetLastRect();
            lastGuiRect.x += 5;
            lastGuiRect.y += lastGuiRect.height + offset.y;
            lastGuiRect.width = 100;
            lastGuiRect.height = 100;
            
            EditorGUI.BeginChangeCheck();

            SerializedObject surfaceDataObject = new SerializedObject(surfaceData.objectReferenceValue);
            SerializedProperty surfacePaints = surfaceDataObject.FindProperty("surfacePaints");

            for (int i = 0; i < surfacePaints.arraySize; i++)
            {
                SerializedProperty paint = surfacePaints.GetArrayElementAtIndex(i);

                SerializedProperty paintTextureProperty = paint.FindPropertyRelative("paintTexture");
                SerializedProperty normalMapProperty = paint.FindPropertyRelative("normalMap");
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

                Rect tilingRect = rect;
                tilingRect.x += rect.width + offset.x;
                tilingRect.y += tilingRect.height * 0.5f + 20;
                tilingRect.width = 50;
                tilingRect.height = 30;

                labelRect.x = tilingRect.x;
                labelRect.y = tilingRect.y - 15;
                
                EditorGUI.LabelField(labelRect, "Tiling");
                
                int tiling = tilingProperty.intValue;
                tiling = EditorGUI.IntField(tilingRect, tiling);
                tilingProperty.intValue = tiling;

                Rect buttonRect = tilingRect;
                buttonRect.x += 70;
                buttonRect.y = rect.y - 15;
                buttonRect.width = 35;
                buttonRect.height = rect.height + 20;

                if (selectedPaintIndex.intValue != i)
                {
                    if (GUI.Button(buttonRect, "Pick"))
                    {
                        selectedPaintIndex.intValue = i;
                    }
                }
                
                serializedObject.ApplyModifiedProperties();
                
                GUILayout.Space(lastGuiRect.height + offset.y);
            }
            
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
                surfacePainter.UpdatePaintSettings();
            }
        }

        private void DrawAddRemovePaintButtons()
        {
            EditorGUILayout.BeginHorizontal("box");
            
            EditorGUI.BeginChangeCheck();
            
            SerializedObject surfaceDataObject = new SerializedObject(surfaceData.objectReferenceValue);
            SerializedProperty surfacePaints = surfaceDataObject.FindProperty("surfacePaints");

            if (surfacePaints.arraySize < SurfacePainter.MAX_PAINTS)
            {
                if (GUILayout.Button("Add Layer"))
                {
                    surfacePainter.AddPaint();
                    SceneView.RepaintAll();
                }
            }
            
            if (surfacePaints.arraySize > 0)
            {
                if (GUILayout.Button("Remove Layer"))
                {
                    surfacePainter.RemovePaint();
                    SceneView.RepaintAll();
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
                surfacePainter.UpdatePaintSettings();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            Handles.BeginGUI();
            
            SerializedObject surfaceDataObject = new SerializedObject(surfaceData.objectReferenceValue);
            SerializedProperty surfacePaints = surfaceDataObject.FindProperty("surfacePaints");

            // TODO: Clean this up, and add option to enable/disable drawing of alpha maps on screen
            for (int i = 0; i < surfacePaints.arraySize; i++)
            {
                if (surfacePaints.GetArrayElementAtIndex(i).FindPropertyRelative("alphaMap").objectReferenceValue != null)
                {
                    Texture alphaMap = (Texture)surfacePaints.GetArrayElementAtIndex(i).FindPropertyRelative("alphaMap")
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

            if (e.type == EventType.MouseDown || 
                e.type == EventType.MouseDrag)
            {
                Vector2? textureCoord = GetTextureCoord(sceneViewCamera);

                if (textureCoord == null || e.button != 0)
                {
                    return;
                }

                if (e.shift)
                {
                    surfacePainter.Erase(textureCoord.Value);
                }
                else
                {
                    surfacePainter.Draw(textureCoord.Value);
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
            Collider collider = surfacePainter.GetComponent<Collider>();

            if (collider.Raycast(mouseRay, out RaycastHit hit, float.MaxValue))
            {
                return hit.textureCoord;
            }

            return null;
        }
    }
}