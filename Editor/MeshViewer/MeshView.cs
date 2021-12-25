namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal sealed class MeshView : IDisposable
    {
        private readonly Mesh _target;
        private readonly MeshViewSettings _meshViewSettings;

        private readonly PreviewRenderUtility _previewRender;
        
        private static Event CurrentEvent => Event.current;

        public MeshView(Mesh target)
        {
            _target = target;
            
            _meshViewSettings = new MeshViewSettings();
            
            _previewRender = new PreviewRenderUtility();
        }

        public void OnGUI(Rect rect)
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
            RenderMesh(rect);
            _previewRender.EndAndDrawPreview(rect);
        }

        public void Dispose()
        {
            _previewRender.Cleanup();
            _meshViewSettings.Dispose();
        }

        private void RenderMesh(Rect rect)
        {
            if(_target == null)
                return;
            
            // perspective camera
            InitializePerspectiveCamera(_previewRender.camera);
            HandlePerspectiveCameraPan(rect);

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
            var position = rotation * Vector3.forward * (-distance * _meshViewSettings.Zoom) +
                           _meshViewSettings.PivotOffset;

            var cameraTransform = camera.transform;
            cameraTransform.position = position;
            cameraTransform.rotation = rotation;

            InitializeLights();
        }

        private void InitializeLights()
        {
            var firstLight = _previewRender.lights[0];
            firstLight.intensity = MeshViewSettings.LightIntensity;
            firstLight.transform.rotation = Quaternion.Euler(_meshViewSettings.LightDirection.y, _meshViewSettings.LightDirection.x, 0.0f);

            var secondLight = _previewRender.lights[1];
            secondLight.intensity = MeshViewSettings.LightIntensity;
            secondLight.transform.rotation = Quaternion.Euler(-_meshViewSettings.LightDirection.y, -_meshViewSettings.LightDirection.x, 0.0f);

            _previewRender.ambientColor = MeshViewSettings.AmbientColor;
        }

        private void RenderShadedMesh(Camera camera)
        {
            var rotation = Quaternion.Euler(_meshViewSettings.Direction.y, 0.0f, 0.0f) *
                           Quaternion.Euler(0.0f, _meshViewSettings.Direction.x, 0.0f);
            var position = rotation * -_target.bounds.center;

            var previousFogFlag = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);

            if (_meshViewSettings.ShadedMaterial != null)
            {
                var subMeshCount = _target.subMeshCount;
                var materialPropertyBlock = new MaterialPropertyBlock();

                camera.clearFlags = CameraClearFlags.Nothing;

                for (var i = 0; i < subMeshCount; i++)
                {
                    materialPropertyBlock.SetColor(MeshViewSettings.ColorPropertyId, MeshViewUtility.GetSubMeshColor(i));
                    _previewRender.DrawMesh(_target, position, rotation, _meshViewSettings.ShadedMaterial, i, materialPropertyBlock);
                }
                
                _previewRender.Render();
            }

            Unsupported.SetRenderSettingsUseFogNoDirty(previousFogFlag);
        }

        private void HandlePerspectiveCameraPan(Rect rect)
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
                        _meshViewSettings.Direction -= delta;
                        CurrentEvent.Use();
                        GUI.changed = true;
                    }

                    break;
            }
        }
    }

    internal class MeshViewSettings : IDisposable
    {
        private Material _shadedMaterial;
        
        public const float DefaultRotationSpeed = 1.0f;
        public const float ShiftRotationSpeed = 3.0f;

        public const float AspectMultiplier = 140.0f;
        
        public const float FieldOfView = 30.0f;
        
        public const float NearClipPlane = 0.0001f;
        public const float FarClipPlane = 1000.0f;
        
        public static readonly Color AmbientColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        public const float LightIntensity = 1.1f;

        public static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        public float Zoom = 1.0f;
        
        public Vector3 PivotOffset = Vector3.zero;
        
        public Vector2 LightDirection = new Vector2(-40, -40);
        
        public Vector2 Direction = new Vector2(130.0f, 0.0f);
        
        public Material ShadedMaterial
        {
            get
            {
                if (_shadedMaterial == null)
                    _shadedMaterial = CreateShadedMaterial();

                return _shadedMaterial;
            }
        }
        
        private static Material CreateShadedMaterial()
        {
            return new Material(Shader.Find("Standard"));
        }

        public void Dispose()
        {
            if(_shadedMaterial != null)
                Object.DestroyImmediate(_shadedMaterial);
        }
    }

    internal static class MeshViewStyles
    {
        public const string RenderTextureNotSupportedMessage = "Mesh view requires render texture support";
        
        public const float RenderTextureNotSupportedWidth = 300.0f;
        public const float RenderTextureNotSupportedHeight = 40.0f;
    }

    internal static class MeshViewUtility
    {
        public static Color GetSubMeshColor(int index)
        {
            var hue = Mathf.Repeat(index * 0.618f, 1);
            var saturation = index == 0 ? 0.0f : 0.3f;
            const float value = 1.0f;

            return Color.HSVToRGB(hue, saturation, value);
        }
    }
}