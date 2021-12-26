namespace GeometrySpreadsheet.Editor.MeshViewer
{
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

        public static readonly GUIContent UvChannelDropDown = EditorGUIUtility.TrTextContent("", "Change active UV channel");

        public const float UvTextureMultiplierSliderWidth = 80.0f;

        public static readonly GUIStyle PreSliderStyle = "preSlider";
        public static readonly GUIStyle PreSliderThumbStyle = "preSliderThumb";

        public static GUIContent GetDisplayModeContent(MeshViewRenderer currentRenderer)
        {
            if(currentRenderer == null)
                return new GUIContent("None", DisplayModeDropDown.tooltip);
            
            return new GUIContent(currentRenderer.DisplayName, DisplayModeDropDown.tooltip);
        }
    }
}