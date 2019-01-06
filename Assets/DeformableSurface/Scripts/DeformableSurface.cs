#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;
using DS;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
[DisallowMultipleComponent]
public class DeformableSurface : MonoBehaviour
{
    protected const string VERSION = "1.0";

    #region Static
    public static class DispatchKernels
    {
        public const int GROUP_THREADS_COUNT = 8;

        public const int GEN_NORMAL = 0;
        public const int DEFORM = 1;
        public const int BLUR_H = 2;
        public const int BLUR_V = 3;
        public const int NORM_DEPTH = 4;
        public const int DENORM_DEPTH = 5;
        public const int GET_OFFSET = 6;
    }
    
    public static List<DeformableSurface> Instances { get { return _instances; } }
    private static List<DeformableSurface> _instances = new List<DeformableSurface>();
    #endregion

    #region Public
    /// <summary>
    /// Original data asset
    /// </summary>
    public SurfaceData Data
    {
        get { return _data; }
        set
        {
            if (value == null)
                return;
            
            MeshFilter.sharedMesh = value.MeshBase;
            MeshRenderer.sharedMaterial = value.SourceMaterial;
            MeshCollider.sharedMesh = value.MeshCollider;
            

            _data = value;
        }
    }
    [SerializeField] private SurfaceData _data;

    /// <summary>
    /// Enables or disables deformation. Set this to false if there is no need to perform deformation for performance
    /// </summary>
    public bool DeformEnabled
    {
        get { return _deformEnabled; }
        set
        {
            if (_depthCamera != null)
                _depthCamera.enabled = value;
            _deformEnabled = value;
        }
    }
    [SerializeField] private bool _deformEnabled = true;
    
    /// <summary>
    /// ARGB32 render texture used by depth camera to send deformation data
    /// </summary>
    public RenderTexture SrcMapRt { get; set; }

    /// <summary>
    /// ARGB32 render texture used by compute shader and changes based on <see cref="SrcMapRt"/>. R - height, G - offset, BA - normal
    /// </summary>
    public RenderTexture BaseMapRt { get; set; }
    #endregion

    #region Refs
    public MeshFilter MeshFilter
    {
        get
        {
            return _meshFilter == null ? _meshFilter = GetComponent<MeshFilter>() : _meshFilter;
        }
    }
    private MeshFilter _meshFilter;

    public MeshRenderer MeshRenderer
    {
        get { return _meshRenderer == null ? _meshRenderer = GetComponent<MeshRenderer>() : _meshRenderer; }
    }
    private MeshRenderer _meshRenderer;

    public MeshCollider MeshCollider
    {
        get { return _meshCollider == null ? _meshCollider = GetComponent<MeshCollider>() : _meshCollider; }
    }
    private MeshCollider _meshCollider;
    #endregion

    #region Private
    private ComputeShader _kernels;
    private DepthCamera _depthCamera;
    private MaterialPropertyBlock _matPb;

    private int _threadGroupsX;
    private int _threadGroupsY;
    #endregion


    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        if (!_instances.Contains(this))
            _instances.Add(this);
    }

    void Start()
    {
        Restore();
    }

    void OnDisable()
    {
        if (_instances.Contains(this))
            _instances.Remove(this);
    }

    void Init()
    {
        if (Data == null)
            return;

        _kernels = Data.SurfaceKernels;
        _matPb = new MaterialPropertyBlock();
        
        SrcMapRt = GetNewRt("DS_SrcMap_runtime", RenderTextureFormat.ARGB32, FilterMode.Bilinear);
        BaseMapRt = GetNewRt("DS_BaseMap_runtime", RenderTextureFormat.ARGB32, FilterMode.Bilinear);
        Graphics.Blit(Data.BaseMap, BaseMapRt);
        _matPb.SetTexture(SurfaceData.ShaderProperties.BASE_MAP, BaseMapRt);
        MeshRenderer.SetPropertyBlock(_matPb);

        _depthCamera = DepthCamera.GetInstance(this, Deform);

        InitKernelsCore();
        InitKernelsSmoothing();

        _threadGroupsX = Data.BaseMap.width / DispatchKernels.GROUP_THREADS_COUNT;
        _threadGroupsY = Data.BaseMap.height / DispatchKernels.GROUP_THREADS_COUNT;
    }


    void InitKernelsCore()
    {
        _kernels.SetFloats("RtDimensions", BaseMapRt.width - 1, BaseMapRt.height - 1);
        _kernels.SetFloat("NormalIntensity", Data.NormalIntensity);
        _kernels.SetFloats("MinMaxFrac", Data.MaxHeight / (Data.MaxHeight + Data.MaxOffset), Data.MaxOffset / (Data.MaxHeight + Data.MaxOffset));

        _kernels.SetTexture(DispatchKernels.DEFORM, "BaseMap", BaseMapRt);
        _kernels.SetTexture(DispatchKernels.DEFORM, "SrcMap", SrcMapRt);
        _kernels.SetTexture(DispatchKernels.GEN_NORMAL, "BaseMap", BaseMapRt);
        _kernels.SetTexture(DispatchKernels.GET_OFFSET, "BaseMap", BaseMapRt);
    }

    void InitKernelsSmoothing()
    {
        if (!Data.Smoothing)
            return;

        _kernels.SetFloat("BlurRadius", Data.SmoothingRadius);
        _kernels.SetTexture(DispatchKernels.BLUR_H, "SrcMap", SrcMapRt);
        _kernels.SetTexture(DispatchKernels.BLUR_V, "SrcMap", SrcMapRt);
        _kernels.SetTexture(DispatchKernels.NORM_DEPTH, "SrcMap", SrcMapRt);
        _kernels.SetTexture(DispatchKernels.NORM_DEPTH, "BaseMap", BaseMapRt);
        _kernels.SetTexture(DispatchKernels.DENORM_DEPTH, "SrcMap", SrcMapRt);
        _kernels.SetTexture(DispatchKernels.DENORM_DEPTH, "BaseMap", BaseMapRt);
    }

    void DispatchComputeFull(int kernelId)
    {
        _kernels.Dispatch(kernelId, _threadGroupsX, _threadGroupsY, 1);
    }
    
    RenderTexture GetNewRt(string rtName, RenderTextureFormat format, FilterMode filter)
    {
        RenderTexture rt = new RenderTexture(Data.BaseMap.width, Data.BaseMap.height, 0, format, RenderTextureReadWrite.Linear)
        {
            name = rtName,
            wrapMode = TextureWrapMode.Clamp,
            autoGenerateMips = false,
            enableRandomWrite = true,
            filterMode = filter,
        };
        rt.Create();

        return rt;
    }
    
    void Deform()
    {
        if (!DeformEnabled || !enabled)
            return;

        if (_instances.Count > 1)
        {
            InitKernelsCore();
            InitKernelsSmoothing();
        }
            
        if (Data.Smoothing)
        {
            DispatchComputeFull(DispatchKernels.NORM_DEPTH);
            _kernels.SetInt("BlurRadius", Data.SmoothingRadius - 1);
            for (int i = 0; i < Data.SmoothingPasses; i++)
            {
                DispatchComputeFull(DispatchKernels.BLUR_H);
                DispatchComputeFull(DispatchKernels.BLUR_V);
            }
            DispatchComputeFull(DispatchKernels.DENORM_DEPTH);
        }

        _kernels.SetFloat("DeformSpeed", Mathf.Clamp01(Data.DeformSpeed * Time.deltaTime * 100));
        DispatchComputeFull(DispatchKernels.DEFORM);
        DispatchComputeFull(DispatchKernels.GEN_NORMAL);
    }


    /// <summary>
    /// Removes all changes done in deformable part and restores initial state of the surface
    /// </summary>
    public void Restore()
    {
        Graphics.Blit(Data.BaseMap, BaseMapRt);
    }

    /// <summary>
    /// Returns offset value clamped between 0 and 1 at given point. Returns -1 if the point does not touch surface offset
    /// </summary>
    /// <param name="point">Position in world space</param>
    /// <returns>Normalized offset value. Returns -1 if point does not tuch surface offset</returns>
    public float GetCollisionOffset(Vector3 point)
    {
        if (!MeshRenderer.bounds.Contains(point))
            return -1;

        OffsetValue[] ovData = new OffsetValue[1];
        Vector3 localPos = point - transform.position;
        ovData[0].InPos = new Vector3(
            (localPos.x / MeshRenderer.bounds.size.x) * Data.BaseMap.width,
            localPos.y / MeshRenderer.bounds.size.y,
            (localPos.z / MeshRenderer.bounds.size.z) * Data.BaseMap.height);

        ComputeBuffer buffer = new ComputeBuffer(1, 16);
        buffer.SetData(ovData);
        Data.SurfaceKernels.SetBuffer(DispatchKernels.GET_OFFSET, "OutOffset", buffer);
        Data.SurfaceKernels.Dispatch(DispatchKernels.GET_OFFSET, 1, 1, 1);
        buffer.GetData(ovData);
        buffer.Release();

        return ovData[0].OutValue;
    }

    /// <summary>
    /// Returns offset values at given points. Use this instead of <see cref="GetCollisionOffset"/> for the large amount of points for performance. 1024 points maximum
    /// </summary>
    /// <param name="points">Positions in world space</param>
    /// <returns>Normalized offset values. Returns -1 if point does not tuch surface offset</returns>
    public float[] GetCollisionOffsets(params Vector3[] points)
    {
        if (points == null || points.Length == 0 || points.Length > 1024)
            return null;

        OffsetValue[] ovData = new OffsetValue[points.Length];
        for (int i = 0; i < ovData.Length; i++)
        {
            Vector3 localPos = points[i] - transform.position;
            ovData[i].InPos = new Vector3(
                (localPos.x / MeshRenderer.bounds.size.x) * Data.BaseMap.width,
                localPos.y / MeshRenderer.bounds.size.y,
                (localPos.z / MeshRenderer.bounds.size.z) * Data.BaseMap.height);
        }
        
        ComputeBuffer buffer = new ComputeBuffer(ovData.Length, OffsetValue.STRUCT_SIZE);
        buffer.SetData(ovData);
        Data.SurfaceKernels.SetBuffer(DispatchKernels.GET_OFFSET, "OutOffset", buffer);
        Data.SurfaceKernels.Dispatch(DispatchKernels.GET_OFFSET, ovData.Length, 1, 1);
        buffer.GetData(ovData);
        buffer.Release();

        float[] offsetValues = new float[ovData.Length];
        for (int i = 0; i < offsetValues.Length; i++)
            offsetValues[i] = ovData[i].OutValue;

        return offsetValues;
    }
    

    struct OffsetValue
    {
        public const int STRUCT_SIZE = 16;

        public Vector3 InPos;
        public float OutValue;
    }
}