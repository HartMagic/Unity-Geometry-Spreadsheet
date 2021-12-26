namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers
{
    using System.Linq;
    using Abstract;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public sealed class UvLayoutRenderer : OrthographicMeshViewRenderer
    {
        private const string LinesShaderName = "Hidden/GeometrySpreadsheet/InternalDefault";
        private const string ShaderName = "Hidden/GeometrySpreadsheet/InternalUvLayout";
        
        private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
        private static readonly int CullId = Shader.PropertyToID("_Cull");
        private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");
        
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int UvChannelId = Shader.PropertyToID("_UvChannel");

        private static readonly Color MajorLineColor = new Color(0.6f, 0.6f, 0.7f, 1.0f);
        private static readonly Color MinorLineColor = new Color(0.6f, 0.6f, 0.7f, 0.5f);

        private const float LineStep = 0.125f;

        private Material _linesMaterial;
        
        private int _currentUvChannel;

        public override string DisplayName => "UV Layout";
        
        public UvLayoutRenderer(MeshViewRenderer wireframeOverride) : base(wireframeOverride)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if(_linesMaterial != null)
                Object.DestroyImmediate(_linesMaterial);
        }
        
        public override SettingsPanelDelegate GetSettingsPanelCallback()
        {
            return DrawUvChannelDropDown;
        }

        protected override void Render()
        {
            if(_linesMaterial == null)
                return;
            
            GL.PushMatrix();
            {
                _linesMaterial.SetPass(0);
                
                GL.LoadProjectionMatrix(Camera.projectionMatrix);
                GL.MultMatrix(Camera.worldToCameraMatrix);

                GL.Begin(GL.LINES);
                {
                    for (var g = -2.0f; g <= 3.0f; g += LineStep)
                    {
                        var majorLine = Mathf.Abs(g - Mathf.Round(g)) < 0.01f;
                        if (majorLine)
                        {
                            GL.Color(MajorLineColor);
                            
                            GL.Vertex3(-2, g, 0);
                            GL.Vertex3(+3, g, 0);
                            GL.Vertex3(g, -2, 0);
                            GL.Vertex3(g, +3, 0);
                        }
                        else if (g >= 0 && g <= 1)
                        {
                            GL.Color(MinorLineColor);
                            
                            GL.Vertex3(0, g, 0);
                            GL.Vertex3(1, g, 0);
                            GL.Vertex3(g, 0, 0);
                            GL.Vertex3(g, 1, 0);
                        }
                    }
                }
                GL.End();

                GL.LoadIdentity();
                
                Material.SetPass(0);
                GL.wireframe = true;
                Graphics.DrawMeshNow(Target, Camera.worldToCameraMatrix);
                GL.wireframe = false;
            }
            GL.PopMatrix();
        }

        protected override Material CreateMaterial()
        {
            _linesMaterial = CreateLinesMaterial();
            return CreateObjectMaterial();
        }

        private Material CreateLinesMaterial()
        {
            var shader = Shader.Find(LinesShaderName);
            if (shader == null)
            {
                Debug.LogWarning("UvLayout lines shader has not FOUND");
                return null;
            }

            var material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            material.SetFloat(SrcBlendId, (float)BlendMode.SrcAlpha);
            material.SetFloat(DstBlendId, (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat(CullId, (float)CullMode.Off);
            material.SetFloat(ZWriteId, 0.0f);

            return material;
        }

        private Material CreateObjectMaterial()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning("UvLayout shader has not FOUND");
                return null;
            }

            var material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            material.SetColor(ColorId, new Color(1.0f, 0.9f, 0.6f, 1.0f));
            material.SetFloat(UvChannelId, _currentUvChannel);

            return material;
        }
        
        private void DrawUvChannelDropDown(Rect rect)
        {
            var availableChannels = MeshViewUtility.GetAvailableUvChannels(Target).ToArray();
            var channelNames = availableChannels.Select(x => x.channel).ToArray();
            var channelAvailableStatus = availableChannels.Select(x => x.isAvailable).ToArray();
            var maxChannel = MeshViewUtility.GetMaxString(channelNames);

            if (_currentUvChannel < 0 || _currentUvChannel > availableChannels.Length ||
                !availableChannels[_currentUvChannel].isAvailable)
                _currentUvChannel = 0;

            var channelDropDownWidth = EditorStyles.toolbarDropDown.CalcSize(new GUIContent(maxChannel)).x;
            var channelDropDownRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(channelDropDownWidth), GUILayout.ExpandWidth(true));
            
            var channelDropDownContent = new GUIContent(availableChannels[_currentUvChannel].channel, MeshViewStyles.UvChannelDropDown.tooltip);

            if (EditorGUI.DropdownButton(channelDropDownRect, channelDropDownContent, FocusType.Passive,
                EditorStyles.toolbarDropDown))
            {
                MeshViewUtility.DrawPopup(channelDropDownRect, channelNames, _currentUvChannel, SetUvChannel, channelAvailableStatus);
            }
        }

        private void SetUvChannel(object index)
        {
            var availableChannels = MeshViewUtility.GetAvailableUvChannels(Target).ToArray();
            var channelIndex = (int) index;
            if (channelIndex < 0 || channelIndex >= availableChannels.Length)
                return;

            _currentUvChannel = channelIndex;
            Material.SetFloat(UvChannelId, _currentUvChannel);
        }
    }
}