using System;
using UnityEngine;

namespace BlueGraph
{
    public enum CommentTheme
    {
        Yellow = 0,
        Grey = 1,
        Red = 2,
        Green = 3,
        Blue = 4
    }
       
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

        /// <summary>
        /// Theme used to display the comment in CanvasView
        /// </summary>
        public CommentTheme theme;

        /// <summary>
        /// Region covered by this comment in the CanvasView
        /// </summary>
        public Rect region;
    }
}
