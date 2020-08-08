using System;
using UnityEngine;

namespace BlueGraph
{
    /// <summary>
    /// Comments placed within the CanvasView to document and group placed nodes.
    /// 
    /// These are typically ignored during runtime and only used for documentation.
    /// </summary>
    [Serializable]
    public class Comment
    {
        /// <summary>
        /// Comment content
        /// </summary>
        public string text;
        public string theme;
        public Rect graphRect;
    }
}
