namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    internal sealed class MeshView : IDisposable
    {
        private Mesh _target;
        
        private readonly MeshViewSettings _meshViewSettings;
        private readonly PreviewRenderUtility _previewRender;
        
        private float _zoom = 1.0f;
        
        private Vector3 _pivotOffset = Vector3.zero;
        private  Vector3 _position = new Vector3(0.5f, 0.5f, -1.0f);
        
        private Vector2 _lightDirection = new Vector2(-40, -40);
        
        private Vector2 _direction = new Vector2(130.0f, 0.0f);
        
        private static Event CurrentEvent => Event.current;

        public MeshView(Mesh target)
        {
            _target = target;
            
            _meshViewSettings = new MeshViewSettings();
            
            _previewRender = new PreviewRenderUtility();
        }

        public void SetTarget(Mesh target)
        {
            _target = target;
        }

        public void OnGUI(Rect rect)
        {
            var settingsRect = new Rect(rect.x, rect.yMax - MeshViewStyles.SettingsPanelHeight, rect.width, MeshViewStyles.SettingsPanelHeight);
            DrawSettingsPanel(settingsRect);
            
            var meshViewRect = new Rect(rect.x, rect.y, rect.width, rect.height - MeshViewStyles.SettingsPanelHeight);
            DrawMeshView(meshViewRect);
        }

        public void Dispose()
        {
            _previewRender.Cleanup();
            _meshViewSettings.Dispose();
        }
        
        #region SettingsPanelRendering

        private void DrawSettingsPanel(Rect rect)
        {
            GUI.enabled = ShaderUtil.hardwareSupportsRectRenderTexture && _target != null;
            {
                GUI.Box(rect, string.Empty, EditorStyles.inspectorDefaultMargins);

                GUILayout.BeginArea(rect);
                {
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();

                    var wireframeToggleWidth = EditorStyles.toolbarButton.CalcSize(MeshViewStyles.WireframeToggle).x;
                    var wireframeToggleRect = EditorGUILayout.GetControlRect(GUILayout.Width(wireframeToggleWidth));

                    _meshViewSettings.IsWireframeShowed = GUI.Toggle(wireframeToggleRect,
                        _meshViewSettings.IsWireframeShowed, MeshViewStyles.WireframeToggle,
                        EditorStyles.toolbarButton);

                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
            GUI.enabled = true;
        }

        #endregion

        #region MeshRendering

        private void DrawMeshView(Rect rect)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (CurrentEvent.type == EventType.Repaint)
                {
                    var halfWidth = rect.width * 0.5f;
                    var halfHeight = rect.height * 0.5f;
                    
                    var x = rect.x + halfWidth - MeshViewStyles.RenderTextureNotSupportedWidth * 0.5f;
                    var y = rect.y + halfHeight - MeshViewStyles.RenderTextureNotSupportedHeight * 0.5f;
                    
                    EditorGUI.DropShadowLabel(new Rect(x, y, MeshViewStyles.RenderTextureNotSupportedWidth, MeshViewStyles.RenderTextureNotSupportedHeight), 
                        MeshViewStyles.RenderTextureNotSupportedMessage);
                }

                return;
            }

            _previewRender.BeginPreview(rect, GUIStyle.none);
            DrawMesh(rect);
            _previewRender.EndAndDrawPreview(rect);
            
            DrawMeshInfo(rect);
        }

        private void DrawMeshInfo(Rect rect)
        {
            var meshInfo = MeshViewUtility.GetMeshInfo(_target);

            EditorGUI.DropShadowLabel(new Rect(rect.x, rect.yMax - (MeshViewStyles.MeshInfoHeight + MeshViewStyles.MeshInfoMargin), 
                rect.width, MeshViewStyles.MeshInfoHeight), meshInfo);
        }

        private void DrawMesh(Rect rect)
        {
            if(_target == null)
                return;
            
            // perspective camera
            InitializePerspectiveCamera(_previewRender.camera);
            
            if(CurrentEvent.button == 0)
                _direction = HandlePerspectiveRotation(_direction, rect);
            if (CurrentEvent.button == 1)
                _lightDirection = HandlePerspectiveRotation(_lightDirection, rect);

            if(CurrentEvent.button == 2)
                HandlePerspectivePan(rect, _previewRender.camera);
            
            HandleZoom(rect, _previewRender.camera);

            RenderShadedMesh(_previewRender.camera);
        }

        private void InitializePerspectiveCamera(Camera camera)
        {
            camera.orthographic = false;
            
            camera.nearClipPlane = MeshViewSettings.NearClipPlane;
            camera.farClipPlane = MeshViewSettings.FarClipPlane;
            camera.fieldOfView = MeshViewSettings.FieldOfView;

            var rotation = Quaternion.identity;

            var distance = 4.0f * _target.bounds.extents.magnitude;
            var position = rotation * Vector3.forward * (-distance * _zoom) +
                           _pivotOffset;

            var cameraTransform = camera.transform;
            cameraTransform.position = position;
            cameraTransform.rotation = rotation;

            InitializeLights();
        }

        private void InitializeLights()
        {
            var firstLight = _previewRender.lights[0];
            firstLight.intensity = MeshViewSettings.LightIntensity;
            firstLight.transform.rotation = Quaternion.Euler(_lightDirection.y, _lightDirection.x, 0.0f);

            var secondLight = _previewRender.lights[1];
            secondLight.intensity = MeshViewSettings.LightIntensity;
            secondLight.transform.rotation = Quaternion.Euler(-_lightDirection.y, -_lightDirection.x, 0.0f);

            _previewRender.ambientColor = MeshViewSettings.AmbientColor;
        }

        private void RenderShadedMesh(Camera camera)
        {
            var rotation = Quaternion.Euler(_direction.y, 0.0f, 0.0f) *
                           Quaternion.Euler(0.0f, _direction.x, 0.0f);
            var position = rotation * -_target.bounds.center;

            var previousFogFlag = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);

            DrawSolidMesh(camera, position, rotation);
            DrawWireframeMesh(camera, position, rotation);

            Unsupported.SetRenderSettingsUseFogNoDirty(previousFogFlag);
        }

        private void DrawSolidMesh(Camera camera, Vector3 position, Quaternion rotation)
        {
            if(_meshViewSettings.ShadedMaterial == null)
                return;
            
            var materialPropertyBlock = new MaterialPropertyBlock();

            camera.clearFlags = CameraClearFlags.Nothing;

            for (var i = 0; i < _target.subMeshCount; i++)
            {
                materialPropertyBlock.SetColor(MeshViewSettings.ColorPropertyId, MeshViewUtility.GetSubMeshColor(i));
                _previewRender.DrawMesh(_target, position, rotation, _meshViewSettings.ShadedMaterial, i, materialPropertyBlock);
            }
                
            _previewRender.Render();
        }

        private void DrawWireframeMesh(Camera camera, Vector3 position, Quaternion rotation)
        {
            if(_meshViewSettings.WireframeMaterial == null || !_meshViewSettings.IsWireframeShowed)
                return;
            
            var materialPropertyBlock = new MaterialPropertyBlock();

            camera.clearFlags = CameraClearFlags.Nothing;
            
            GL.wireframe = true;
            {
                materialPropertyBlock.SetColor(MeshViewSettings.ColorPropertyId,
                    _meshViewSettings.WireframeMaterial.color);

                for (var i = 0; i < _target.subMeshCount; i++)
                {
                    var topology = _target.GetTopology(i);
                    if (topology == MeshTopology.Lines || topology == MeshTopology.LineStrip ||
                        topology == MeshTopology.Points)
                        continue;

                    _previewRender.DrawMesh(_target, position, rotation, _meshViewSettings.ShadedMaterial, i,
                        materialPropertyBlock);
                }

                _previewRender.Render();
            }
            GL.wireframe = false;
        }

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

        private void HandlePerspectivePan(Rect rect, Camera camera)
        {
            if(CurrentEvent.type != EventType.MouseDrag || !rect.Contains(CurrentEvent.mousePosition))
                return;
            
            var delta = new Vector3(-CurrentEvent.delta.x * camera.pixelWidth / rect.width,
                CurrentEvent.delta.y * camera.pixelHeight / rect.height, 0.0f);

            var screenPosition = camera.WorldToScreenPoint(_pivotOffset);
            screenPosition += delta;
            var worldPosition = camera.ScreenToWorldPoint(screenPosition) - _pivotOffset;

            _pivotOffset += worldPosition;
            
            CurrentEvent.Use();
        }

        private void HandleZoom(Rect rect, Camera camera)
        {
            if (CurrentEvent.type != EventType.ScrollWheel)
                return;
            
            var zoomDelta = -(HandleUtility.niceMouseDeltaZoom * 0.5f) * 0.05f;
            var newZoom = _zoom + _zoom * zoomDelta;
            newZoom = Mathf.Clamp(newZoom, MeshViewSettings.MinZoom, MeshViewSettings.MaxZoom);
            
            var mouseViewPosition = new Vector2(CurrentEvent.mousePosition.x / rect.width, 1.0f - CurrentEvent.mousePosition.y / rect.height);
            var mouseWorldPosition = camera.ViewportToWorldPoint(mouseViewPosition);
            var mouseToCameraPosition = _position - mouseWorldPosition;
            var cameraPosition = mouseWorldPosition + mouseToCameraPosition * (newZoom / _zoom);
            
            camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);

            _zoom = newZoom;
            CurrentEvent.Use();
        }
        
        #endregion
    }
}