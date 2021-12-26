namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers.Abstract
{
    using UnityEditor;
    using UnityEngine;

    public abstract class OrthographicMeshViewRenderer : MeshViewRenderer
    {
        protected OrthographicMeshViewRenderer(MeshViewRenderer wireframeOverride) : base(wireframeOverride)
        {
        }

        public override void InitializeCamera()
        {
            Camera.orthographic = true;
            
            Camera.nearClipPlane = MeshViewSettings.NearClipPlane;
            Camera.farClipPlane = MeshViewSettings.FarClipPlane;
            Camera.fieldOfView = MeshViewSettings.FieldOfView;

            Camera.orthographicSize = RenderState.Zoom;
            
            var cameraTransform = Camera.transform;
            cameraTransform.position = RenderState.Position;
            cameraTransform.rotation = Quaternion.identity;
        }

        public sealed override void InitializeLights()
        {
            // do nothing
        }

        public override void HandleUserInput(Rect rect)
        {
            if(CurrentEvent.button == 2)
                HandleOrthographicPan(rect);
            
            HandleZoom(rect, Camera);
        }
        
        private void HandleOrthographicPan(Rect rect)
        {
            if(CurrentEvent.type != EventType.MouseDrag || !rect.Contains(CurrentEvent.mousePosition))
                return;
            
            var delta = new Vector3(-CurrentEvent.delta.x * Camera.pixelWidth / rect.width,
                CurrentEvent.delta.y * Camera.pixelHeight / rect.height, 0.0f);

            var screenPosition = Camera.WorldToScreenPoint(RenderState.Position);
            screenPosition += delta;
            var worldPosition = Camera.ScreenToWorldPoint(screenPosition);

            RenderState.Position = worldPosition;
            
            CurrentEvent.Use();
        }
        
        private void HandleZoom(Rect rect, Camera camera)
        {
            if (CurrentEvent.type != EventType.ScrollWheel || !rect.Contains(CurrentEvent.mousePosition))
                return;
            
            var zoomDelta = -(HandleUtility.niceMouseDeltaZoom * 0.5f) * 0.05f;
            var newZoom = RenderState.Zoom + RenderState.Zoom * zoomDelta;
            newZoom = Mathf.Clamp(newZoom, MeshViewSettings.MinZoom, MeshViewSettings.MaxZoom);
            
            var mouseViewPosition = new Vector2(CurrentEvent.mousePosition.x / rect.width, 1.0f - CurrentEvent.mousePosition.y / rect.height);
            var mouseWorldPosition = camera.ViewportToWorldPoint(mouseViewPosition);
            var mouseToCameraPosition = RenderState.Position - mouseWorldPosition;
            var cameraPosition = mouseWorldPosition + mouseToCameraPosition * (newZoom / RenderState.Zoom);
            
            camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);

            RenderState.Zoom = newZoom;
            CurrentEvent.Use();
        }
    }
}