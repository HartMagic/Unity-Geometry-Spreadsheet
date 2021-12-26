namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public abstract class MeshViewRender : IDisposable
    {
        private Material _material;

        public abstract string DisplayName { get; }
        
        public bool IsWireframeSupported => WireframeOverride != null;
        
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

        public MeshViewRender WireframeOverride { get; }

        protected MeshViewRender(MeshViewRender wireframeOverride)
        {
            WireframeOverride = wireframeOverride;
        }

        internal abstract void Render();

        internal abstract void InitializeCamera();
        internal abstract void InitializeLights();

        internal abstract void HandleUserInput(Rect rect);

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