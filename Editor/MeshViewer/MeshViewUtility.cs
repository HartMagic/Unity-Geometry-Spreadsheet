namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    internal static class MeshViewUtility
    {
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
        
        public static string GetMaxDisplayMode(string[] displayModes)
        {
            var maxLength = int.MinValue;
            var maxDisplayMode = string.Empty;

            foreach (var displayMode in displayModes)
            {
                if (displayMode.Length > maxLength)
                {
                    maxLength = displayMode.Length;
                    maxDisplayMode = displayMode;
                }
            }

            return string.IsNullOrEmpty(maxDisplayMode) ? "None" : maxDisplayMode;
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