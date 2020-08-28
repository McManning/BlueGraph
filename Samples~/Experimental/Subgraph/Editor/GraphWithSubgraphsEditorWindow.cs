
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using BlueGraph;
using BlueGraph.Editor;
using System;

namespace BlueGraphSamples
{
    /// <summary>
    /// Custom editor window for handling graphs that contain subgraphs.
    /// 
    /// May eventually be merged into the main framework, since tabbing
    /// between multiple graphs in the same editor window is a potential
    /// reusable use case. 
    /// </summary>
    public class GraphWithSubgraphsEditorWindow : GraphEditorWindow
    {
        private readonly List<CanvasView> canvasStack = new List<CanvasView>();
        private CanvasView activeCanvas;

        /// <summary>
        /// Load a graph asset in this window for editing
        /// </summary>
        public override void Load(Graph graph)
        {
            base.Load(graph);
            canvasStack.Add(Canvas);
            activeCanvas = Canvas;

            // Add additional content to the titlebar.
            
            var toolbar = new IMGUIContainer(OnToolbarGUI);
            rootVisualElement.Insert(0, toolbar);
        }

        void OnToolbarGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Fizz", EditorStyles.toolbarButton))
            {
                ShowRootGraph();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (canvasStack.Count > 1) 
            {
                DrawSubgraphStackToolbar();
            }
        }

        /// <summary>
        /// Draw a list of navigation buttons, one per subgraph that's open.
        /// </summary>
        void DrawSubgraphStackToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Root", EditorStyles.toolbarButton))
            {
                ShowRootGraph();
            }
            
            // Icon from https://unitylist.com/p/5c3/Unity-editor-icons
            var icon = EditorGUIUtility.FindTexture("tab_next");

            // Render buttons for the rest of the stack
            for (int i = 1; i < canvasStack.Count; i++)
            {
                var canvas = canvasStack[i];
                var content = new GUIContent(canvas.name, icon);

                if (canvas == activeCanvas)
                {
                    // Display the name of the active subgraph
                    GUILayout.Label(content, EditorStyles.toolbarButton);
                } 
                else // Render as a button to go back to that subgraph
                {
                    if (GUILayout.Button(content, EditorStyles.toolbarButton))
                    {
                        FocusCanvas(i);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Replace the view with a subgraph
        /// </summary>
        public void ShowSubgraph(Graph graph)
        {
            // TODO: Handle graphs already in the stack.
            // (If somehow applicable)

            var canvas = new CanvasView(this);
            canvas.Load(graph);
            canvas.name = graph.name;
            
            canvasStack.Add(canvas);
            FocusCanvas(canvasStack.Count - 1);
        }
        
        /// <summary>
        /// Focus one of the canvas views, removing anything else deeper in the stack
        /// </summary>
        void FocusCanvas(int index)
        {
            // Destroy any nesting 
            DestroyCanvasesAfterIndex(index);

            // Swap the active
            rootVisualElement.Remove(activeCanvas);
            activeCanvas = canvasStack[index];
            rootVisualElement.Add(activeCanvas);
        }

        /// <summary>
        /// Destroy all canvases in the stack after the given index
        /// </summary>
        /// <param name="index"></param>
        void DestroyCanvasesAfterIndex(int index)
        {
            if (index >= canvasStack.Count) return;

            // TODO: Actually destroy the CanvasView?
            // Or will automatic garbage collection be sufficient?
            canvasStack.RemoveRange(index + 1, canvasStack.Count - (index + 1));
        }

        /// <summary>
        /// Switch back to displaying the main graph, closing all subgraph canvases
        /// </summary>
        public void ShowRootGraph()
        {
            FocusCanvas(0);
        }

        protected override void Update()
        {
            activeCanvas.Update();
        }

        /// <summary>
        /// Restore an already opened graph after a reload of assemblies
        /// </summary>
        protected override void OnEnable()
        {
            canvasStack.Clear();
            base.OnEnable();
        }
    }
}
