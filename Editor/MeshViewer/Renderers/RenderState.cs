namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers
{
    using UnityEngine;

    public sealed class RenderState
    {
        public float Zoom = 1.0f;
        
        public Vector3 PivotOffset = Vector3.zero;
        public Vector3 Position = new Vector3(0.5f, 0.5f, -1.0f);
        
        public Vector2 LightDirection = new Vector2(-40, -40);
        
        public Vector2 Direction = new Vector2(130.0f, 0.0f);
    }
}