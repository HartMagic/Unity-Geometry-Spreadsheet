namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers.Abstract
{
    public abstract class OrthographicMeshViewRenderer : MeshViewRenderer
    {
        protected OrthographicMeshViewRenderer(MeshViewRenderer wireframeOverride) : base(wireframeOverride)
        {
        }
    }
}