namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public abstract class MeshViewRender : IDisposable
    {
        private Material _material;
        private MeshViewRender _wireframeOverride;
        
        public abstract string DisplayName { get; }
        public abstract bool IsWireframeSupported { get; }
        
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

        public MeshViewRender WireframeOverride
        {
            get
            {
                if (_wireframeOverride == null)
                {
                    _wireframeOverride = CreateWireframeOverride();
                    _wireframeOverride.SetTarget(Target);
                    _wireframeOverride.SetRenderContext(RenderContext);
                    _wireframeOverride.SetRenderState(RenderState);
                }

                return _wireframeOverride;
            }
        }

        internal abstract void Render();

        internal abstract void InitializeCamera();
        internal abstract void InitializeLights();

        internal abstract void HandleUserInput(Rect rect);

        internal virtual MeshViewRender CreateWireframeOverride()
        {
            return null;
        }

        protected abstract Material CreateMaterial();

        internal void SetRenderState(RenderState renderState)
        {
            RenderState = renderState;
            _wireframeOverride?.SetRenderState(renderState);
        }

        internal void SetTarget(Mesh target)
        {
            Target = target;
            _wireframeOverride?.SetTarget(target);
        }

        internal void SetRenderContext(PreviewRenderUtility renderUtility)
        {
            RenderContext = renderUtility;
            _wireframeOverride?.SetRenderContext(renderUtility);
        }

        public void Dispose()
        {
            _wireframeOverride?.Dispose();
            
            if(_material != null)
                Object.DestroyImmediate(_material);
        }
    }
}