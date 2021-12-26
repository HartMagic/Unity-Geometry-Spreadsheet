namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Renderers;
    using Renderers.Abstract;
    using UnityEditor;
    using UnityEngine;

    internal sealed class MeshView : IDisposable
    {
        private readonly List<MeshViewRenderer> _renders = new List<MeshViewRenderer>();

        private readonly RenderState _renderState = new RenderState();
        
        private MeshViewRenderer _currentRenderer;

        private Mesh _target;
        
        private readonly PreviewRenderUtility _previewRender;
        
        private bool _isWireframeShowed = true;

        private static Event CurrentEvent => Event.current;

        public MeshView(Mesh target)
        {
            _target = target;

            _previewRender = new PreviewRenderUtility();
        }

        public void RegisterRender(MeshViewRenderer renderer)
        {
            if(_renders.Contains(renderer))
                return;

            InitializeRender(renderer);
            _renders.Add(renderer);

            _currentRenderer ??= renderer;
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

        private void InitializeRender(MeshViewRenderer renderer)
        {
            renderer.SetTarget(_target);
            renderer.SetRenderContext(_previewRender);
            renderer.SetRenderState(_renderState);
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

                    _currentRenderer?.GetSettingsPanelCallback()?.Invoke(rect);

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
            var displayModes = _renders.Select(x => x.DisplayName).ToArray();
            var maxDisplayMode = MeshViewUtility.GetMaxString(displayModes);

            var currentDisplayModeIndex = _renders.IndexOf(_currentRenderer);
            var availableDisplayModes = _renders.Select(x => x.IsAvailable).ToArray();

            var displayModeDropDownWidth = EditorStyles.toolbarDropDown.CalcSize(new GUIContent(maxDisplayMode)).x;
            var displayModeDropDownRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(displayModeDropDownWidth), GUILayout.ExpandWidth(true));

            var displayModeDropDownContent = MeshViewStyles.GetDisplayModeContent(_currentRenderer);
            if (EditorGUI.DropdownButton(displayModeDropDownRect, displayModeDropDownContent, FocusType.Passive,
                EditorStyles.toolbarDropDown))
            {
                MeshViewUtility.DrawPopup(displayModeDropDownRect, displayModes, currentDisplayModeIndex, SetDisplayMode, availableDisplayModes);
            }
        }

        private void SetDisplayMode(object index)
        {
            var popupIndex = (int) index;
            if(popupIndex < 0 || popupIndex >= _renders.Count)
                return;

            _currentRenderer = _renders[popupIndex];
        }

        private void DrawWireframeToggle()
        {
            GUI.enabled = _currentRenderer != null && _currentRenderer.IsWireframeSupported;
            var wireframeToggleWidth = EditorStyles.toolbarButton.CalcSize(MeshViewStyles.WireframeToggle).x;
            var wireframeToggleRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(wireframeToggleWidth), GUILayout.ExpandWidth(true));

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
            if(_target == null || _currentRenderer == null)
                return;
            
            _currentRenderer.InitializeCamera();
            _currentRenderer.InitializeLights();
            
            _currentRenderer.HandleUserInput(rect);
            
            _currentRenderer.Draw();

            if (_currentRenderer.IsWireframeSupported && _isWireframeShowed)
            {
                var wireframeRender = _currentRenderer.WireframeOverride;
                wireframeRender?.Draw();
            }
        }

        #endregion
    }
}