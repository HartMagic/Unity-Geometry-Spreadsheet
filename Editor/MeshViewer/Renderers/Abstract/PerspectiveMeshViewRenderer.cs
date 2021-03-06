namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers.Abstract
{
    using UnityEditor;
    using UnityEngine;

    public abstract class PerspectiveMeshViewRenderer : MeshViewRenderer
    {
        protected PerspectiveMeshViewRenderer(MeshViewRenderer wireframeOverride) : base(wireframeOverride)
        {
        }

        public override void InitializeCamera()
        {
            Camera.orthographic = false;
            
            Camera.nearClipPlane = MeshViewSettings.NearClipPlane;
            Camera.farClipPlane = MeshViewSettings.FarClipPlane;
            Camera.fieldOfView = MeshViewSettings.FieldOfView;

            var rotation = Quaternion.identity;

            var distance = 4.0f * Target.bounds.extents.magnitude;
            var position = rotation * Vector3.forward * (-distance * RenderState.Zoom) + RenderState.PivotOffset;

            var cameraTransform = Camera.transform;
            cameraTransform.position = position;
            cameraTransform.rotation = rotation;
        }

        public override void InitializeLights()
        {
            var firstLight = RenderContext.lights[0];
            firstLight.intensity = MeshViewSettings.LightIntensity;
            firstLight.transform.rotation = Quaternion.Euler(RenderState.LightDirection.y, RenderState.LightDirection.x, 0.0f);

            var secondLight = RenderContext.lights[1];
            secondLight.intensity = MeshViewSettings.LightIntensity;
            secondLight.transform.rotation = Quaternion.Euler(-RenderState.LightDirection.y, -RenderState.LightDirection.x, 0.0f);

            RenderContext.ambientColor = MeshViewSettings.AmbientColor;
        }

        public override void HandleUserInput(Rect rect)
        {
            if(CurrentEvent.button == 0)
                RenderState.Direction = HandlePerspectiveRotation(RenderState.Direction, rect);
            if (CurrentEvent.button == 1)
                RenderState.LightDirection = HandlePerspectiveRotation(RenderState.LightDirection, rect);

            if(CurrentEvent.button == 2)
                HandlePerspectivePan(rect);
            
            HandleZoom(rect);
        }
        
        protected sealed override void Render()
        {
            var rotation = Quaternion.Euler(RenderState.Direction.y, 0.0f, 0.0f) *
                           Quaternion.Euler(0.0f, RenderState.Direction.x, 0.0f);
            var position = rotation * -Target.bounds.center;

            var previousFogFlag = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            
            Camera.clearFlags = CameraClearFlags.Nothing;

            var materialPropertyBlock = new MaterialPropertyBlock();

            RenderInternal(position, rotation, materialPropertyBlock);
            
            Unsupported.SetRenderSettingsUseFogNoDirty(previousFogFlag);
        }

        protected abstract void RenderInternal(Vector3 position, Quaternion rotation, MaterialPropertyBlock materialPropertyBlock);
        
        private Vector2 HandlePerspectiveRotation(Vector2 direction, Rect rect)
        {
            var controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            switch (CurrentEvent.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (rect.Contains(CurrentEvent.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        CurrentEvent.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                        GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        var delta = CurrentEvent.delta * (CurrentEvent.shift ? MeshViewSettings.ShiftRotationSpeed : MeshViewSettings.DefaultRotationSpeed) /
                            Mathf.Min(rect.width, rect.height) * MeshViewSettings.AspectMultiplier;
                        direction -= delta;
                        CurrentEvent.Use();
                        GUI.changed = true;
                    }

                    break;
            }

            return direction;
        }

        private void HandlePerspectivePan(Rect rect)
        {
            if(CurrentEvent.type != EventType.MouseDrag || !rect.Contains(CurrentEvent.mousePosition))
                return;
            
            var delta = new Vector3(-CurrentEvent.delta.x * Camera.pixelWidth / rect.width,
                CurrentEvent.delta.y * Camera.pixelHeight / rect.height, 0.0f);

            var screenPosition = Camera.WorldToScreenPoint(RenderState.PivotOffset);
            screenPosition += delta;
            var worldPosition = Camera.ScreenToWorldPoint(screenPosition) - RenderState.PivotOffset;

            RenderState.PivotOffset += worldPosition;
            
            CurrentEvent.Use();
        }

        private void HandleZoom(Rect rect)
        {
            if (CurrentEvent.type != EventType.ScrollWheel || !rect.Contains(CurrentEvent.mousePosition))
                return;
            
            var zoomDelta = -(HandleUtility.niceMouseDeltaZoom * 0.5f) * 0.05f;
            var newZoom = RenderState.Zoom + RenderState.Zoom * zoomDelta;
            newZoom = Mathf.Clamp(newZoom, MeshViewSettings.MinZoom, MeshViewSettings.MaxZoom);
            
            var mouseViewPosition = new Vector2(CurrentEvent.mousePosition.x / rect.width, 1.0f - CurrentEvent.mousePosition.y / rect.height);
            var mouseWorldPosition = Camera.ViewportToWorldPoint(mouseViewPosition);
            var mouseToCameraPosition = RenderState.Position - mouseWorldPosition;
            var cameraPosition = mouseWorldPosition + mouseToCameraPosition * (newZoom / RenderState.Zoom);
            
            Camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);

            RenderState.Zoom = newZoom;
            CurrentEvent.Use();
        }
    }
}