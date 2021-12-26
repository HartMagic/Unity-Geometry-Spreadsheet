namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    internal sealed class MeshView : IDisposable
    {
        private readonly List<MeshViewRender> _renders = new List<MeshViewRender>();

        private readonly RenderState _renderState = new RenderState();
        
        private MeshViewRender _currentRender;

        private Mesh _target;
        
        private readonly PreviewRenderUtility _previewRender;
        
        private bool _isWireframeShowed = true;

        private static Event CurrentEvent => Event.current;

        public MeshView(Mesh target)
        {
            _target = target;

            _previewRender = new PreviewRenderUtility();
        }

        public void RegisterRender(MeshViewRender render)
        {
            if(_renders.Contains(render))
                return;

            InitializeRender(render);
            _renders.Add(render);

            if (_currentRender == null)
                _currentRender = render;
        }

        public void SetTarget(Mesh target)
        {
            _target = target;
            foreach (var viewRender in _renders)
            {
                InitializeRender(viewRender);
            }
        }

        public void OnGUI(Rect rect)
        {
            var settingsRect = new Rect(rect.x, rect.yMax - MeshViewStyles.SettingsPanelHeight, rect.width, MeshViewStyles.SettingsPanelHeight);
            DrawSettingsPanel(settingsRect);
            
            var meshViewRect = new Rect(rect.x, rect.y, rect.width, rect.height - MeshViewStyles.SettingsPanelHeight);
            DrawMeshView(meshViewRect);
        }

        public void Dispose()
        {
            foreach (var viewRender in _renders)
            {
                viewRender.Dispose();
            }
            
            _previewRender.Cleanup();
        }

        private void InitializeRender(MeshViewRender render)
        {
            render.SetTarget(_target);
            render.SetRenderContext(_previewRender);
            render.SetRenderState(_renderState);
        }

        #region SettingsPanelRendering

        private void DrawSettingsPanel(Rect rect)
        {
            GUI.enabled = ShaderUtil.hardwareSupportsRectRenderTexture && _target != null;
            {
                GUI.Box(rect, string.Empty, EditorStyles.inspectorDefaultMargins);

                GUILayout.BeginArea(rect);
                {
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();

                    DrawDisplayModeDropDown();
                    DrawWireframeToggle();
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
            GUI.enabled = true;
        }

        private void DrawDisplayModeDropDown()
        {
            
        }

        private void DrawWireframeToggle()
        {
            GUI.enabled = _currentRender != null && _currentRender.IsWireframeSupported;
            var wireframeToggleWidth = EditorStyles.toolbarButton.CalcSize(MeshViewStyles.WireframeToggle).x;
            var wireframeToggleRect = EditorGUILayout.GetControlRect(GUILayout.Width(wireframeToggleWidth));

            _isWireframeShowed = GUI.Toggle(wireframeToggleRect,
                _isWireframeShowed, MeshViewStyles.WireframeToggle,
                EditorStyles.toolbarButton);
            GUI.enabled = true;
        }

        #endregion

        #region MeshRendering

        private void DrawMeshView(Rect rect)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (CurrentEvent.type == EventType.Repaint)
                {
                    var halfWidth = rect.width * 0.5f;
                    var halfHeight = rect.height * 0.5f;
                    
                    var x = rect.x + halfWidth - MeshViewStyles.RenderTextureNotSupportedWidth * 0.5f;
                    var y = rect.y + halfHeight - MeshViewStyles.RenderTextureNotSupportedHeight * 0.5f;
                    
                    EditorGUI.DropShadowLabel(new Rect(x, y, MeshViewStyles.RenderTextureNotSupportedWidth, MeshViewStyles.RenderTextureNotSupportedHeight), 
                        MeshViewStyles.RenderTextureNotSupportedMessage);
                }

                return;
            }

            _previewRender.BeginPreview(rect, GUIStyle.none);
            DrawMesh(rect);
            _previewRender.EndAndDrawPreview(rect);
            
            DrawMeshInfo(rect);
        }

        private void DrawMeshInfo(Rect rect)
        {
            var meshInfo = MeshViewUtility.GetMeshInfo(_target);

            EditorGUI.DropShadowLabel(new Rect(rect.x, rect.yMax - (MeshViewStyles.MeshInfoHeight + MeshViewStyles.MeshInfoMargin), 
                rect.width, MeshViewStyles.MeshInfoHeight), meshInfo);
        }

        private void DrawMesh(Rect rect)
        {
            if(_target == null || _currentRender == null)
                return;
            
            _currentRender.InitializeCamera();
            _currentRender.InitializeLights();
            
            _currentRender.HandleUserInput(rect);
            
            _currentRender.Render();

            if (_currentRender.IsWireframeSupported && _isWireframeShowed)
            {
                var wireframeRender = _currentRender.WireframeOverride;
                wireframeRender?.Render();
            }
        }

        #endregion
    }
}