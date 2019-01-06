using System;
using UnityEngine;
using System.Collections;

namespace DS
{
    [RequireComponent(typeof(Camera))]
    public class DepthCamera : MonoBehaviour
    {
        #region Private
        private Material _depthMat;
        private DeformableSurface _sourceSurface;
        private Camera _camera;
        private Action _onRenderImage;

        private bool[] _lastSurfaceRender;
        #endregion


        void Awake()
        {
            _camera = GetComponent<Camera>();
        }
        
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_sourceSurface == null)
                return;
            
            Graphics.Blit(src, dest, _depthMat);

            if (_onRenderImage != null)
                _onRenderImage();
        }

        void OnPreCull()
        {
            _camera.ResetWorldToCameraMatrix();
            _camera.ResetProjectionMatrix();
            _camera.projectionMatrix = _camera.projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));
        }

        void OnPreRender()
        {
            if (_sourceSurface == null)
                return;

            _lastSurfaceRender = new bool[DeformableSurface.Instances.Count];
            for (int i = 0; i < DeformableSurface.Instances.Count; i++)
            {
                _lastSurfaceRender[i] = DeformableSurface.Instances[i].MeshRenderer.enabled;
                DeformableSurface.Instances[i].MeshRenderer.enabled = false;
            }

            _camera.cullingMask = _sourceSurface.Data.DeformMask;
            GL.invertCulling = true;
        }

        void OnPostRender()
        {
            if (_sourceSurface == null)
                return;

            for (int i = 0; i < DeformableSurface.Instances.Count; i++)
                DeformableSurface.Instances[i].MeshRenderer.enabled = _lastSurfaceRender[i];

            GL.invertCulling = false;
        }


        public static DepthCamera GetInstance(DeformableSurface target, Action onRenderImage)
        {
            Camera cam = new GameObject("DS_RtCamera").AddComponent<Camera>();

            cam.gameObject.hideFlags = HideFlags.HideInHierarchy;
            cam.transform.parent = target.transform;

            cam.nearClipPlane = 0;
            Vector2 sizeXz = new Vector2(target.Data.MeshWidth / 2f, target.Data.MeshLength / 2f) * target.Data.MeshScale;
            cam.transform.localPosition = new Vector3(sizeXz.x, -cam.nearClipPlane, sizeXz.y);
            cam.transform.rotation = Quaternion.Euler(-90, 0, 0);
            cam.cullingMask = target.Data.DeformMask;
            cam.clearFlags = CameraClearFlags.Depth;
            cam.orthographic = true;
            cam.orthographicSize = (target.Data.MeshLength / 2f) * target.Data.MeshScale;
            cam.farClipPlane = target.Data.MaxHeight + target.Data.MaxOffset + cam.nearClipPlane;
            cam.depthTextureMode = DepthTextureMode.Depth;
            cam.renderingPath = RenderingPath.VertexLit;
            cam.targetTexture = target.SrcMapRt;

            cam.enabled = target.DeformEnabled;

            DepthCamera dc = cam.gameObject.AddComponent<DepthCamera>();
            dc._sourceSurface = target;
            dc._depthMat = new Material(target.Data.DepthTextureShader);
            dc._onRenderImage = onRenderImage;

            return dc;
        }
    }
}

