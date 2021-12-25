namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal class MeshViewSettings : IDisposable
    {
        private Material _shadedMaterial;
        private Material _wireframeMaterial;
        
        public const float DefaultRotationSpeed = 1.0f;
        public const float ShiftRotationSpeed = 3.0f;

        public const float AspectMultiplier = 140.0f;
        
        public const float FieldOfView = 30.0f;
        
        public const float NearClipPlane = 0.0001f;
        public const float FarClipPlane = 1000.0f;
        
        public static readonly Color AmbientColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        public const float LightIntensity = 1.1f;

        public static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int ZWritePropertyId = Shader.PropertyToID("_ZWrite");
        private static readonly int ZBiasPropertyId = Shader.PropertyToID("_ZBias");

        public const float MinZoom = 0.1f;
        public const float MaxZoom = 10.0f;

        public bool IsWireframeShowed;
        

        public Material ShadedMaterial
        {
            get
            {
                if (_shadedMaterial == null)
                    _shadedMaterial = CreateShadedMaterial();

                return _shadedMaterial;
            }
        }

        public Material WireframeMaterial
        {
            get
            {
                if (_wireframeMaterial == null)
                    _wireframeMaterial = CreateWireframeMaterial();

                return _wireframeMaterial;
            }
        }

        private static Material CreateShadedMaterial()
        {
            return new Material(Shader.Find("Standard"));
        }

        private static Material CreateWireframeMaterial()
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

        public void Dispose()
        {
            if(_shadedMaterial != null)
                Object.DestroyImmediate(_shadedMaterial);
        }
    }
}