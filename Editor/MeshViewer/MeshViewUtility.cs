namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using UnityEngine;

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