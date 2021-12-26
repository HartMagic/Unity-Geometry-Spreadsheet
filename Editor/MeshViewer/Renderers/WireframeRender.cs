namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers
{
    using Abstract;
    using UnityEngine;

    public sealed class WireframeRender : PerspectiveMeshViewRender
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int ZWritePropertyId = Shader.PropertyToID("_ZWrite");
        private static readonly int ZBiasPropertyId = Shader.PropertyToID("_ZBias");
        
        public override string DisplayName => "Wireframe";
        
        public WireframeRender() : base(null)
        {
        }
        
        protected override void RenderInternal(Vector3 position, Quaternion rotation, MaterialPropertyBlock materialPropertyBlock)
        {
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