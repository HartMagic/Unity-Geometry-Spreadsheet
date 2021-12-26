namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers
{
    using Abstract;
    using UnityEngine;

    public sealed class ShadedRender : PerspectiveMeshViewRender
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        
        public override string DisplayName => "Shaded";
        
        public ShadedRender(MeshViewRender wireframeOverride) : base(wireframeOverride)
        {
        }
        
        protected override void RenderInternal(Vector3 position, Quaternion rotation, MaterialPropertyBlock materialPropertyBlock)
        {
            for (var i = 0; i < Target.subMeshCount; i++)
            {
                materialPropertyBlock.SetColor(ColorPropertyId, MeshViewUtility.GetSubMeshColor(i));
                RenderContext.DrawMesh(Target, position, rotation, Material, i, materialPropertyBlock);
            }
                
            RenderContext.Render();
        }

        protected override Material CreateMaterial()
        {
            return new Material(Shader.Find("Standard"));
        }
    }
}