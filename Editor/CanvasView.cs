
using BlueGraph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlueGraphEditor
{
    /// <summary>
    /// Graph view that contains the nodes, edges, etc. 
    /// </summary>
    public class CanvasView : GraphView
    {
        public Label title;
        
        List<CommentView> m_CommentViews = new List<CommentView>();
        
        GraphEditor m_GraphEditor;
        Graph m_Graph;
        
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
            
            nodeCreationRequest = (ctx) => OpenSearch(ctx.screenMousePosition);
        
            // Add handlers for (de)serialization
            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnTryPasteSerializedData;
            unserializeAndPaste = OnUnserializeAndPaste;
        
            RegisterCallback<GeometryChangedEvent>(OnFirstResize);

            title = new Label();
            title.text = "BLUEGRAPH";
            title.AddToClassList("canvasViewTitle");

            Add(title);
            
            // Add a grid renderer *behind* content containers
            Insert(0, new GridBackground());
        }

        private void OnGraphMouseMove(MouseMoveEvent evt)
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

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
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
            }
            
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is NodeView node)
                    {
                        DestroyNode(node);
                    }
                    else if (element is Edge edge)
                    {
                        DestroyEdge(edge);
                    }
                    else if (element is CommentView comment)
                    {
                        DestroyComment(comment);
                    }
                    
                    if (element is ICanDirty canDirty)
                    {
                        m_Dirty.Remove(canDirty);
                    }
                }
                
                // Save the batch of changes all at once
                AssetDatabase.SaveAssets();
            }
            
            return change;
        }
        
        private void OnGraphKeydown(KeyDownEvent evt)
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
            
            AddNodes(graph.nodes);
            AddComments(graph.comments);

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
        public void CreateNode(NodeReflectionData data, Vector2 screenPosition, PortView connectedPort = null)
        {
            var windowRoot = m_EditorWindow.rootVisualElement;
            var windowMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo(
                windowRoot.parent, 
                screenPosition - m_EditorWindow.position.position
            );

            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
        
            var node = data.CreateInstance();
            node.graphPosition = graphMousePosition;

            m_Graph.AddNode(node);
            
            // Add a node to the visual graph
            var editorType = NodeReflection.GetNodeEditorType(data.type);
            var element = Activator.CreateInstance(editorType) as NodeView;
            element.Initialize(node, m_EdgeListener);

            AddElement(element);
            
            AssetDatabase.AddObjectToAsset(node, m_Graph);
            AssetDatabase.SaveAssets();
            
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
                
                ConnectNodes(edge);
            }
            
            Dirty(element);
        }
        
        /// <summary>
        /// Remove a node from both the graph and the linked asset
        /// </summary>
        /// <param name="node"></param>
        public void DestroyNode(NodeView node)
        {
            if (node.comment != null)
            {
                node.comment.RemoveElement(node);
            }

            m_Graph.RemoveNode(node.target);
            ScriptableObject.DestroyImmediate(node.target, true);
        }

        public void DestroyComment(CommentView comment)
        {
            m_CommentViews.Remove(comment);
            m_Graph.comments.Remove(comment.target);
        }

        public void ConnectNodes(Edge edge)
        {
            if (edge.input == null || edge.output == null) return;
            
            // Handle single connection ports on either end. 
            var edgesToRemove = new List<GraphElement>();
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

            if (edgesToRemove.Count > 0)
            {
                DeleteElements(edgesToRemove);
            }

            var newEdge = edge.input.ConnectTo(edge.output);
            AddElement(newEdge);
            
            Dirty(edge.input.node as NodeView);
            Dirty(edge.output.node as NodeView);
        }

        /// <summary>
        /// Mark a node and all dependents as dirty for the next refresh. 
        /// </summary>
        /// <param name="node"></param>
        public void Dirty(ICanDirty element)
        {
            m_Dirty.Add(element);

            // Also dirty outputs if a nodeview
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
            nodes.ForEach((node) =>
            {
                if (node is ICanDirty dnode)
                    m_Dirty.Add(dnode);
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

        public void DestroyEdge(Edge edge)
        {
            var input = edge.input.node as NodeView;
            var output = edge.output.node as NodeView;

            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);

            edge.input = null;
            edge.output = null;

            RemoveElement(edge);

            Dirty(input);
            Dirty(output);
        }

        public void OpenSearch(Vector2 screenPosition, PortView connectedPort = null)
        {
            m_SearchProvider.connectedPort = connectedPort;
            SearchWindow.Open(new SearchWindowContext(screenPosition), m_SearchProvider);
        }
        
        /// <summary>
        /// Append views for nodes from a Graph
        /// </summary>
        private void AddNodes(List<AbstractNode> nodes, bool selectOnceAdded = false, bool centerOnMouse = false)
        {
            // Add views of each node from the graph
            Dictionary<AbstractNode, NodeView> nodeMap = new Dictionary<AbstractNode, NodeView>();
            foreach (var node in nodes)
            {
                var editorType = NodeReflection.GetNodeEditorType(node.GetType());
                var element = Activator.CreateInstance(editorType) as NodeView;
                element.Initialize(node, m_EdgeListener);
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
            foreach (var node in nodeMap)
            {
                foreach (var port in node.Key.ports)
                {
                    if (!port.isInput)
                    {
                        continue;
                    }

                    foreach (var conn in port.connections)
                    {
                        // Only add if the linked node is in the collection
                        if (nodeMap.ContainsKey(conn.node))
                        {
                            var inPort = node.Value.GetInputPort(port.portName);
                            var outPort = nodeMap[conn.node].GetOutputPort(conn.portName);
                        
                            if (inPort == null)
                            {
                                Debug.LogError(
                                    $"Could not connect `{node.Value.title}:{port.portName}` -> `{conn.node.name}:{conn.portName}`. " +
                                    $"Input port `{port.portName}` no longer exists.",
                                    node.Key
                                );
                            }
                            else if (outPort == null)
                            {
                                Debug.LogError(
                                    $"Could not connect `{conn.node.name}:{conn.portName}` to `{node.Value.name}:{port.portName}`. " +
                                    $"Output port `{conn.portName}` no longer exists.",
                                    conn.node
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

        private NodeView GetNodeElement(AbstractNode node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        /// <summary>
        /// Append views for comments from a Graph
        /// </summary>
        private void AddComments(List<GraphComment> comments)
        { 
            foreach (var comment in comments)
            {
                var commentView = new CommentView(comment);
                AddElement(commentView);
                Dirty(commentView);
            }
        }

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

            var comment = new GraphComment();
            comment.title = "New Comment";
            comment.position = bounds;

            var commentView = new CommentView(comment);
            commentView.onResize += Dirty;
            

            m_Graph.comments.Add(comment);
            m_CommentViews.Add(commentView);
            AddElement(commentView);

            Dirty(commentView);
        }
        
        /// <summary>
        /// Handler for deserializing a node from a string payload
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="data"></param>
        private void OnUnserializeAndPaste(string operationName, string data)
        {
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
            AddNodes(graph.nodes, true, true);
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
