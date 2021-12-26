namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers.Abstract
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public delegate void SettingsPanelCallback(Rect rect);
    
    public abstract class MeshViewRenderer : IDisposable
    {
        private Material _material;

        public abstract string DisplayName { get; }
        
        public bool IsWireframeSupported => WireframeOverride != null;
        
        public virtual bool IsAvailable { get; } = true;
        
        protected Mesh Target { get; private set; }
        
        protected PreviewRenderUtility RenderContext { get; private set; }

        protected Camera Camera => RenderContext?.camera;
        
        protected RenderState RenderState { get; private set; }

        protected Material Material
        {
            get
            {
                if (_material == null)
                    _material = CreateMaterial();

                return _material;
            }
        }
        
        protected static Event CurrentEvent => Event.current;

        public MeshViewRenderer WireframeOverride { get; }

        protected MeshViewRenderer(MeshViewRenderer wireframeOverride)
        {
            WireframeOverride = wireframeOverride;
        }

        internal void Draw()
        {
            if(Material == null)
                return;

            Render();
        }

        public abstract void InitializeCamera();
        public abstract void InitializeLights();

        public abstract void HandleUserInput(Rect rect);

        public virtual SettingsPanelCallback GetSettingsPanelCallback()
        {
            return null;
        }

        protected abstract void Render();
        protected abstract Material CreateMaterial();

        internal void SetRenderState(RenderState renderState)
        {
            RenderState = renderState;
            WireframeOverride?.SetRenderState(renderState);
        }

        internal void SetTarget(Mesh target)
        {
            Target = target;
            WireframeOverride?.SetTarget(target);
        }

        internal void SetRenderContext(PreviewRenderUtility renderUtility)
        {
            RenderContext = renderUtility;
            WireframeOverride?.SetRenderContext(renderUtility);
        }

        public void Dispose()
        {
            WireframeOverride?.Dispose();
            
            if(_material != null)
                Object.DestroyImmediate(_material);
        }
    }
}