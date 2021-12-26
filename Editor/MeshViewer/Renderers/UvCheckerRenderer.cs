namespace GeometrySpreadsheet.Editor.MeshViewer.Renderers
{
    using System.Linq;
    using Abstract;
    using UnityEditor;
    using UnityEngine;

    public sealed class UvCheckerRenderer : PerspectiveMeshViewRenderer
    {
        private const string ShaderName = "Hidden/GeometrySpreadsheet/InternalUvChecker";
        private const string CheckerTextureName = "Previews/Textures/textureChecker.png";
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int UvChannelId = Shader.PropertyToID("_UvChannel");
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        private const int MinTextureMultiplier = 30;
        private const int MaxTextureMultiplier = 1;

        private int _currentUvChannel;
        private int _textureMultiplier = 10;

        public override string DisplayName => "UV Checker";
        
        public UvCheckerRenderer(MeshViewRenderer wireframeOverride) : base(wireframeOverride)
        {
        }

        protected override void RenderInternal(Vector3 position, Quaternion rotation, MaterialPropertyBlock materialPropertyBlock)
        {
            for (var i = 0; i < Target.subMeshCount; i++)
            {
                materialPropertyBlock.SetColor(ColorPropertyId, MeshViewUtility.GetSubMeshColor(i));
                RenderContext.DrawMesh(Target, position, rotation, Material, i, materialPropertyBlock);
            }
                
            RenderContext.Render();
        }
        
        protected override Material CreateMaterial()
        {
            var material = new Material(Shader.Find(ShaderName))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            var checkerTexture = EditorGUIUtility.LoadRequired(CheckerTextureName) as Texture2D;
            material.SetTexture(MainTexId, checkerTexture);
            material.SetFloat(UvChannelId, _currentUvChannel);
            material.mainTextureScale = new Vector2(_textureMultiplier, _textureMultiplier);
            
            return material;
        }

        public override SettingsPanelDelegate GetSettingsPanelCallback()
        {
            return DrawUvChannelSettings;
        }

        private void DrawUvChannelSettings(Rect rect)
        {
            DrawTextureSizeSlider();
            DrawUvChannelDropDown();
        }

        private void DrawTextureSizeSlider()
        {
            var sliderRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(MeshViewStyles.UvTextureMultiplierSliderWidth), GUILayout.ExpandWidth(true));
            
            EditorGUI.BeginChangeCheck();
            _textureMultiplier = (int) GUI.HorizontalSlider(sliderRect, _textureMultiplier, MinTextureMultiplier,
                MaxTextureMultiplier, MeshViewStyles.PreSliderStyle, MeshViewStyles.PreSliderThumbStyle);
            if (EditorGUI.EndChangeCheck())
            {
                Material.mainTextureScale = new Vector2(_textureMultiplier, _textureMultiplier);
            }
        }

        private void DrawUvChannelDropDown()
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