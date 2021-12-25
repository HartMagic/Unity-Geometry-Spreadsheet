namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

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

        public const float MinZoom = 0.1f;
        public const float MaxZoom = 10.0f;

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
}