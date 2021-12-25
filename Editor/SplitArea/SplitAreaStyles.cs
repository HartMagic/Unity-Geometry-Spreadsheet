namespace GeometrySpreadsheet.Editor.SplitArea
{
    using UnityEditor;
    using UnityEngine;

    internal static class SplitAreaStyles
    {
        public static readonly Vector2 Margin = new Vector2(100.0f, 100.0f);
        
        public static Color SplitLineColor => EditorGUIUtility.isProSkin
            ? new Color(0.1725f, 0.1725f, 0.1725f, 1.0f)
            : new Color(0.513f, 0.513f, 0.513f, 1.0f);
    }
}