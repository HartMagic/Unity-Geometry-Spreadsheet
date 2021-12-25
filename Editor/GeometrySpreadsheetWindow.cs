namespace GeometrySpreadsheet.Editor
{
    using MeshViewer;
    using SplitArea;
    using UnityEditor;
    using UnityEngine;

    public sealed class GeometrySpreadsheetWindow : EditorWindow
    {
        private VerticalSplitArea _verticalSplitArea;
        private MeshView _meshView;
        
        private Mesh _selectedMesh;
    
        [MenuItem("Window/Geometry Spreadsheet")]
        internal static void ShowWindow()
        {
            var window = GetWindow<GeometrySpreadsheetWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            if(_verticalSplitArea == null)
                return;
            
            _verticalSplitArea.OnGUI();
            
            _meshView?.OnGUI(_verticalSplitArea.FirstRect);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            
            Initialize();
            Repaint();
        }
        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            
            Dispose();
        }

        private void Initialize()
        {
            titleContent = GeometrySpreadsheetStyles.TitleContent;
            minSize = new Vector2(256.0f, 256.0f);
            
            _verticalSplitArea = new VerticalSplitArea(this);
        }

        private void Dispose()
        {
            _meshView?.Dispose();
        }
        
        private void OnSelectionChanged()
        {
            if (Selection.activeObject == null || !(Selection.activeObject is Mesh selectedMesh))
            {
                return;
            }

            _selectedMesh = selectedMesh;

            CreateOrUpdateMeshView();
            
            Repaint();
        }

        private void CreateOrUpdateMeshView()
        {
            _meshView?.Dispose();

            _meshView = new MeshView(_selectedMesh);
        }
    }
}