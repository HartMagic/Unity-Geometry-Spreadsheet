namespace GeometrySpreadsheet.Editor.MeshViewer
{
    using System.Collections;
    using System.Collections.Generic;

    internal enum DisplayMode
    {
        Shaded,
        UvChecker,
        UvLayout,
        VertexColor,
        Normals,
        Tangents,
        Blendshapes
    }

    internal class DisplayModeData
    {
        public DisplayMode Mode;
        public bool IsAvailable;
        public string Name;
    }

    internal sealed class DisplayModes : IEnumerable<DisplayModeData>
    {
        private readonly List<DisplayModeData> _modes = new List<DisplayModeData>();

        public DisplayModes()
        {
            _modes.AddRange(new []
            {
                new DisplayModeData
                {
                    Mode = DisplayMode.Shaded,
                    IsAvailable = true,
                    Name = "Shaded"
                },
                new DisplayModeData
                {
                    Mode = DisplayMode.UvChecker,
                    IsAvailable = true,
                    Name = "UV Checker"
                },
                new DisplayModeData
                {
                    Mode = DisplayMode.UvLayout,
                    IsAvailable = true,
                    Name = "UV Layout"
                },
                new DisplayModeData
                {
                    Mode = DisplayMode.VertexColor,
                    IsAvailable = true,
                    Name = "Vertex Color"
                },
                new DisplayModeData
                {
                    Mode = DisplayMode.Normals,
                    IsAvailable = true,
                    Name = "Normals"
                },
                new DisplayModeData
                {
                    Mode = DisplayMode.Tangents,
                    IsAvailable = true,
                    Name = "Tangents"
                },
                new DisplayModeData
                {
                    Mode = DisplayMode.Blendshapes,
                    IsAvailable = false,
                    Name = "Blendshapes"
                }, 
            });
        }

        public IEnumerator<DisplayModeData> GetEnumerator()
        {
            return _modes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}