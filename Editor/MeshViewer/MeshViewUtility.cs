namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System.Text;
    using UnityEngine;
    using UnityEngine.Rendering;

    internal static class MeshViewUtility
    {
        public static Color GetSubMeshColor(int index)
        {
            var hue = Mathf.Repeat(index * 0.618f, 1);
            var saturation = index == 0 ? 0.0f : 0.3f;
            const float value = 1.0f;

            return Color.HSVToRGB(hue, saturation, value);
        }
        
        public static string GetMeshInfo(Mesh mesh)
        {
            if (mesh == null)
                return "None\n0 Vertices, 0 Triangles | None";

            var stringBuilder = new StringBuilder();
            for (var i = 4; i < 12; i++)
            {
                if (mesh.HasVertexAttribute((VertexAttribute) i))
                {
                    stringBuilder.Append($"UV{i - 3} |");
                }
            }

            var uvInfo = stringBuilder.ToString().TrimEnd('|');
            if (string.IsNullOrEmpty(uvInfo))
                uvInfo = "None";

            return $"{mesh.name}\n{mesh.vertexCount} Vertices, {mesh.triangles.Length/3} Triangles | {uvInfo}";
        }
    }
}