namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using UnityEngine;

    internal class MeshViewSettings
    {

        public const float DefaultRotationSpeed = 1.0f;
        public const float ShiftRotationSpeed = 3.0f;

        public const float AspectMultiplier = 140.0f;
        
        public const float FieldOfView = 30.0f;
        
        public const float NearClipPlane = 0.0001f;
        public const float FarClipPlane = 1000.0f;
        
        public static readonly Color AmbientColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        public const float LightIntensity = 1.1f;

        public const float MinZoom = 0.1f;
        public const float MaxZoom = 10.0f;
    }
}