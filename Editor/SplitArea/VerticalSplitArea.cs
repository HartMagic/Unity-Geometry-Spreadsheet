﻿namespace GeometrySpreadsheet.Editor.SplitArea
{
    using UnityEditor;
    using UnityEngine;

    internal class VerticalSplitArea
    {
        private readonly EditorWindow _parent;
        
        private float _splitLinePosition;
        private float _splitLineOffset;

        private bool _isResizing;
        
        public Rect FirstRect => new Rect(0.0f, 0.0f, _parent.position.width, _splitLinePosition);
        public Rect SecondRect => new Rect(0.0f, _splitLinePosition, _parent.position.width, _parent.position.height - _splitLinePosition);

        public VerticalSplitArea(EditorWindow parent)
        {
            _parent = parent;

            _splitLinePosition = parent.position.height * 0.5f;
        }

        public void OnGUI()
        {
            var splitLineRect = new Rect(0.0f, _splitLinePosition, _parent.position.width, 4.0f);
            EditorGUIUtility.AddCursorRect(splitLineRect, MouseCursor.SplitResizeUpDown);
            
            var drawSplitLineRect = new Rect(0.0f, _splitLinePosition, _parent.position.width, 1.0f);
            EditorGUI.DrawRect(drawSplitLineRect, SplitAreaStyles.SplitLineColor);

            if (Event.current.type == EventType.MouseDown && splitLineRect.Contains(Event.current.mousePosition))
            {
                _splitLineOffset = Event.current.mousePosition.y - _splitLinePosition;
                _isResizing = true;
            }

            if (_isResizing)
            {
                _splitLinePosition = Mathf.Clamp(Event.current.mousePosition.y - _splitLineOffset, SplitAreaStyles.Margin.x,
                    _parent.position.height - SplitAreaStyles.Margin.y);
                _parent.Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                _isResizing = false;
            }
        }
    }
}