using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace DS
{
    [CustomEditor(typeof(DeformableSurface))]
    public class DeformableSurfaceEditor : Editor
    {
        #region Const
        const string NEW_SURFACE_NAME = "New Deformable Surface";
        const string GENERAL_ASSET_NAME = "DS";
        const string COMPUTE_ASSET_NAME = "SurfaceKernels";
        const string MENU_ITEM_CREATE_PATH = "GameObject/3D Object/Deformable Surface";
        const string BRUSH_TEXTURE_NAME = "DeformableSurface_brush";

        static readonly string[,] DEFAULT_TEX_NAMES =
        {
            { SurfaceData.ShaderProperties.TEXTURES[0], "DeformableSurface_snow_base_albedo" },
            { SurfaceData.ShaderProperties.TEXTURES[1], "DeformableSurface_snow_flat_albedo" },
            { SurfaceData.ShaderProperties.TEXTURES[2], "DeformableSurface_snow_base_normal" },
            { SurfaceData.ShaderProperties.TEXTURES[3], "DeformableSurface_snow_flat_normal" },
        };
        static readonly DebugMessage[] EDITOR_MESSAGES =
        {
            new DebugMessage("One or more shader assets was missed", MessageType.Error), //0
            new DebugMessage("Compute shader was not found", MessageType.Error), //1
            new DebugMessage("DeformableSurfaceEditor has lost reference to this object", MessageType.Error), //2
            new DebugMessage("Reference to SurfaceData is missing", MessageType.Error), //3
            new DebugMessage("DepthTexture shader was not found", MessageType.Error), //4
            new DebugMessage("Changing mesh width, lenght and maps resolution will clear heightmap data", MessageType.Info), //5
            new DebugMessage(BRUSH_TEXTURE_NAME + " texture was not found. Painting will not be permitted", MessageType.Warning), //6
            new DebugMessage("This will clear all manual changes done in height map. Continue anyway?", MessageType.Info), //7
            new DebugMessage("This will clear all manual changes done in offset map. Continue anyway?", MessageType.Info), //8
            new DebugMessage("Reference to Deformable Surface object is missing", MessageType.Error), //9
            new DebugMessage("[Ctrl + Scroll] - Adjust brush radius", MessageType.Info), //10
            new DebugMessage("[Shift + Scroll] - Adjust brush hardness", MessageType.Info), //11
            new DebugMessage("Target terrain is too small for alignment", MessageType.Info), //12
            new DebugMessage("Unable to load inspector", MessageType.Warning), //13
            new DebugMessage("[LMB / LMB + Shift] - Raise / Lower", MessageType.Info), //14
        };
        static readonly GUIContent[] FIELDS_CONTENT =
        {
            new GUIContent("Width", "Base mesh polygons count by X"), //0
            new GUIContent("Length", "Base mesh polygons count by Z"), //1
            new GUIContent("Scale", "Base mesh scale"), //2
            new GUIContent("Resolution", "Base map resolution. Multiplied by mesh width and length to determine the final resolution"), //3
            new GUIContent("Max Offset", "Maximum offset in units (deformable part of the surface)"), //4
            new GUIContent("Deform Mask", "Layer mask used to determine which objects on the scene may deform the surface. It is reasonable to keep there only layers with dynamic objects which may actual deform the surface to avoid unnecessary batches"), //5
            new GUIContent("Deform Speed", "Framerate dependent deformation speed of the surface"), //6
            new GUIContent("Smoothing", "Apply Gaussian blur to depth texture. Requires additional compute shader passes"), //7
            new GUIContent("Radius", "Smoothing radius in pixels"), //8
            new GUIContent("Passes", "Number of smoothing passes. NOTE: Higher value may affect performance"), //9
            new GUIContent("Quality", "Number of tessellation quad patches for each base mesh polygon "), //10
            new GUIContent("Distance", "Tesselation camera distance in units"), //11
            new GUIContent("Max Height", "Maximum height in units"), //12
        };
        #endregion

        #region Editor fields
        private bool _editorReady;

        private GUIContent[] _toolbarContent;
        private string[] _paintToolbarContent;
        
        private Texture2D _brushAlpha;
        private Material _brushMaterial;
        private Material _paintMaterial;
        private RenderTexture _brushRt;
        private RenderTexture _tempRt;

        private int _selectedToolbar;
        
        private SurfaceMapType _paintMode;
        private int _brushRadius;
        private float _brushHardness;

        private float _flattenValue;
        private float _rndPerlinSize;
        private float _rndPerlinAmplitude;
        private int _rndPerlinSeed;
        private float _alignOffset;

        private bool[] _foldouts = new bool[4];
        #endregion

        #region Private
        private DeformableSurface _target;

        private static Terrain _targetTerrain;
        private static Texture2D _targetMap;

        private bool _isPainting;
        #endregion


        void OnEnable()
        {
            _target = (DeformableSurface)target;
            if (_target == null || _target.Data == null)
                return;

            //Load GUI content
            _toolbarContent = new[]
            {
                EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSetHeight", "Paint height"),
                EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSplat", "Textures"),
                EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSettings", "Settings")
            };
            _paintToolbarContent = new[]
            {
                 "Height", "Offset"
            };

            LoadPrefs();
            LoadBrush();

            _editorReady = true;
        }

        void OnDisable()
        {
            if (!_editorReady)
                return;

            SavePrefs();

            if (_brushRt != null)
                DestroyImmediate(_brushRt);
            if (_tempRt != null)
                DestroyImmediate(_tempRt);
            if (_brushMaterial != null)
                DestroyImmediate(_brushMaterial);
            if (_paintMaterial != null)
                DestroyImmediate(_paintMaterial);
        }

        void LoadBrush()
        {
            _brushAlpha = GetBrushAlpha();

            _brushMaterial = new Material(Shader.Find("Hidden/Deformable Surface/Surface Editor Brush"));
            _brushMaterial.SetTexture(SurfaceData.ShaderProperties.BASE_MAP, _target.Data.BaseMap);

            _paintMaterial = new Material(Shader.Find("Hidden/Deformable Surface/Surface Paint"));
            
            if (_selectedToolbar == 0)
                InitBrushResources();
        }

        void InitBrushResources()
        {
            int w = _target.Data.BaseMap.width;
            int h = _target.Data.BaseMap.height;

            if (_brushRt == null || _brushRt.width != w || _brushRt.height != h)
            {
                _brushRt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    name = "DS_BrushRt",
                    filterMode = FilterMode.Point,
                    autoGenerateMips = false,
                    wrapMode = TextureWrapMode.Clamp,
                    enableRandomWrite = true,
                };
            }

            if (_tempRt == null || _tempRt.width != w || _tempRt.height != h)
            {
                _tempRt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    name = "DS_TempRt",
                    filterMode = FilterMode.Bilinear,
                    autoGenerateMips = false,
                    wrapMode = TextureWrapMode.Clamp,
                    enableRandomWrite = true
                };
            }

            _brushMaterial.SetFloat("_height_max", _target.Data.MaxHeight);
            _brushMaterial.SetFloat("_offset_max", _target.Data.MaxOffset);
            _brushMaterial.SetFloat("_tess_amount", _target.Data.TessQuality + (_target.Data.TessQuality - 1));
            _brushMaterial.SetFloat("_tess_distance", _target.Data.TessDistance);
            _brushMaterial.SetTexture("_brush", _brushRt);
        }

        void LoadPrefs()
        {
            _selectedToolbar = Mathf.Clamp(EditorPrefs.GetInt("DS._selectedToolbar", 0), 0, _toolbarContent.Length - 1);
            _paintMode = (SurfaceMapType)Mathf.Clamp(EditorPrefs.GetInt("DS._selectedPaintMode", 0), 0, 1);
            _brushRadius = EditorPrefs.GetInt("DS._brushRadius", 50);
            _brushHardness = Mathf.Clamp(EditorPrefs.GetFloat("DS._brushHardness", 0.5f), 0.001f, 1f);
            _flattenValue = Mathf.Clamp(EditorPrefs.GetFloat("DS._flattenValue", 0), 0, float.MaxValue);
            _rndPerlinSize = EditorPrefs.GetFloat("DS._rndPerlinSize", 0.1f);
            _rndPerlinAmplitude = EditorPrefs.GetFloat("DS._rndPerlinAmplitude", 0.025f);
            _rndPerlinSeed = EditorPrefs.GetInt("DS._rndPerlinSeed", 123456);
            _alignOffset = EditorPrefs.GetFloat("DS._alignOffset", 0.1f);

            for (int i = 0; i < _foldouts.Length; i++)
                _foldouts[i] = EditorPrefs.GetBool(string.Format("DS._foldouts[{0}]", i), false);
        }

        void SavePrefs()
        {
            EditorPrefs.SetInt("DS._selectedToolbar", _selectedToolbar);
            EditorPrefs.SetInt("DS._selectedPaintMode", (int)_paintMode);
            EditorPrefs.SetInt("DS._brushRadius", _brushRadius);
            EditorPrefs.SetFloat("DS._brushHardness", _brushHardness);
            EditorPrefs.SetFloat("DS._flattenValue", _flattenValue);
            EditorPrefs.SetFloat("DS._rndPerlinSize", _rndPerlinSize);
            EditorPrefs.SetFloat("DS._rndPerlinAmplitude", _rndPerlinAmplitude);
            EditorPrefs.SetInt("DS._rndPerlinSeed", _rndPerlinSeed);
            EditorPrefs.SetFloat("DS._alignOffset", _alignOffset);

            for (int i = 0; i < _foldouts.Length; i++)
                EditorPrefs.SetBool(string.Format("DS._foldouts[{0}]", i), _foldouts[i]);
        }
        

        public override void OnInspectorGUI()
        {
            #region Check
            if (_target == null)
            {
                DrawMessage(2);
                return;
            }

            SurfaceData data = _target.Data;
            if (data == null)
            {
                _target.Data = (SurfaceData)EditorGUILayout.ObjectField("Surface Data", _target.Data, typeof(SurfaceData), false);
                DrawMessage(3);
                return;
            }

            if (!_editorReady)
            {
                DrawMessage(13);
                return;
            }
            #endregion

            DrawToolbar();
            UseBaseHotKeys(Event.current);
            switch (_selectedToolbar)
            {
                case 0:
                    #region Draw paint settings
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);

                    DrawLabel("Edit Mode");
                    _paintMode = (SurfaceMapType) GUILayout.Toolbar((int) _paintMode, _paintToolbarContent);
                    
                    DrawLabel("Brush");
                    _brushRadius = EditorGUILayout.IntSlider("Radius", _brushRadius, 1, 512);
                    _brushHardness = EditorGUILayout.Slider("Hardness", _brushHardness, 0.001f, 1f);
                    EditorGUILayout.HelpBox(string.Format("{0}\n{1}\n{2}", 
                        EDITOR_MESSAGES[14].Message, EDITOR_MESSAGES[10].Message, EDITOR_MESSAGES[11].Message), MessageType.Info);

                    _foldouts[0] = EditorGUILayout.Foldout(_foldouts[0], "Flatten", true);
                    if (_foldouts[0])
                    {
                        _flattenValue = EditorGUILayout.Slider(new GUIContent("Target Value"), _flattenValue, 0, 1);
                        if (GUILayout.Button("Flatten " + _paintMode))
                            Flatten();
                    }
                    
                    _foldouts[1] = EditorGUILayout.Foldout(_foldouts[1], "Randomize", true);
                    if (_foldouts[1])
                    {
                        _rndPerlinSize = EditorGUILayout.Slider("Size", _rndPerlinSize, 0f, 1f);
                        _rndPerlinAmplitude = EditorGUILayout.FloatField("Amplitude", _rndPerlinAmplitude);

                        int perlinSeed = _rndPerlinSeed;
                        perlinSeed = EditorGUILayout.DelayedIntField("Seed", perlinSeed);
                        if (perlinSeed != _rndPerlinSeed)
                        {
                            _rndPerlinSeed = perlinSeed;
                            Randomize(false);
                        }

                        if (GUILayout.Button("Randomize " + _paintMode))
                            Randomize(true);
                    }

                    
                    _foldouts[2] = EditorGUILayout.Foldout(_foldouts[2], "Import Map", true);
                    if (_foldouts[2])
                    {
                        _targetMap =
                            (Texture2D) EditorGUILayout.ObjectField(_targetMap, typeof(Texture2D), false);

                        EditorGUI.BeginDisabledGroup(_targetMap == null || _targetMap == data.BaseMap);
                        if (GUILayout.Button(_paintMode == SurfaceMapType.Height
                            ? "Import Height Map"
                            : "Import Offset Map"))
                            ImportMap(data, _targetMap);
                        EditorGUI.EndDisabledGroup();

                    }
                    _foldouts[3] = EditorGUILayout.Foldout(_foldouts[3], "Align to Terrain", true);
                    if (_foldouts[3])
                    {
                        _targetTerrain = (Terrain)EditorGUILayout.ObjectField(_targetTerrain, typeof(Terrain), true);
                        _alignOffset = EditorGUILayout.FloatField("Height Offset", _alignOffset);

                        bool noTerrain = _targetTerrain == null || _targetTerrain.terrainData == null;
                        EditorGUI.BeginDisabledGroup(noTerrain);
                        if (GUILayout.Button("Align"))
                            AlignToTerrain(data);

                        if (noTerrain)
                            EditorGUI.EndDisabledGroup();

                        EditorGUI.EndDisabledGroup();
                    }
                    #endregion
                    break;

                case 1:
                    #region Draw material settings

                    DrawLabel("Base Map Settings");

                    EditorGUI.BeginDisabledGroup(Application.isPlaying);
                    float nmv = data.NormalIntensity;
                    EditorGUI.BeginChangeCheck();
                    nmv = EditorGUILayout.Slider("Normal Intensity", nmv, 0, 1);
                    if (EditorGUI.EndChangeCheck())
                        data.NormalIntensity = nmv;
                    EditorGUI.EndDisabledGroup();

                    DrawLabel("Material Settings");
                    
                    data.TextureSize = EditorGUILayout.FloatField("Texture Size", data.TextureSize);
                    if (data.SourceMaterial.HasProperty(SurfaceData.ShaderProperties.BLEND_H))
                        data.TextureBlendHeight = EditorGUILayout.FloatField("Texture Blend Height", data.TextureBlendHeight);

                    EditorGUILayout.Space();
                    DrawMatSlider(new GUIContent("Metallic"), "_metallic", 0, 1);
                    DrawMatSlider(new GUIContent("Smoothness"), "_smoothness", 0, 1);

                    DrawLabel("Textures (Height | Offset)");
                    float boxSize = 100;
                    EditorGUILayout.LabelField("Albedo (RGB) Smoothness (A)");
                    Rect layourRect = EditorGUILayout.GetControlRect();
                    layourRect.y += 5;

                    DrawTextureBox(SurfaceData.ShaderProperties.TEXTURES[1], new Rect(layourRect.x, layourRect.y, boxSize, boxSize), true);
                    DrawTextureBox(SurfaceData.ShaderProperties.TEXTURES[0], new Rect(layourRect.x + boxSize + 10, layourRect.y, boxSize, boxSize), false);

                    EditorGUILayout.LabelField("Normal Map (RGB)");
                    layourRect = EditorGUILayout.GetControlRect();
                    layourRect.y += 5;

                    DrawTextureBox(SurfaceData.ShaderProperties.TEXTURES[3], new Rect(layourRect.x, layourRect.y, boxSize, boxSize), true);
                    DrawTextureBox(SurfaceData.ShaderProperties.TEXTURES[2], new Rect(layourRect.x + boxSize + 10, layourRect.y, boxSize, boxSize), false);
                    #endregion
                    break;

                case 2:
                    #region Draw surface settings
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);

                    EditorGUILayout.Space();
                    EditorGUI.BeginChangeCheck();
                    SurfaceData d = (SurfaceData)EditorGUILayout.ObjectField("Surface Data", _target.Data, typeof(SurfaceData), false);
                    if (EditorGUI.EndChangeCheck())
                        _target.Data = d;

                    EditorGUI.EndDisabledGroup();
                    _target.DeformEnabled = EditorGUILayout.Toggle("Deform Enabled", _target.DeformEnabled);
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);

                    EditorGUI.BeginChangeCheck();
                    DrawLabel("Mesh");
                    data.MeshWidth = EditorGUILayout.DelayedIntField(FIELDS_CONTENT[0], data.MeshWidth);
                    data.MeshLength = EditorGUILayout.DelayedIntField(FIELDS_CONTENT[1], data.MeshLength);
                    data.MeshScale = EditorGUILayout.DelayedFloatField(FIELDS_CONTENT[2], data.MeshScale);

                    DrawLabel("Maps");
                    data.MapsResolution = EditorGUILayout.DelayedIntField(FIELDS_CONTENT[3], data.MapsResolution);

                    float mh = data.MaxHeight;
                    mh = EditorGUILayout.DelayedFloatField(FIELDS_CONTENT[12], mh);
                    if (EditorGUI.EndChangeCheck())
                    {
                        data.MaxHeight = mh;
                        RefreshMeshCollider();
                    }
                    
                    data.MaxOffset = EditorGUILayout.FloatField(FIELDS_CONTENT[4], data.MaxOffset);

                    EditorGUILayout.HelpBox(EDITOR_MESSAGES[5].Message, EDITOR_MESSAGES[5].Type);

                    EditorGUI.EndDisabledGroup();

                    DrawLabel("Deformation");
                    data.DeformMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(EditorGUILayout.MaskField(FIELDS_CONTENT[5],
                        InternalEditorUtility.LayerMaskToConcatenatedLayersMask(data.DeformMask),
                        InternalEditorUtility.layers));
                    data.DeformSpeed = EditorGUILayout.Slider(FIELDS_CONTENT[6], data.DeformSpeed, 0, SurfaceData.MAX_DEFORM_SPEED);
                    data.Smoothing = EditorGUILayout.Toggle(FIELDS_CONTENT[7], data.Smoothing);
                    if (data.Smoothing)
                    {
                        EditorGUI.indentLevel = 1;
                        data.SmoothingRadius = EditorGUILayout.IntSlider(FIELDS_CONTENT[8], data.SmoothingRadius, 1, SurfaceData.MAX_SMOOTH_RADIUS);
                        data.SmoothingPasses = EditorGUILayout.IntSlider(FIELDS_CONTENT[9], data.SmoothingPasses, 1, SurfaceData.MAX_SMOOTH_PASSES);
                        EditorGUI.indentLevel = 0;
                    }

                    DrawLabel("Tesselation");
                    if (data.SourceMaterial.HasProperty(SurfaceData.ShaderProperties.TESS_AMOUNT))
                        data.TessQuality = EditorGUILayout.IntSlider(FIELDS_CONTENT[10], data.TessQuality, 1, SurfaceData.MAX_TESS_QUALITY);
                    
                    if (data.SourceMaterial.HasProperty(SurfaceData.ShaderProperties.TESS_DIST) && data.TessQuality != 0)
                        data.TessDistance = EditorGUILayout.FloatField(FIELDS_CONTENT[11], data.TessDistance);
                    #endregion
                    break;
            }
        }

        void OnSceneGUI()
        {
            #region Check
            if (_target == null || !_editorReady)
                return;

            SurfaceData data = _target.Data;
            if (data == null)
                return;

            Camera sceneViewCamera = GetSceneViewCamera();
            if (sceneViewCamera == null)
                return;
            #endregion

            #region Draw bounds
            if (_target.Data.MeshBase != null)
            {
                Handles.color = new Color(1, 1, 1, 0.25f);
                Handles.DrawWireCube(_target.Data.MeshBase.bounds.center + _target.transform.position, _target.Data.MeshBase.bounds.size);
            }
            #endregion

            Event e = Event.current;

            bool showBrush = _selectedToolbar == 0 && Tools.current == Tool.None && !Application.isPlaying;
            if (showBrush)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                #region Use HotKeys
                if (e.control && e.type == EventType.ScrollWheel)
                {
                    _brushRadius += (int)-e.delta.y;
                    e.Use();
                    Repaint();
                }
                if (e.shift && e.type == EventType.ScrollWheel)
                {
                    _brushHardness += (-e.delta.y * 0.01f);
                    e.Use();
                    Repaint();
                }
                UseBaseHotKeys(e);
                #endregion

                Vector3 pointerPos = GetPointerPosition(sceneViewCamera);
                Vector2 uvPos = GetUvFromPointer(pointerPos);

                DrawBrush(uvPos);

                if (!_isPainting && e.type == EventType.MouseDown && e.button == 0 && !e.control && !e.alt)
                    _isPainting = true;

                if (_isPainting && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown))
                    Paint();

                if (_isPainting && e.type == EventType.MouseUp && e.button == 0)
                {
                    _isPainting = false;
                    
                    _target.Data.BuildMeshes();
                    _target.Data.RebuildNormals();
                    RefreshMeshCollider();
                }

                SceneView.RepaintAll();
            }

            _target.transform.rotation = Quaternion.identity;
            _target.transform.localScale = Vector3.one;
        }

        void DrawBrush(Vector2 uvPos)
        {
            _brushMaterial.SetFloat("_brush_hardness", _brushHardness);
            if (_brushMaterial.SetPass(0))
                Graphics.DrawMeshNow(_target.Data.MeshBase, _target.transform.position + new Vector3(0, 0.05f, 0), _target.transform.rotation);

            int w = _target.Data.BaseMap.width;
            int h = _target.Data.BaseMap.height;

            Vector2 brushPos = new Vector2((uvPos.x * w) - _brushRadius * 0.5f, h - (uvPos.y * h + _brushRadius * 0.5f));
            Rect brushRect = new Rect(brushPos, new Vector2(_brushRadius, _brushRadius));

            RenderTexture.active = _brushRt;
            GL.Clear(true, true, Color.clear, 0);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, w, h, 0);
            Graphics.DrawTexture(brushRect, _brushAlpha);
            GL.PopMatrix();

            RenderTexture.active = null;
        }

        void Paint()
        {
            float hardness = Event.current.shift ? -_brushHardness : _brushHardness;
            hardness /= _paintMode == SurfaceMapType.Height ? _target.Data.MaxHeight : _target.Data.MaxOffset;
            hardness *= 2;

            _paintMaterial.SetTexture("_baseMap", _target.Data.BaseMap);
            _paintMaterial.SetTexture("_brush", _brushRt);
            _paintMaterial.SetFloat("_hardness", hardness);
            _paintMaterial.SetInt("_mode", (int) _paintMode);

            RenderTexture lastRt = RenderTexture.active;

            RenderTexture.active = _tempRt;
            Graphics.Blit(_tempRt, _paintMaterial);
            
            _target.Data.BaseMap.ReadPixels(new Rect(0, 0, _tempRt.width, _tempRt.height), 0, 0);
            _target.Data.BaseMap.Apply();
            RenderTexture.active = lastRt;
        }


        void UseBaseHotKeys(Event e)
        {
            if (e.keyCode == KeyCode.F1 || e.keyCode == KeyCode.F2 || e.keyCode == KeyCode.F3)
            {
                _selectedToolbar = (int)e.keyCode - 282;

                e.Use();
                Repaint();
            }

            if (e.keyCode == KeyCode.Escape)
                Tools.current = Tool.Move;
        }

        Vector2 GetUvFromPointer(Vector3 pointerPos)
        {
            Vector3 localMapPos = (_target.transform.InverseTransformPoint(pointerPos) * _target.Data.MapsResolution) / _target.Data.MeshScale;
            return new Vector2(localMapPos.x / _target.Data.BaseMap.width, localMapPos.z / _target.Data.BaseMap.height);
        }

        Vector3 GetPointerPosition(Camera camera)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hit;
            if (_target.MeshCollider.Raycast(mouseRay, out hit, float.MaxValue))
                return hit.point;

            Plane plane = new Plane(Vector3.up, _target.transform.position);
            float distance;
            plane.Raycast(mouseRay, out distance);

            return camera.transform.position + mouseRay.direction * distance;
        }

        
        void RefreshMeshCollider()
        {
            if (!_target.MeshCollider.enabled)
                return;

            _target.MeshCollider.enabled = false;
            _target.MeshCollider.enabled = true;
        }

        void Flatten()
        {
            _target.Data.FlattenMap(_paintMode, _flattenValue);
            if (_paintMode == SurfaceMapType.Height)
                RefreshMeshCollider();
        }
        
        void Randomize(bool generateSeed)
        {
            if (generateSeed)
                _rndPerlinSeed = Random.Range(0, 9999);

            _target.Data.RandomizeMap(_paintMode, _rndPerlinSize, _rndPerlinAmplitude, _rndPerlinSeed);
            RefreshMeshCollider();
        }

        void AlignToTerrain(SurfaceData data)
        {
            TerrainData tData = _targetTerrain.terrainData;
            Vector3 meshSize = data.MeshBase.bounds.size;

            if (tData.size.x < meshSize.x || tData.size.z < meshSize.z)
            {
                DebugMessage(12);
                return;
            }

            Vector3 sPos = _target.transform.position;
            Vector3 tPos = _targetTerrain.gameObject.transform.position;

            int w = data.BaseMap.width;
            int h = data.BaseMap.height;

            #region Align position
            if (sPos.x < tPos.x)
                sPos.x = tPos.x;
            if (sPos.x + meshSize.x > tPos.x + tData.size.x)
                sPos.x = tPos.x + tData.size.x - meshSize.x;

            if (sPos.z < tPos.z)
                sPos.z = tPos.z;
            if (sPos.z + meshSize.z > tPos.z + tData.size.z)
                sPos.z = tPos.z + tData.size.z - meshSize.z;
            #endregion

            #region Calculate height values
            Vector3 localPos = -(tPos - sPos);
            Vector2 relSize = new Vector2(meshSize.x / tData.size.x, meshSize.z / tData.size.z);
            Vector2 startPoint = new Vector2(localPos.x / tData.size.x, localPos.z / tData.size.z);
            Vector2 endPoint = startPoint + relSize;

            float[,] heightValues = new float[w, h];
            float minHeight = float.MaxValue;
            float maxHeight = 0;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Vector2 curPoint = new Vector2(
                        Mathf.Lerp(startPoint.x, endPoint.x, (float)i / w), 
                        Mathf.Lerp(startPoint.y, endPoint.y, (float)j / h));

                    float height = tData.GetInterpolatedHeight(curPoint.x, curPoint.y);

                    if (height < minHeight)
                        minHeight = height;
                    if (height > maxHeight)
                        maxHeight = height;

                    heightValues[i, j] = height;
                }
            }
            #endregion

            sPos.y = _targetTerrain.transform.position.y + minHeight;
            _target.transform.position = sPos;

            #region Build map
            float tarMaxHeight = (maxHeight - minHeight) + _alignOffset;
            data.MaxHeight = tarMaxHeight;

            Texture2D bm = _target.Data.BaseMap;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color c = bm.GetPixel(i, j);
                    c.r = (heightValues[i, j] - minHeight + _alignOffset) / tarMaxHeight;
                    bm.SetPixel(i, j, c);
                }
            }
            bm.Apply();
            #endregion

            data.BuildMeshes();
            data.RebuildNormals();
            RefreshMeshCollider();
        }

        void ImportMap(SurfaceData data, Texture2D map)
        {
            var importer = (TextureImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(map));
            if (importer == null)
                return;

            bool readable = importer.isReadable;
            if (!readable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            int w = data.BaseMap.width;
            int h = data.BaseMap.height;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color tar = data.BaseMap.GetPixel(i, j);
                    Color src = map.GetPixelBilinear((i + 1f) / w, (j + 1f) / h);

                    if (_paintMode == SurfaceMapType.Height)
                        tar.r = (src.r + src.g + src.b) / 3;
                    else tar.g = (src.r + src.g + src.b) / 3;

                    data.BaseMap.SetPixel(i, j, tar);
                }
            }
            data.BaseMap.Apply();

            if (_paintMode == SurfaceMapType.Height)
            {
                data.BuildMeshes();
                RefreshMeshCollider();
            }

            data.UpdateMeshBounds();
            data.RebuildNormals();

            if (!readable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }


        #region Inspector helpers
        void DrawToolbar()
        {
            int toolbarId = _selectedToolbar;
            toolbarId = GUILayout.Toolbar(toolbarId, _toolbarContent);

            if (toolbarId != _selectedToolbar)
            {
                if (toolbarId == 0)
                {
                    Tools.current = Tool.None;
                    InitBrushResources();
                }
            }

            _selectedToolbar = toolbarId;
        }

        void DrawLabel(string label)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        void DrawTextureBox(string textureName, Rect rect, bool space)
        {
            if (space)
                GUILayout.Space(rect.height + 5);

            Texture tex = null;
            if (!_target.Data.SourceMaterial.HasProperty(textureName))
            {
                EditorGUI.BeginDisabledGroup(true);
                tex = (Texture) EditorGUI.ObjectField(rect, tex, typeof(Texture), false);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                tex = _target.Data.SourceMaterial.GetTexture(textureName);
                tex = (Texture)EditorGUI.ObjectField(rect, tex, typeof(Texture), false);
                if (EditorGUI.EndChangeCheck())
                    _target.Data.SourceMaterial.SetTexture(textureName, tex);
            }
        }

        void DrawMatSlider(GUIContent content, string propertyName, float min, float max)
        {
            if (!_target.Data.SourceMaterial.HasProperty(propertyName))
                return;

            float p = _target.Data.SourceMaterial.GetFloat(propertyName);
            EditorGUI.BeginChangeCheck();
            p = EditorGUILayout.Slider(content, p, min, max);
            if (EditorGUI.EndChangeCheck())
                _target.Data.SourceMaterial.SetFloat(propertyName, p);
        }
        #endregion
        
        #region Service
        Texture2D GetBrushAlpha()
        {
            string[] guids = AssetDatabase.FindAssets(BRUSH_TEXTURE_NAME + " t:" + typeof(Texture2D).Name);
            if (guids == null || guids.Length == 0)
            {
                DebugMessage(1);
                return Texture2D.whiteTexture;
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return tex == null ? Texture2D.whiteTexture : tex;
        }

        Camera GetSceneViewCamera()
        {
            if (SceneView.lastActiveSceneView != null)
                if (SceneView.lastActiveSceneView.camera != null)
                    return SceneView.lastActiveSceneView.camera;

            return null;
        }

        static void DrawMessage(int messageId)
        {
            if (messageId < 0 || messageId >= EDITOR_MESSAGES.Length)
                return;

            EditorGUILayout.HelpBox(EDITOR_MESSAGES[messageId].Message, EDITOR_MESSAGES[messageId].Type);
        }

        static void DebugMessage(int messageId)
        {
            if (messageId < 0 || messageId >= EDITOR_MESSAGES.Length)
                return;

            switch (EDITOR_MESSAGES[messageId].Type)
            {
                case MessageType.Info:
                    Debug.Log(EDITOR_MESSAGES[messageId].Message);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(EDITOR_MESSAGES[messageId].Message);
                    break;
                case MessageType.Error:
                    Debug.LogError(EDITOR_MESSAGES[messageId].Message);
                    break;
            }
        }
        #endregion

        #region Asset builder
        [MenuItem(MENU_ITEM_CREATE_PATH)]
        public static void CreateNewSurface()
        {
            #region Create data asset
            string dataId = GetNewSurfaceDataId();
            string newDataAssetPath = "Assets/" + NEW_SURFACE_NAME + dataId + ".asset";
            SurfaceData data = CreateInstance<SurfaceData>();
            AssetDatabase.CreateAsset(data, newDataAssetPath);
            #endregion

            #region Find shaders
            Shader dsShader = Shader.Find(SurfaceData.ShaderProperties.SH_PATH_STANDARD);
            Shader depthShader = Shader.Find(SurfaceData.ShaderProperties.SH_PATH_DEPTH_TEX);

            if (!dsShader || !depthShader)
            {
                DebugMessage(0);
                return;
            }
            #endregion

            #region Find compute shader
            ComputeShader compute = GetCompute(COMPUTE_ASSET_NAME);
            if (compute == null)
                return;
            #endregion

            #region Create materials
            Material mat = new Material(dsShader)
            {
                name = GetFullAssetName("material", data)
            };
            AssetDatabase.AddObjectToAsset(mat, data);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat));
            #endregion

            #region Fill data
            data.MeshBase = AddNewMesh("base_mesh", data);
            data.MeshCollider = AddNewMesh("collider_mesh", data);
            data.BaseMap = AddNewTextureMap("base_map", TextureFormat.ARGB32, data);
            
            data.SurfaceKernels = compute;
            data.SourceMaterial = mat;
            data.DepthTextureShader = depthShader;

            data.BuildBaseMap();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(data));
            #endregion

            #region Assign textures to material
            mat.SetTexture(SurfaceData.ShaderProperties.BASE_MAP, data.BaseMap);
            for (int i = 0; i < DEFAULT_TEX_NAMES.GetLength(0); i++)
            {
                string[] texGUIds = AssetDatabase.FindAssets(DEFAULT_TEX_NAMES[i, 1] + " t:" + typeof(Texture).Name, new[] { "Assets" });
                if (texGUIds == null || texGUIds.Length == 0)
                    continue;

                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(texGUIds[0]));

                mat.SetTexture(DEFAULT_TEX_NAMES[i, 0], tex);
            }
            #endregion

            //Create game object
            DeformableSurface newSurface = new GameObject(NEW_SURFACE_NAME + dataId).AddComponent<DeformableSurface>();
            newSurface.Data = data;
            Undo.RegisterCreatedObjectUndo(newSurface.gameObject, "New Deformable Surface");

            EditorPrefs.SetInt("_selectedToolbar", 2);

            //Align to parent
            if (Selection.activeGameObject)
                GameObjectUtility.SetParentAndAlign(newSurface.gameObject, Selection.activeGameObject);
            Selection.activeGameObject = newSurface.gameObject;
        }

        static ComputeShader GetCompute(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName + " t:" + typeof(ComputeShader).Name);
            if (guids == null || guids.Length == 0)
            {
                DebugMessage(1);
                return null;
            }

            ComputeShader[] shaders = new ComputeShader[guids.Length];
            for (int i = 0; i < shaders.Length; i++)
                shaders[i] = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath(guids[i]));

            for (int i = 0; i < shaders.Length; i++)
                if (shaders[i] != null && shaders[i].name == fileName)
                    return shaders[i];

            return null;
        }

        static Mesh AddNewMesh(string name, SurfaceData data)
        {
            Mesh mesh = new Mesh { name = GetFullAssetName(name, data) };
            AssetDatabase.AddObjectToAsset(mesh, data);

            return mesh;
        }

        static Texture2D AddNewTextureMap(string name, TextureFormat format, SurfaceData data)
        {
            Texture2D tex = new Texture2D(4, 4, format, false, true)
            {
                name = GetFullAssetName(name, data),
                wrapMode = TextureWrapMode.Clamp,
            };
            AssetDatabase.AddObjectToAsset(tex, data);

            return tex;
        }

        static string GetNewSurfaceDataId()
        {
            string searchFilter = NEW_SURFACE_NAME + " t:" + typeof(SurfaceData).Name;

            string[] foundDataGUIDs = AssetDatabase.FindAssets(searchFilter, new[] { "Assets" });
            string[] foundDataNames = new string[foundDataGUIDs.Length];
            List<int> foundIDs = new List<int>();

            for (int i = 0; i < foundDataGUIDs.Length; i++)
            {
                foundDataNames[i] = AssetDatabase.GUIDToAssetPath(foundDataGUIDs[i]).Replace(".asset", "");

                string[] splittedName = foundDataNames[i].Split(' ');
                int resultId;
                if (int.TryParse(splittedName[splittedName.Length - 1], out resultId))
                    foundIDs.Add(resultId);
            }

            if (foundIDs.Count == 0)
                return " 1";

            foundIDs.Sort();

            int finalId = 1;
            for (int i = 0; i < foundIDs.Count; i++)
            {
                if (finalId != foundIDs[i])
                    break;

                if (foundIDs[i] == finalId)
                    finalId++;
            }

            return " " + finalId;
        }

        static string GetFullAssetName(string name, SurfaceData data)
        {
            return string.Format("{0}_{1}_{2}", GENERAL_ASSET_NAME, name, data.GetInstanceID());
        }
#endregion
    }

    internal struct DebugMessage
    {
        public string Message;
        public MessageType Type;

        public DebugMessage(string message, MessageType type)
        {
            Message = message;
            Type = type;
        }
    }
}


