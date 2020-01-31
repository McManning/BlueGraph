using System;
using UnityEngine;

namespace BlueGraph
{
    /// <summary>
    /// Comments placed within the CanvasView to document and group placed nodes
    /// </summary>
    [Serializable]
    public class Comment
    {
        public string title;
        public string theme;
        public Rect graphRect;
    }
}
