namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers
{
    using Abstract;
    using UnityEditor;
    using UnityEngine;

    public sealed class ShadedRender : PerspectiveMeshViewRender
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        
        public override string DisplayName => "Shaded";
        
        public ShadedRender(MeshViewRender wireframeOverride) : base(wireframeOverride)
        {
        }
        
        internal override void Render()
        {
            if(Material == null)
                return;
            
            var rotation = Quaternion.Euler(RenderState.Direction.y, 0.0f, 0.0f) *
                           Quaternion.Euler(0.0f, RenderState.Direction.x, 0.0f);
            var position = rotation * -Target.bounds.center;

            var previousFogFlag = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);

            var materialPropertyBlock = new MaterialPropertyBlock();

            Camera.clearFlags = CameraClearFlags.Nothing;

            for (var i = 0; i < Target.subMeshCount; i++)
            {
                materialPropertyBlock.SetColor(ColorPropertyId, MeshViewUtility.GetSubMeshColor(i));
                RenderContext.DrawMesh(Target, position, rotation, Material, i, materialPropertyBlock);
            }
                
            RenderContext.Render();

            Unsupported.SetRenderSettingsUseFogNoDirty(previousFogFlag);
        }

        protected override Material CreateMaterial()
        {
            return new Material(Shader.Find("Standard"));
        }
    }
}