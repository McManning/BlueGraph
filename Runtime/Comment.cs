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
        [SerializeField] string m_Text;

        /// <summary>
        /// Comment content
        /// </summary>
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }
        
        [SerializeField] CommentTheme m_Theme;

        /// <summary>
        /// Theme used to display the comment in CanvasView
        /// </summary>
        public CommentTheme Theme
        {
            get { return m_Theme; }
            set { m_Theme = value; }
        }

        [SerializeField] Rect m_Region;

        /// <summary>
        /// Region covered by this comment in the CanvasView
        /// </summary>
        public Rect Region
        {
            get { return m_Region; }
            set { m_Region = value; }
        }
    }
}
