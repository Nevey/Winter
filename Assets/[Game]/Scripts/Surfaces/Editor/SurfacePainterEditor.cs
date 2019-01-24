using Game.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Surfaces.Editor
{
    [CustomEditor(typeof(SurfacePainter))]
    public class SurfacePainterEditor : UnityEditor.Editor
    {
        private const string SURFACES_PATH = "Assets/[Game]/Data/Surfaces/";
        
        private SurfacePainter surfacePainter;
        
        private SerializedProperty brushSize;
        private SerializedProperty brushOpacity;
        private SerializedProperty paintType;
        private SerializedProperty selectedPaintIndex;
        private SerializedProperty visibleSurfacePaintCount;
        private SerializedProperty surfaceData;
        
        private SerializedObject surfaceDataObject;

        private string dataId;
        private bool isMouseDown;
        private bool isPainting;

        private void OnEnable()
        {
            surfacePainter = (SurfacePainter)target;
            
            brushSize = serializedObject.FindProperty("brushSize");
            brushOpacity = serializedObject.FindProperty("brushOpacity");
            paintType = serializedObject.FindProperty("paintType");
            selectedPaintIndex = serializedObject.FindProperty("selectedPaintIndex");
            visibleSurfacePaintCount = serializedObject.FindProperty("visibleSurfacePaintCount");
            surfaceData = serializedObject.FindProperty("surfaceData");
            
            surfacePainter.Load();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSurfaceDataProperties(out bool didInit);

            // Use this to skip one frame as "surfaceData.objectReferenceValue" is being disposed when initializing
            // making the null check throw an exception
            if (didInit)
            {
                return;
            }

            if (surfaceData.objectReferenceValue == null)
            {
                return;
            }
            
            // TODO: Don't do this every frame, but this timing is important
            surfaceDataObject = new SerializedObject(surfaceData.objectReferenceValue);
            
            bool areShaderPropertiesSet = DrawShaderProperties();

            if (!areShaderPropertiesSet)
            {
                return;
            }
            
            DrawBrushProperties();
            DrawDefaultTextureProperties();
            DrawDeformTextureProperties();
            DrawPaintTypeProperties();
            DrawSurfacePaintProperties();

            surfaceDataObject.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSurfaceDataProperties(out bool didInit)
        {
            didInit = false;
            
            if (string.IsNullOrEmpty(dataId))
            {
                dataId = "SurfaceData_" + surfacePainter.gameObject.name;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Surface Paint Data");
            EditorGUILayout.BeginVertical("box");
            
            dataId = EditorGUILayout.TextField("ID", dataId);
            
            if (surfaceData.objectReferenceValue == null && GUILayout.Button("INITIALIZE"))
            {
                SurfaceData existingData = AssetUtility.LoadAssetAtPath<SurfaceData>($"{SURFACES_PATH}{dataId}.asset");

                if (existingData)
                {
                    if (EditorUtility.DisplayDialog("Overwrite existing data",
                        "Do you wish to overwrite existing data?",
                        "Yes", "No"))
                    {
                        CreateSurfaceData();

                        didInit = true;
                    }
                }
                else
                {
                    CreateSurfaceData();

                    didInit = true;
                }
            }

            if (GUILayout.Button("LOAD"))
            {
                SurfaceData loadedData = AssetUtility.LoadAssetAtPath<SurfaceData>($"{SURFACES_PATH}{dataId}.asset");

                if (loadedData == null)
                {
                    EditorUtility.DisplayDialog("Data not found", $"SurfaceData with ID {dataId} was not found!", "OK");
                }
                else
                {
                    surfaceData.objectReferenceValue = loadedData;
                    
                    serializedObject.ApplyModifiedProperties();
                    
                    surfacePainter.Load();

                    didInit = true;
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void CreateSurfaceData()
        {
            surfaceData.objectReferenceValue = CreateInstance<SurfaceData>();
                    
            AssetUtility.CreateAsset<SurfaceData>(surfaceData.objectReferenceValue,
                $"{SURFACES_PATH}{dataId}.asset");
                        
            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawShaderProperties()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shader Properties");
            EditorGUILayout.BeginVertical("box");
            
            EditorGUI.BeginChangeCheck();
            SerializedProperty brushShaderProperty = surfaceDataObject.FindProperty("brushShader");
            EditorGUILayout.PropertyField(brushShaderProperty);
            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.BeginChangeCheck();
            SerializedProperty paintedSurfaceShaderProperty = surfaceDataObject.FindProperty("paintedSurfaceShader");
            EditorGUILayout.PropertyField(paintedSurfaceShaderProperty);
            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
            }

            if (brushShaderProperty.objectReferenceValue != null &&
                paintedSurfaceShaderProperty.objectReferenceValue != null)
            {
                if (GUILayout.Button("Update Material"))
                {
                    surfacePainter.Initialize();
                    surfacePainter.UpdateMaterial();
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            return brushShaderProperty.objectReferenceValue != null &&
                   paintedSurfaceShaderProperty.objectReferenceValue != null;
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
            EditorGUILayout.ObjectField(surfaceDataObject.FindProperty("mainTexture"), typeof(Texture));
            EditorGUILayout.ObjectField(surfaceDataObject.FindProperty("mainNormal"), typeof(Texture));
            EditorGUILayout.PropertyField(surfaceDataObject.FindProperty("mainTextureTiling"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
                
                surfacePainter.UpdateMainTextures();
            }
        }

        private void DrawDeformTextureProperties()
        {
            SerializedProperty deformPaint = surfaceDataObject.FindProperty("deformPaint");
            
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
                surfacePainter.UpdateDeformTextures();
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

            SerializedProperty surfacePaints = surfaceDataObject.FindProperty("surfacePaints");

            for (int i = 0; i < visibleSurfacePaintCount.intValue; i++)
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
                
                GUILayout.Space(lastGuiRect.height + offset.y);
            }
            
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                surfaceDataObject.ApplyModifiedProperties();                
                surfacePainter.UpdateSurfaceTextures();
            }
        }

        private void DrawAddRemovePaintButtons()
        {
            EditorGUILayout.BeginHorizontal("box");

            if (visibleSurfacePaintCount.intValue < SurfacePainter.MAX_PAINTS)
            {
                if (GUILayout.Button("Add Layer"))
                {
                    visibleSurfacePaintCount.intValue++;

                    if (visibleSurfacePaintCount.intValue > SurfaceData.MAX_PAINTS)
                    {
                        visibleSurfacePaintCount.intValue = SurfaceData.MAX_PAINTS;
                    }
                    
                    SceneView.RepaintAll();
                }
            }
            
            if (visibleSurfacePaintCount.intValue > 0)
            {
                if (GUILayout.Button("Remove Layer"))
                {
                    visibleSurfacePaintCount.intValue--;

                    if (visibleSurfacePaintCount.intValue < 0)
                    {
                        visibleSurfacePaintCount.intValue = 0;
                    }
                    
                    // TODO: Reset alpha map for removed layer
                    
                    SceneView.RepaintAll();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            if (surfaceData.objectReferenceValue == null)
            {
                return;
            }
            
            DrawSurfaceAlphaMapsInSceneView();
            DrawDeformAlphaMapInSceneView();
            UpdateMouseInput();
        }

        private void DrawSurfaceAlphaMapsInSceneView()
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
        }

        private void DrawDeformAlphaMapInSceneView()
        {
            Handles.BeginGUI();
            
            SerializedObject surfaceDataObject = new SerializedObject(surfaceData.objectReferenceValue);
            SerializedProperty deformPaint = surfaceDataObject.FindProperty("deformPaint");
            
            Texture alphaMap = (Texture)deformPaint.FindPropertyRelative("alphaMap").objectReferenceValue;
                    
            GUI.DrawTexture(new Rect(5 + 74, 5, 64, 64), alphaMap, ScaleMode.ScaleToFit, false, 1);
            
            Handles.EndGUI();
        }

        private void UpdateMouseInput()
        {
            // TODO: Research... not happy with current behaviour
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            
            Event e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                isMouseDown = true;
            }

            if (isMouseDown)
            {
                Vector2? textureCoord = GetTextureCoord();

                if (e.button != 0)
                {
                    isMouseDown = false;
                    return;
                }
                
                if (textureCoord == null)
                {
                    return;
                }

                isPainting = true;

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

            if (e.type == EventType.MouseUp && isPainting)
            {
                if (e.button != 0)
                {
                    return;
                }
                
                isMouseDown = false;
                
                surfaceDataObject.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
                
                surfacePainter.Save();
            }
        }

        private Vector2? GetTextureCoord()
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