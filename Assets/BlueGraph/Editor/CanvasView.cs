
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using BlueGraph;

namespace BlueGraphEditor
{
    /// <summary>
    /// Graph view that contains the nodes, edges, etc. 
    /// </summary>
    public class CanvasView : GraphView
    {
        public Label title;

        GraphEditor m_GraphEditor;
        Graph m_Graph;
        
        SearchProvider m_SearchProvider;
        EditorWindow m_EditorWindow;

        EdgeConnectorListener m_EdgeListener;
        
        HashSet<ICanDirty> m_Dirty = new HashSet<ICanDirty>();

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
            // Dirty moved elements to update the target assets
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is GroupView)
                    {
                        DirtyGroup(element as GroupView);
                    }
                    else if (element is ICanDirty)
                    {
                        Dirty(element as ICanDirty);
                    }

                    // TODO: Move/optimize
                    if (element is NodeView)
                    {
                        UpdateCommentLink(element as NodeView);
                    }
                }
            }
            
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is NodeView)
                    {
                        DestroyNode(element as NodeView);
                    }
                    else if (element is Edge)
                    {
                        DestroyEdge(element as Edge);
                    }
                    else if (element is CommentView)
                    {
                        DestroyComment(element as CommentView);
                    }
                }
                
                // Save the batch of changes all at once
                AssetDatabase.SaveAssets();
            }
            
            return change;
        }
        
        private void OnGraphKeydown(KeyDownEvent evt)
        {
            // TODO: Mac support

            // Group selected nodes
            if (evt.modifiers.HasFlag(EventModifiers.Control) && evt.keyCode == KeyCode.G)
            {
                GroupSelection();
            }
            
        
            // Other ideas:
            // - add comment node shortcut
            // - 
        }
        
        public void Load(Graph graph)
        {
            m_Graph = graph;
            
            AddNodes(graph.nodes);
            AddGroups(graph.groups);
        }
        
        public void CreateNode(Type type, Vector2 screenPosition, PortView connectedPort = null)
        {
            var windowRoot = m_EditorWindow.rootVisualElement;
            var windowMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo(
                windowRoot.parent, 
                screenPosition - m_EditorWindow.position.position
            );
        
            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
        
            var typeData = NodeReflection.GetNodeType(type);
            var node = m_Graph.AddNode(type);
            node.name = typeData.name;
            node.position = graphMousePosition;
        
            // Add a node to the visual graph
            var editorType = NodeReflection.GetNodeEditorType(type);
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
            comments.Remove(comment);
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
                foreach (var conn in edge.input.connections)
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

        public List<CommentView> comments = new List<CommentView>();
        
        /// <summary>
        /// Dirty the group and everything contained within it
        /// </summary>
        /// <param name="group"></param>
        public void DirtyGroup(GroupView group)
        {
            foreach (var element in group.containedElements)
            {
                if (element is ICanDirty)
                {
                    Dirty(element as ICanDirty);
                }
            }

            Dirty(group);
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
            foreach (var comment in comments)
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
        /// Append nodes from a Graph onto the viewport
        /// </summary>
        /// <param name="graph"></param>
        private void AddNodes(List<AbstractNode> nodes, bool selectOnceAdded = false)
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
            
            // Sync edges on the graph with our graph's connections 
            // TODO: Deal with trash connections from bad imports
            foreach (var node in nodeMap)
            {
                foreach (var port in node.Key.inputs)
                {
                    foreach (var conn in port.connections)
                    {
                        // Only add if the linked node is in the collection
                        if (nodeMap.ContainsKey(conn.node))
                        {
                            var inPort = node.Value.GetInputPort(port.portName);
                            var outPort = nodeMap[conn.node].GetOutputPort(conn.portName);
                        
                            if (inPort == null)
                            {
                                Debug.LogError($"Could not connect `{node.Value.title}:{port.portName}` -> `{conn.node.name}:{conn.portName}`. Input port `{port.portName}` no longer exists.");
                            }
                            else if (outPort == null)
                            {
                                Debug.LogError($"Could not connect `{conn.node.name}:{conn.portName}` to `{node.Value.name}:{port.portName}`. Output port `{conn.portName}` no longer exists.");
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
        /// Create views for a set of NodeGroups
        /// </summary>
        /// <param name="groups"></param>
        private void AddGroups(List<NodeGroup> groups)
        { 
            foreach (var group in groups)
            {
                var groupView = new GroupView(group);
                
                foreach (var node in group.nodes)
                {
                    groupView.AddElement(GetNodeElement(node));
                }
                
                AddElement(groupView);
                groupView.SetPosition(new Rect(group.position.x, group.position.y, 0, 0));
            }
        }

        private void GroupSelection()
        {
            if (selection.Count < 0)
            {
                return;
            }
            
            var group = new NodeGroup();
            group.title = "New Group";
           //  m_Graph.groups.Add(group);
            
           /* var groupView = new GroupView(group); 
            foreach (var node in selection)
            {
                if (node is NodeView)
                {
                    var nodeView = node as NodeView;
                    groupView.AddElement(nodeView);
                }
            }
            
            AddElement(groupView);*/

            var comment = new CommentView(group);
            comment.onResize += Dirty;

            comments.Add(comment);
            AddElement(comment);
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
            AddNodes(graph.nodes, true);
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
