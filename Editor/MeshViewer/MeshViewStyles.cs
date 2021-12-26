namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using Renderers;
    using Renderers.Abstract;
    using UnityEditor;
    using UnityEngine;

    internal static class MeshViewStyles
    {
        public const string RenderTextureNotSupportedMessage = "Mesh view requires render texture support";
        
        public const float RenderTextureNotSupportedWidth = 300.0f;
        public const float RenderTextureNotSupportedHeight = 40.0f;

        public const float SettingsPanelHeight = 20.0f;

        public const float MeshInfoHeight = 40.0f;
        public const float MeshInfoMargin = 6.0f;

        public static readonly GUIContent WireframeToggle = EditorGUIUtility.TrTextContent("Wireframe", "Show wireframe");

        public static readonly GUIContent DisplayModeDropDown = EditorGUIUtility.TrTextContent("", "Change display mode");

        public static GUIContent GetDisplayModeContent(MeshViewRender currentRender)
        {
            if(currentRender == null)
                return new GUIContent("None", DisplayModeDropDown.tooltip);
            
            return new GUIContent(currentRender.DisplayName, DisplayModeDropDown.tooltip);
        }
    }
}