
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace BlueGraph.Editor
{
    using Port = UnityEditor.Experimental.GraphView.Port;

    /// <summary>
    /// Graph view that contains the nodes, edges, etc. 
    /// </summary>
    public class CanvasView : GraphView
    {
        /// <summary>
        /// Title displayed in the bottom left of the canvas
        /// </summary>
        public string Title
        {
            get
            {
                return m_Title.text;
            }
            set
            {
                m_Title.text = value;
            }
        }
        
        Label m_Title;
        
        List<CommentView> m_CommentViews = new List<CommentView>();
        
        GraphEditor m_GraphEditor;
        Graph m_Graph;
        SerializedObject m_SerializedGraph;
        
        SearchProvider m_SearchProvider;
        EditorWindow m_EditorWindow;

        EdgeConnectorListener m_EdgeListener;
        
        HashSet<ICanDirty> m_Dirty = new HashSet<ICanDirty>();

        Vector2 m_LastMousePosition;

        public CanvasView(EditorWindow window)
        {
            m_EditorWindow = window;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/CanvasView"));
            AddToClassList("canvasView");
            
            m_EdgeListener = new EdgeConnectorListener(this);
            m_SearchProvider = ScriptableObject.CreateInstance<SearchProvider>();
            m_SearchProvider.target = this;

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
            Debug.Log("Undo/Redo");
            Refresh();
        }
        
        void OnGraphMouseMove(MouseMoveEvent evt)
        {
            m_LastMousePosition = evt.mousePosition;
        }

        /// <summary>
        /// Event handler to frame the graph view on initial layout
        /// </summary>
        private void OnFirstResize(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnFirstResize);
            FrameAll();
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    // TODO: Move/optimize
                    if (element is NodeView node)
                    {
                        UpdateCommentLink(node);
                    }
                }
                
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
                    else if (element is Edge edge)
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
            // Reset();

            m_Graph = graph;
            m_SerializedGraph = new SerializedObject(m_Graph);
            
            AddNodeViews(graph.nodes);
            AddCommentViews(graph.comments);

            // Reset the lookup to a new set of whitelisted modules
            m_SearchProvider.modules.Clear();

            var attrs = graph.GetType().GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is IncludeModulesAttribute modulesAttr)
                {
                    foreach (var module in modulesAttr.modules)
                    {
                        m_SearchProvider.modules.Add(module.Split('/'));
                    }
                }
            }
        }

        /// <summary>
        /// Create a new node from reflection data and insert into the Graph.
        /// </summary>
        internal void AddNodeFromReflectionData(
            NodeReflectionData data,
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
        
            // Create a new node instance and set initial data (ports, etc) 
            Undo.RegisterCompleteObjectUndo(m_Graph, $"Add Node {data.name}");
            
            Debug.Log($"+node {data.name}");

            var node = data.CreateInstance();
            node.graphPosition = graphMousePosition;

            m_Graph.AddNode(node);
            m_SerializedGraph.Update();

            var serializedNodesArr = m_SerializedGraph.FindProperty("nodes");
            var serializedNode = serializedNodesArr.GetArrayElementAtIndex(serializedNodesArr.arraySize - 1);
            
            // Add a node to the visual graph
            var editorType = NodeReflection.GetNodeEditorType(data.type);
            var element = Activator.CreateInstance(editorType) as NodeView;
            element.Initialize(node, serializedNode, m_EdgeListener);
            
            AddElement(element);
            
            EditorUtility.SetDirty(m_Graph);
            
            // If there was a provided existing port to connect to, find the best 
            // candidate port on the new node and connect. 
            if (connectedPort != null)
            {
                var edge = new Edge();

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
            
            Debug.Log($"-node {node.name}");

            if (node.comment != null)
            {
                node.comment.RemoveElement(node);
            }

            m_Graph.RemoveNode(node.target);

            EditorUtility.SetDirty(m_Graph);
        }

        /// <summary>
        /// Add a new edge to both the canvas view and the underlying graph model
        /// </summary>
        public void AddEdge(Edge edge, bool registerAsNewUndo)
        {
            if (edge.input == null || edge.output == null) return;
            
            if (registerAsNewUndo)
            {
                Undo.RegisterCompleteObjectUndo(m_Graph, "Add Edge");
            }
            
            // Handle single connection ports on either end. 
            var edgesToRemove = new List<Edge>();
            if (edge.input.capacity == Port.Capacity.Single)
            {
                foreach (var conn in edge.input.connections)
                {
                    edgesToRemove.Add(conn);
                }
            }

            if (edge.output.capacity == Port.Capacity.Single)
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
            
            Debug.Log($"+edge {input.portName} to {output.portName}");

            // Connect the ports in the model
            m_Graph.AddEdge(input.target, output.target);

            // Add a matching edge view onto the canvas
            var newEdge = input.ConnectTo(output);
            AddElement(newEdge);

            // Sync serialized with the updated models
            m_SerializedGraph.Update();

            EditorUtility.SetDirty(m_Graph);
            
            // Dirty the affected node views
            Dirty(input.node as NodeView);
            Dirty(output.node as NodeView);
        }
        
        /// <summary>
        /// Remove an edge from both the canvas view and the underlying graph model
        /// </summary>
        public void RemoveEdge(Edge edge, bool registerAsNewUndo)
        {
            var input = edge.input as PortView;
            var output = edge.output as PortView;
            
            if (registerAsNewUndo)
            {
                Undo.RegisterCompleteObjectUndo(m_Graph, "Remove Edge");
            }
            
            Debug.Log($"-edge {input.portName} to {output.portName}");

            // Disconnect the ports in the model
            m_Graph.RemoveEdge(input.target, output.target);
            
            // Remove the edge view
            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);
            edge.input = null;
            edge.output = null;
            RemoveElement(edge);
            
            // Sync serialized with the updated models
            m_SerializedGraph.Update();

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

            // Also dirty outputs if a NodeView
            if (element is NodeView)
            {
                var node = element as NodeView;
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

        public void UpdateCommentLink(NodeView node)
        {
            if (node.comment != null)
            {
                // Keep existing connection
                if (node.comment.OverlapsElement(node))
                {
                    Debug.Log("keep conn");
                    return;
                }

                Debug.Log("Remove old");
                node.comment.RemoveElement(node);
            }

            // Find new comment associations
            foreach (var comment in m_CommentViews)
            {
                Debug.Log("Try overlap");
                if (comment.OverlapsElement(node))
                {
                    Debug.Log("Found");
                    comment.AddElement(node);
                    return;
                }
            }
        }

        public void OpenSearch(Vector2 screenPosition, PortView connectedPort = null)
        {
            m_SearchProvider.sourcePort = connectedPort;
            SearchWindow.Open(new SearchWindowContext(screenPosition), m_SearchProvider);
        }
        
        /// <summary>
        /// Append views for nodes from a Graph
        /// </summary>
        void AddNodeViews(List<AbstractNode> nodes, bool selectOnceAdded = false, bool centerOnMouse = false)
        {
            var serializedNodesArr = m_SerializedGraph.FindProperty("nodes");
            
            // Add views of each node from the graph
            Dictionary<AbstractNode, NodeView> nodeMap = new Dictionary<AbstractNode, NodeView>();
            // TODO: Could just be a list with index checking. 

            for (int i = 0; i < nodes.Count; i++)
            {
                // TODO: This assumes it exists on the graph already and serialized.
                // Not correct for pasting. 
                var node = nodes[i];
                var serializedNode = serializedNodesArr.GetArrayElementAtIndex(i);

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
            
            if (centerOnMouse)
            {
                Rect bounds = GetBounds(nodeMap.Values);
                Vector2 worldPosition = contentViewContainer.WorldToLocal(m_LastMousePosition);
                Vector2 delta = worldPosition - bounds.center;
                
                foreach (var node in nodeMap)
                {
                    node.Value.SetPosition(new Rect(node.Key.graphPosition + delta, Vector2.one));
                }
            }

            // Sync edges on the graph with our graph's connections 
            // TODO: Deal with trash connections from bad imports
            // and try to just refactor this whole thing tbh
            foreach (var node in nodeMap)
            {
                foreach (var port in node.Key.ports)
                {
                    if (!port.isInput)
                    {
                        continue;
                    }

                    var connections = port.Connections;
                    foreach (var conn in connections)
                    {
                        if (conn.node == null)
                        {
                            Debug.LogError(
                                 $"Could not connect `{node.Value.title}:{port.name}`: " +
                                 $"Connected node no longer exists."
                            );
                            continue;
                        }

                        // Only add if the linked node is in the collection
                        if (nodeMap.ContainsKey(conn.node))
                        {
                            var inPort = node.Value.GetInputPort(port.name);
                            var outPort = nodeMap[conn.node].GetOutputPort(conn.name);
                        
                            if (inPort == null)
                            {
                                Debug.LogError(
                                    $"Could not connect `{node.Value.title}:{port.name}` -> `{conn.node.name}:{conn.name}`. " +
                                    $"Input port `{port.name}` no longer exists."
                                );
                            }
                            else if (outPort == null)
                            {
                                Debug.LogError(
                                    $"Could not connect `{conn.node.name}:{conn.name}` to `{node.Value.name}:{port.name}`. " +
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
        }
        
        /// <summary>
        /// Append views for comments from a Graph
        /// </summary>
        private void AddCommentViews(List<Comment> comments)
        { 
            foreach (var comment in comments)
            {
                var commentView = new CommentView(comment);
                AddElement(commentView);
                Dirty(commentView);
            }
        }

        /// <summary>
        /// Calculate the bounding box for a set of elements
        /// </summary>
        Rect GetBounds(IEnumerable<ISelectable> items)
        {
            Rect contentRect = Rect.zero;
               
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
        private void AddComment()
        {
            Undo.RegisterCompleteObjectUndo(m_Graph, "Add Comment");
            
            Debug.Log("+comment");

            // Pad out the bounding box a bit more on the selection
            float padding = 30; // TODO: Remove hardcoding

            Rect bounds = GetBounds(selection);
            
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

            var comment = new Comment();
            comment.title = "New Comment";
            comment.graphRect = bounds;

            var commentView = new CommentView(comment);
            commentView.onResize += Dirty;
            
            m_Graph.comments.Add(comment);
            m_CommentViews.Add(commentView);

            AddElement(commentView);
            Dirty(commentView);
            
            EditorUtility.SetDirty(m_Graph);
        }
        
        /// <summary>
        /// Remove a comment from both the canvas view and the graph model
        /// </summary>
        /// <param name="comment"></param>
        public void RemoveComment(CommentView comment)
        {
            Undo.RegisterCompleteObjectUndo(m_Graph, "Delete Comment");
            
            Debug.Log($"-comment {comment.target.title}");

            m_CommentViews.Remove(comment);
            m_Graph.comments.Remove(comment.target);

            EditorUtility.SetDirty(m_Graph);
        }

        /// <summary>
        /// Handler for deserializing a node from a string payload
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="data"></param>
        private void OnUnserializeAndPaste(string operationName, string data)
        {
            throw new Exception("TODO: Reimplement");
            /*
            var graph = CopyPasteGraph.Deserialize(data);
            
            // Add each node to the working graph
            foreach (var node in graph.nodes)
            {
                m_Graph.AddNode(node);
                AssetDatabase.AddObjectToAsset(node, m_Graph);
            }
            
            AssetDatabase.SaveAssets();

            // Add the new nodes and select them
            ClearSelection();
            AddNodeViews(graph.nodes, true, true);*/
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
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
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
