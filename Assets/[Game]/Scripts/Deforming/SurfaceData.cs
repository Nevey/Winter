using UnityEngine;

namespace Game.Deforming
{
    public class SurfaceData : ScriptableObject
    {
        [SerializeField] private SurfacePaint[] surfacePaints = new SurfacePaint[0];

        [SerializeField] private DeformPaint deformPaint;

        public SurfacePaint[] SurfacePaints
        {
            get => surfacePaints;
            set => surfacePaints = value;
        }
        
        public DeformPaint DeformPaint
        {
            get => deformPaint;
            set => deformPaint = value;
        }
    }
}