
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Custom NodeView for the `Say` node.
    /// 
    /// Displays a multiline editor for the text content.
    /// </summary>
    [CustomNodeView(typeof(Say))]
    public class SayNodeView : DialogNodeView
    {
        /// <summary>
        /// Scroll view state for the IMGUI container
        /// </summary>
        private Vector2 scrollState;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Setup a container to render IMGUI content in 
            var container = new IMGUIContainer(OnGUI);
            extensionContainer.Add(container);
            
            // Currently needed by Unity's base Node class to properly
            // resize for extensionContent.
            RefreshExpandedState();
        }
        
        void OnGUI()
        {
            var say = Target as Say;

            // Multiline TextInputs in Unity suck. So we're going IMGUI for this.
            
            scrollState = EditorGUILayout.BeginScrollView(scrollState);

            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };

            say.text = EditorGUILayout.TextArea(
                say.text, 
                style,
                GUILayout.Height(60),
                GUILayout.MaxWidth(250)
            );

            EditorGUILayout.EndScrollView();
        }
    }
}
