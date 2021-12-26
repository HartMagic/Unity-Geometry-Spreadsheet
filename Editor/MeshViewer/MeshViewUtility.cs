namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public static class MeshViewUtility
    {
        public static IEnumerable<(string channel, bool isAvailable)> GetAvailableUvChannels(Mesh mesh)
        {
            for (var i = 4; i < 12; i++)
            {
                var isAvailable = mesh.HasVertexAttribute((VertexAttribute) i);
                yield return ($"Channel {i - 4}", isAvailable);
            }
        }

        public static void DrawPopup(Rect popupRect, string[] elements, int selectedIndex,
            GenericMenu.MenuFunction2 func, bool[] disabledItems)
        {
            var genericMenu = new GenericMenu();
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                if(Selection.count > 1)
                    continue;

                if (disabledItems == null || disabledItems[i])
                {
                    genericMenu.AddItem(new GUIContent(element), i == selectedIndex, func, i);
                }
                else
                {
                    genericMenu.AddDisabledItem(new GUIContent(element));
                }
            }
            
            genericMenu.DropDown(popupRect);
        }

        public static Color GetSubMeshColor(int index)
        {
            var hue = Mathf.Repeat(index * 0.618f, 1);
            var saturation = index == 0 ? 0.0f : 0.3f;
            const float value = 1.0f;

            return Color.HSVToRGB(hue, saturation, value);
        }
        
        public static string GetMaxString(IEnumerable<string> strings)
        {
            var maxLength = int.MinValue;
            var maxString = string.Empty;

            foreach (var stringValue in strings)
            {
                if (stringValue.Length > maxLength)
                {
                    maxLength = stringValue.Length;
                    maxString = stringValue;
                }
            }

            return string.IsNullOrEmpty(maxString) ? "None" : maxString;
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

            var subMeshesInfo = string.Empty;
            if (mesh.subMeshCount > 1)
            {
                subMeshesInfo = $", {mesh.subMeshCount} Sub Meshes";
            }

            var uvInfo = stringBuilder.ToString().TrimEnd('|');
            if (string.IsNullOrEmpty(uvInfo))
                uvInfo = "None";

            return $"{mesh.name}\n{mesh.vertexCount} Vertices, {mesh.triangles.Length/3} Triangles{subMeshesInfo} | {uvInfo}";
        }
    }
}