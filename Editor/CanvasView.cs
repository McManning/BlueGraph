
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor.Experimental.GraphView;
using GraphViewPort = UnityEditor.Experimental.GraphView.Port;
using GraphViewEdge = UnityEditor.Experimental.GraphView.Edge;
using GraphViewSearchWindow = UnityEditor.Experimental.GraphView.SearchWindow;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Graph view that contains the nodes, edges, etc. 
    /// </summary>
    public class CanvasView : GraphView
    {
        Label m_Title;
        
        List<CommentView> m_CommentViews = new List<CommentView>();
        
        Graph m_Graph;
        SerializedObject m_SerializedGraph;
        
        SearchWindow m_Search;
        EditorWindow m_EditorWindow;

        EdgeConnectorListener m_EdgeListener;
        
        HashSet<ICanDirty> m_Dirty = new HashSet<ICanDirty>();

        Vector2 m_LastMousePosition;

        public CanvasView(EditorWindow window)
        {
            m_EditorWindow = window;
            
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/BlueGraphVariables"));
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/CanvasView"));
            AddToClassList("canvasView");
            
            m_EdgeListener = new EdgeConnectorListener(this);
            m_Search = ScriptableObject.CreateInstance<SearchWindow>();
            m_Search.AddSearchProvider(new DefaultSearchProvider());
            m_Search.target = this;

            SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
        
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        
            // Add event handlers for shortcuts and changes
            RegisterCallback<KeyDownEvent>(OnGraphKeydown);
            RegisterCallback<MouseMoveEvent>(OnGraphMouseMove);

            graphViewChanged = OnGraphViewChanged;
            
            RegisterCallback<AttachToPanelEvent>(c => { Undo.undoRedoPerformed += OnUndoRedo; });
            RegisterCallback<DetachFromPanelEvent>(c => { Undo.undoRedoPerformed -= OnUndoRedo; });

            nodeCreationRequest = (ctx) => OpenSearch(ctx.screenMousePosition);
        
            // Add handlers for (de)serialization
            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnTryPasteSerializedData;
            unserializeAndPaste = OnUnserializeAndPaste;
        
            RegisterCallback<GeometryChangedEvent>(OnFirstResize);
            
            m_Title = new Label("BLUEGRAPH");
            m_Title.AddToClassList("canvasViewTitle");
            Add(m_Title);
            
            // Add a grid renderer *behind* content containers
            Insert(0, new GridBackground());
        }

        void OnUndoRedo()
        {
            Refresh();
        }
        
        void OnGraphMouseMove(MouseMoveEvent evt)
        {
            m_LastMousePosition = evt.mousePosition;
        }

        /// <summary>
        /// Event handler to frame the graph view on initial layout
        /// </summary>
        void OnFirstResize(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnFirstResize);
            FrameAll();
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (m_SerializedGraph == null)
            {
                return change;
            }

            if (change.movedElements != null)
            {
                // Moved nodes will update their underlying models automatically.
                EditorUtility.SetDirty(m_Graph);
            }
            
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is NodeView node)
                    {
                        RemoveNode(node);
                    }
                    else if (element is GraphViewEdge edge)
                    {
                        RemoveEdge(edge, true);
                    }
                    else if (element is CommentView comment)
                    {
                        RemoveComment(comment);
                    }
                    
                    if (element is ICanDirty canDirty)
                    {
                        m_Dirty.Remove(canDirty);
                    }
                }
            }
            
            return change;
        }
        
        void OnGraphKeydown(KeyDownEvent evt)
        {
            // C: Add a new comment around the selected nodes (or just at mouse position)
            if (evt.keyCode == KeyCode.C && !evt.ctrlKey && !evt.commandKey)
            {
                AddComment();
            }
        }
        
        public void Load(Graph graph)
        {
            m_Graph = graph;
            m_SerializedGraph = new SerializedObject(m_Graph);
            m_Title.text = graph.Title;

            AddNodeViews(graph.nodes);
            AddCommentViews(graph.comments);

            // Reset the lookup to a new set of whitelisted modules
            m_Search.ClearTags();

            var attrs = graph.GetType().GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is IncludeTagsAttribute include)
                {
                    m_Search.includeTags.AddRange(include.tags);
                }
            }
        }

        /// <summary>
        /// Create a new node from reflection data and insert into the Graph.
        /// </summary>
        internal void AddNodeFromSearch(
            AbstractNode node,
            Vector2 screenPosition, 
            PortView connectedPort = null
        ) {
            // Calculate where to place this node on the graph
            var windowRoot = m_EditorWindow.rootVisualElement;
            var windowMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo(
                windowRoot.parent, 
                screenPosition - m_EditorWindow.position.position
            );

            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
        
            // Track undo and add to the graph
            Undo.RegisterCompleteObjectUndo(m_Graph, $"Add Node {node.name}");
            
            node.position = graphMousePosition;

            m_Graph.AddNode(node);
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);

            var serializedNodesArr = m_SerializedGraph.FindProperty("nodes");

            var nodeIdx = m_Graph.nodes.IndexOf(node);
            var serializedNode = serializedNodesArr.GetArrayElementAtIndex(nodeIdx);
            
            // Add a node to the visual graph
            var editorType = NodeReflection.GetNodeEditorType(node.GetType());
            var element = Activator.CreateInstance(editorType) as NodeView;
            element.Initialize(node, serializedNode, m_EdgeListener);
            
            AddElement(element);
            
            // If there was a provided existing port to connect to, find the best 
            // candidate port on the new node and connect. 
            if (connectedPort != null)
            {
                var edge = new GraphViewEdge();

                if (connectedPort.direction == Direction.Input)
                {
                    edge.input = connectedPort;
                    edge.output = element.GetCompatibleOutputPort(connectedPort);
                }
                else
                {
                    edge.output = connectedPort;
                    edge.input = element.GetCompatibleInputPort(connectedPort);
                }
                
                AddEdge(edge, false);
            }
            
            Dirty(element);
        }
        
        /// <summary>
        /// Remove a node from both the canvas view and the graph model
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(NodeView node)
        {
            Undo.RegisterCompleteObjectUndo(m_Graph, $"Delete Node {node.name}");
            
            m_Graph.RemoveNode(node.target);
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);

            RemoveElement(node);
        }

        /// <summary>
        /// Add a new edge to both the canvas view and the underlying graph model
        /// </summary>
        public void AddEdge(GraphViewEdge edge, bool registerAsNewUndo)
        {
            if (edge.input == null || edge.output == null) return;
            
            if (registerAsNewUndo)
            {
                Undo.RegisterCompleteObjectUndo(m_Graph, "Add Edge");
            }
            
            // Handle single connection ports on either end. 
            var edgesToRemove = new List<GraphViewEdge>();
            if (edge.input.capacity == GraphViewPort.Capacity.Single)
            {
                foreach (var conn in edge.input.connections)
                {
                    edgesToRemove.Add(conn);
                }
            }

            if (edge.output.capacity == GraphViewPort.Capacity.Single)
            {
                foreach (var conn in edge.output.connections)
                {
                    edgesToRemove.Add(conn);
                }
            }

            foreach (var edgeToRemove in edgesToRemove)
            {
                RemoveEdge(edgeToRemove, false);
            }
            
            var input = edge.input as PortView;
            var output = edge.output as PortView;
            
            // Connect the ports in the model
            m_Graph.AddEdge(input.target, output.target);
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);

            // Add a matching edge view onto the canvas
            var newEdge = input.ConnectTo(output);
            AddElement(newEdge);
            
            // Dirty the affected node views
            Dirty(input.node as NodeView);
            Dirty(output.node as NodeView);
        }
        
        /// <summary>
        /// Remove an edge from both the canvas view and the underlying graph model
        /// </summary>
        public void RemoveEdge(GraphViewEdge edge, bool registerAsNewUndo)
        {
            var input = edge.input as PortView;
            var output = edge.output as PortView;
            
            if (registerAsNewUndo)
            {
                Undo.RegisterCompleteObjectUndo(m_Graph, "Remove Edge");
            }
            
            // Disconnect the ports in the model
            m_Graph.RemoveEdge(input.target, output.target);
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);
            
            // Remove the edge view
            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);
            edge.input = null;
            edge.output = null;
            RemoveElement(edge);
            
            // Dirty the affected node views
            Dirty(input.node as NodeView);
            Dirty(output.node as NodeView);
        }
        
        /// <summary>
        /// Resync nodes and edges on the canvas with the modified graph.
        /// </summary>
        void Refresh()
        {
            // TODO: Smart diff - if we start seeing performance issues. 
            // It gets complicated due to how we bind serialized objects though.
            
            // For now, we just nuke everything and start over.

            // Clear serialized graph first so that change events aren't undo tracked
            m_SerializedGraph = null;

            DeleteElements(graphElements.ToList());

            Load(m_Graph);
        }
        
        /// <summary>
        /// Mark a node and all dependents as dirty for the next refresh. 
        /// </summary>
        /// <param name="node"></param>
        public void Dirty(ICanDirty element)
        {
            m_Dirty.Add(element);
            
            // TODO: Not the best place for this.
            EditorUtility.SetDirty(m_Graph);

            element.OnDirty();

            // Also dirty outputs if a NodeView
            if (element is NodeView node)
            {
                foreach (var port in node.outputs)
                {
                    foreach (var conn in port.connections)
                    {
                        Dirty(conn.input.node as NodeView);
                    }
                }
            }
        }

        /// <summary>
        /// Dirty all nodes on the canvas for a complete refresh
        /// </summary>
        public void DirtyAll()
        {
            graphElements.ForEach((element) =>
            {
                if (element is ICanDirty cd)
                {
                    cd.OnDirty();
                    m_Dirty.Add(cd);
                }
            });
        }

        public void Update()
        {
            // Propagate change on dirty elements
            foreach (var element in m_Dirty)
            {
                element.OnUpdate();
            }
            
            m_Dirty.Clear();
        }

        public void OpenSearch(Vector2 screenPosition, PortView connectedPort = null)
        {
            m_Search.sourcePort = connectedPort;
            GraphViewSearchWindow.Open(new SearchWindowContext(screenPosition), m_Search);
        }
        
        /// <summary>
        /// Append views for a set of nodes
        /// </summary>
        void AddNodeViews(List<AbstractNode> nodes, bool selectOnceAdded = false, bool centerOnMouse = false)
        {
            var serializedNodesArr = m_SerializedGraph.FindProperty("nodes");
            
            // Add views of each node from the graph
            var nodeMap = new Dictionary<AbstractNode, NodeView>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var graphIdx = m_Graph.nodes.IndexOf(node);

                if (graphIdx < 0)
                {
                    Debug.LogError("Cannot add NodeView: Node is not indexed on the graph");
                }
                else
                {
                    var serializedNode = serializedNodesArr.GetArrayElementAtIndex(graphIdx);

                    var editorType = NodeReflection.GetNodeEditorType(node.GetType());
                    var element = Activator.CreateInstance(editorType) as NodeView;
                
                    element.Initialize(node, serializedNode, m_EdgeListener);
                    AddElement(element);
                
                    nodeMap.Add(node, element);
                    Dirty(element);
                
                    if (selectOnceAdded)
                    {
                        AddToSelection(element);
                    }
                }
            }
            
            if (centerOnMouse)
            {
                var bounds = GetBounds(nodeMap.Values);
                var worldPosition = contentViewContainer.WorldToLocal(m_LastMousePosition);
                var delta = worldPosition - bounds.center;
                
                foreach (var node in nodeMap)
                {
                    node.Value.SetPosition(new Rect(node.Key.position + delta, Vector2.one));
                }
            }

            // Sync edges on the graph with our graph's connections 
            // TODO: Deal with trash connections from bad imports
            // and try to just refactor this whole thing tbh
            foreach (var node in nodeMap)
            {
                foreach (var port in node.Key.Ports)
                {
                    if (port.direction == PortDirection.Output) continue;

                    foreach (var conn in port.Connections)
                    {
                        var connectedNode = conn.node;
                        if (connectedNode == null)
                        {
                            Debug.LogError(
                                 $"Could not connect `{node.Value.title}:{port.name}`: " +
                                 $"Connected node no longer exists."
                            );
                            continue;
                        }

                        // Only add if the linked node is in the collection
                        // TODO: This shouldn't be a problem
                        if (!nodeMap.ContainsKey(connectedNode))
                        {
                            Debug.LogError(
                                 $"Could not connect `{node.Value.title}:{port.name}` -> `{connectedNode.name}:{conn.name}`. " +
                                 $"Target node does not exist in the NodeView map"
                            );
                            continue;
                        }

                        var inPort = node.Value.GetInputPort(port.name);
                        var outPort = nodeMap[connectedNode].GetOutputPort(conn.name);
                        
                        if (inPort == null)
                        {
                            Debug.LogError(
                                $"Could not connect `{node.Value.title}:{port.name}` -> `{connectedNode.name}:{conn.name}`. " +
                                $"Input port `{port.name}` no longer exists."
                            );
                        }
                        else if (outPort == null)
                        {
                            Debug.LogError(
                                $"Could not connect `{connectedNode.name}:{conn.name}` to `{node.Value.name}:{port.name}`. " +
                                $"Output port `{conn.name}` no longer exists."
                            );
                        }
                        else
                        {
                            var edge = inPort.ConnectTo(outPort);
                            AddElement(edge);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Append views for comments from a Graph
        /// </summary>
        void AddCommentViews(IEnumerable<Comment> comments)
        { 
            foreach (var comment in comments)
            {
                var commentView = new CommentView(comment);
                m_CommentViews.Add(commentView);
                AddElement(commentView);
                Dirty(commentView);
            }
        }

        /// <summary>
        /// Calculate the bounding box for a set of elements
        /// </summary>
        Rect GetBounds(IEnumerable<ISelectable> items)
        {
            var contentRect = Rect.zero;
               
            foreach (var item in items)
            {
                if (item is GraphElement ele)
                {
                    var boundingRect = ele.GetPosition();
                    boundingRect.width = Mathf.Max(boundingRect.width, 1);
                    boundingRect.height = Mathf.Max(boundingRect.height, 1);
                    
                    boundingRect = ele.parent.ChangeCoordinatesTo(contentViewContainer, boundingRect);

                    if (contentRect.width < 1 || contentRect.height < 1)
                    {
                        contentRect = boundingRect;
                    }
                    else
                    {
                        contentRect = RectUtils.Encompass(contentRect, boundingRect);
                    }
                }
            }

            return contentRect;
        }

        /// <summary>
        /// Add a new comment to the canvas and the associated Graph
        /// 
        /// If there are selected nodes, this'll encapsulate the selection with
        /// the comment box. Otherwise, it'll add at defaultPosition.
        /// </summary>
        void AddComment()
        {
            Undo.RegisterCompleteObjectUndo(m_Graph, "Add Comment");
            
            // Pad out the bounding box a bit more on the selection
            var padding = 30f; // TODO: Remove hardcoding

            var bounds = GetBounds(selection);
            
            if (bounds.width < 1 || bounds.height < 1)
            {
                Vector2 worldPosition = contentViewContainer.WorldToLocal(m_LastMousePosition);
                bounds.x = worldPosition.x;
                bounds.y = worldPosition.y;

                // TODO: For some reason CSS minWidth/minHeight isn't being respected. 
                // Maybe I need to wait for CSS to load before setting bounds?
                bounds.width = 150 - padding * 2;
                bounds.height = 100 - padding * 3;
            }

            bounds.x -= padding;
            bounds.y -= padding * 2;
            bounds.width += padding * 2;
            bounds.height += padding * 3; 
            
            // Add the model
            var comment = new Comment();
            comment.text = "New Comment";
            comment.region = bounds;

            m_Graph.comments.Add(comment);
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);
            
            // Add the view
            var commentView = new CommentView(comment);
            m_CommentViews.Add(commentView);
            AddElement(commentView);
            
            Dirty(commentView);
        }
        
        /// <summary>
        /// Remove a comment from both the canvas view and the graph model
        /// </summary>
        /// <param name="comment"></param>
        public void RemoveComment(CommentView comment)
        {
            Undo.RegisterCompleteObjectUndo(m_Graph, "Delete Comment");
            
            // Remove the model
            m_Graph.comments.Remove(comment.target);
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);
            
            // Remove the view
            RemoveElement(comment);
            m_CommentViews.Remove(comment);
        }

        /// <summary>
        /// Handler for deserializing a node from a string payload
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="data"></param>
        private void OnUnserializeAndPaste(string operationName, string data)
        {
            Undo.RegisterCompleteObjectUndo(m_Graph, "Paste Subgraph");

            var cpg = CopyPasteGraph.Deserialize(data, m_Search.includeTags);
            
            foreach (var node in cpg.nodes)
            {
                m_Graph.AddNode(node);
            }

            foreach (var comment in cpg.comments)
            {
                m_Graph.comments.Add(comment);
            }
            
            m_SerializedGraph.Update();
            EditorUtility.SetDirty(m_Graph);
            
            // Add views for all the new elements
            ClearSelection();
            AddNodeViews(cpg.nodes, true, true);
            AddCommentViews(cpg.comments);

            ScriptableObject.DestroyImmediate(cpg);
        }

        private bool OnTryPasteSerializedData(string data)
        {
            return CopyPasteGraph.CanDeserialize(data);
        }
        
        /// <summary>
        /// Serialize a selection to support cut/copy/duplicate
        /// </summary>
        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            return CopyPasteGraph.Serialize(elements);
        }
        
        public override List<GraphViewPort> GetCompatiblePorts(GraphViewPort startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<GraphViewPort>();
            var startPortView = startPort as PortView;

            ports.ForEach((port) => {
                var portView = port as PortView;
                if (portView.IsCompatibleWith(startPortView))
                {
                    compatiblePorts.Add(portView);
                }
            });
            
            return compatiblePorts;
        }

        /// <summary>
        /// Replacement of the base AddElement() to undo the hardcoded
        /// border style that's overriding USS files. 
        /// Should probably report this as dumb. 
        /// 
        /// See: https://github.com/Unity-Technologies/UnityCsReference/blob/02d565cf3dd0f6b15069ba976064c75dc2705b08/Modules/GraphViewEditor/Views/GraphView.cs#L1222
        /// </summary>
        /// <param name="graphElement"></param>
        public new void AddElement(GraphElement graphElement)
        {
            var borderBottomWidth = graphElement.style.borderBottomWidth;
            base.AddElement(graphElement);

            if (graphElement.IsResizable())
            {
                graphElement.style.borderBottomWidth = borderBottomWidth;
            }
        }
    }
}
