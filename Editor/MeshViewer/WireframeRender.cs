namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using UnityEditor;
    using UnityEngine;

    public sealed class WireframeRender : PerspectiveMeshViewRender
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int ZWritePropertyId = Shader.PropertyToID("_ZWrite");
        private static readonly int ZBiasPropertyId = Shader.PropertyToID("_ZBias");
        
        public override string DisplayName => "Wireframe";
        
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

            GL.wireframe = true;
            {
                materialPropertyBlock.SetColor(ColorPropertyId, Material.color);

                for (var i = 0; i < Target.subMeshCount; i++)
                {
                    var topology = Target.GetTopology(i);
                    if (topology == MeshTopology.Lines || topology == MeshTopology.LineStrip ||
                        topology == MeshTopology.Points)
                        continue;

                    RenderContext.DrawMesh(Target, position, rotation, Material, i, materialPropertyBlock);
                }

                RenderContext.Render();
            }
            GL.wireframe = false;

            Unsupported.SetRenderSettingsUseFogNoDirty(previousFogFlag);
        }

        protected override Material CreateMaterial()
        {
            var shader = Shader.Find("Hidden/GeometrySpreadsheet/InternalDefault");
            if (shader == null)
            {
                Debug.LogWarning("Wireframe shader has not FOUND");
                return null;
            }

            var material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            material.SetColor(ColorPropertyId, new Color(0.0f, 0.0f, 0.0f, 0.3f));
            material.SetFloat(ZWritePropertyId, 0.0f);
            material.SetFloat(ZBiasPropertyId, -1.0f);

            return material;
        }
    }
}