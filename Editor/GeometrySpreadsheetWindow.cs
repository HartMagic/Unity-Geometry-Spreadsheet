namespace GeometrySpreadsheet.Editor
{
    using SplitArea;
    using UnityEditor;
    using UnityEngine;

    public sealed class GeometrySpreadsheetWindow : EditorWindow
    {
        private VerticalSplitArea _verticalSplitArea;
        
        private Mesh _selectedMesh;
    
        [MenuItem("Window/Geometry Spreadsheet")]
        internal static void ShowWindow()
        {
            var window = GetWindow<GeometrySpreadsheetWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            _verticalSplitArea?.OnGUI();
        }

        private void OnEnable()
        {
            Initialize();
            Repaint();
        }

        private void OnDisable()
        {
        
        }

        private void Initialize()
        {
            titleContent = GeometrySpreadsheetStyles.TitleContent;
            minSize = new Vector2(256.0f, 256.0f);
            
            _verticalSplitArea = new VerticalSplitArea(this);
        }

        private void HandleSplitArea()
        {
            
        }
    }
}