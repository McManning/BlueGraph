
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Graph2
{
    /// <summary>
    /// Required concrete implementation of a GraphView
    /// </summary>
    class NodeGraphView : GraphView
    {
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
    }

    public class GraphViewElement : VisualElement
    {
        GraphEditor m_GraphEditor;
        Graph m_Graph;

        NodeGraphView m_GraphView;
        SearchProvider m_SearchProvider;
        EditorWindow m_EditorWindow;

        EdgeConnectorListener m_EdgeListener;
        
        public GraphViewElement(EditorWindow window)
        {
            m_EditorWindow = window;

            // TODO: Less hardcoded of a path
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Graph/Editor/Styles/GraphViewElement.uss"
            );
        
            styleSheets.Add(styles);
            
            m_EdgeListener = new EdgeConnectorListener(this);
            m_SearchProvider = ScriptableObject.CreateInstance<SearchProvider>();
            m_SearchProvider.graphView = this;

            CreateGraph();
        
            RegisterCallback<GeometryChangedEvent>(OnFirstResize);
        }
        
        /// <summary>
        /// Event handler to frame the graph view on initial layout
        /// </summary>
        private void OnFirstResize(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnFirstResize);
            m_GraphView.FrameAll();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            Debug.Log("Change: " + change.ToString());

            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is NodeView)
                    {
                        // TODO: something here?
                        var node = element as NodeView;
                        node.NodeData.position = element.GetPosition().position;
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    Debug.Log("Add edge : " + edge.output.name + " to " + edge.input.name);
                }

                // Intercept. We handled it already
                // change.edgesToCreate = null;
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is NodeView)
                    {
                        Debug.Log("Destroy node");
                        DestroyNode(element as NodeView);
                    }
                    else if (element is Edge)
                    {
                        Debug.Log("Destroy edge");
                        DestroyEdge(element as Edge);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Create and configure a new graph window for node editing
        /// </summary>
        private void CreateGraph()
        {
            m_GraphView = new NodeGraphView();
            m_GraphView.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
        
            // Manipulators for the graph view itself
            m_GraphView.AddManipulator(new ContentDragger());
            m_GraphView.AddManipulator(new SelectionDragger());
            m_GraphView.AddManipulator(new RectangleSelector());
            m_GraphView.AddManipulator(new ClickSelector());
        
            // Add event handlers for shortcuts and changes
            m_GraphView.RegisterCallback<KeyDownEvent>(OnGraphKeydown);
            m_GraphView.graphViewChanged = OnGraphViewChanged;
            
            m_GraphView.nodeCreationRequest = (ctx) => OpenSearch(ctx.screenMousePosition);
        
            /*
            // Add handlers for (de)serialization
            m_GraphView.serializeGraphElements = OnSerializeGraphElements;
            m_GraphView.canPasteSerializedData = OnTryPasteSerializedData;
            m_GraphView.unserializeAndPaste = OnUnserializeAndPaste;
            
            */

            VisualElement content = new VisualElement { name = "Content" };
            content.Add(m_GraphView);
        
            Add(content);
        }
       
        private void OnGraphKeydown(KeyDownEvent evt)
        {
        
        }
        
        public void Load(Graph graph)
        {
            Debug.Log("Load graph");
            m_Graph = graph;
            
            // Add views of each node from the graph
            Dictionary<AbstractNode, NodeView> nodeMap = new Dictionary<AbstractNode, NodeView>();
            foreach (var node in graph.Nodes)
            {
                var element = new NodeView();
                element.Initialize(node, m_EdgeListener);
                m_GraphView.AddElement(element);

                nodeMap.Add(node, element);
            }
            
            // Sync edges on the graph with our graph's connections 
            // TODO: Deal with trash connections from bad imports
            foreach (var node in nodeMap)
            {
                foreach (var port in node.Key.Inputs)
                {
                    foreach (var conn in port.Connections)
                    {
                        var inPort = node.Value.GetInputPort(port.fieldName);
                        var outPort = nodeMap[conn.Node].GetOutputPort(conn.FieldName);
                        
                        var edge = inPort.ConnectTo(outPort);
                        m_GraphView.AddElement(edge);
                    }
                }
            }

        }
        
        public void CreateNode(Type type, Vector2 screenPosition, PortView connectedPort = null)
        {
            var windowRoot = m_EditorWindow.rootVisualElement;
            var windowMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo(
                windowRoot.parent, 
                screenPosition - m_EditorWindow.position.position
            );
        
            var graphMousePosition = m_GraphView.contentViewContainer.WorldToLocal(windowMousePosition);
        
            var typeData = NodeReflection.GetNodeType(type);
            var node = m_Graph.AddNode(type);
            node.name = typeData.name;
            node.position = graphMousePosition;
        
            // Add a node to the visual graph
            var element = new NodeView();
            element.Initialize(node, m_EdgeListener);

            m_GraphView.AddElement(element);
            
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
        }

        public void DestroyNode(NodeView node)
        {
            // Remove all edges on the graph 
            
            /*
            foreach (var port in node.OutputPorts.Values)
            {
                foreach (var conn in port.connections)
                {
                    m_GraphView.RemoveElement(conn);
                }
            }
            
            foreach (var port in node.InputPorts.Values)
            {
                foreach (var conn in port.connections)
                {
                    m_GraphView.RemoveElement(conn);
                }
            }*/
            
            m_Graph.RemoveNode(node.NodeData);
            ScriptableObject.DestroyImmediate(node.NodeData, true);
            AssetDatabase.SaveAssets();
        }

        public void ConnectNodes(Edge edge)
        {
            if (edge.input == null || edge.output == null) return;

            var input = edge.input.node as NodeView;
            var output = edge.output.node as NodeView;
            
            Debug.Log($"{edge.input.portName} of {edge.input.title} to {edge.output.portName}");

            var inputPort = input.NodeData.GetInputPort(edge.input.portName);
            var outputPort = output.NodeData.GetOutputPort(edge.output.portName);

            Debug.Log(inputPort);
            Debug.Log(outputPort);
            Debug.Log(input.NodeData);
            Debug.Log(output.NodeData);

            // Skip pre-existing connections
            if (inputPort.IsConnected(output.NodeData, edge.output.portName))
            {
                Debug.Log("Input already connected");
                return;
            }

            if (outputPort.IsConnected(input.NodeData, edge.input.portName))
            {
                Debug.Log("Output already connected");
                return;
            }
            
            Debug.Log("Add connection");

            // Update the underlying asset
            inputPort.Connect(output.NodeData, edge.output.portName);
            outputPort.Connect(input.NodeData, edge.input.portName);
            
            // Add visual edge. Note this cannot be the input edge as it 
            // could be dropped from being tracked by the graph 
            // (thus not properly cleaned up later). Instead, we need to 
            // construct a new one and add that instead.
            var newEdge = edge.input.ConnectTo(edge.output);
            m_GraphView.AddElement(newEdge);
        }

        public void DestroyEdge(Edge edge)
        {
            Debug.Log("rm Edge " + edge.input.portName + " to " + edge.output.portName);
            var input = edge.input.node as NodeView;
            var output = edge.output.node as NodeView;
            
            var inputPort = input.NodeData.GetInputPort(edge.input.portName);
            var outputPort = output.NodeData.GetOutputPort(edge.output.portName);

            inputPort.Disconnect(output.NodeData, edge.output.portName);
            outputPort.Disconnect(input.NodeData, edge.input.portName);

            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);

            edge.input = null;
            edge.output = null;

            m_GraphView.RemoveElement(edge);
        }

        public void OpenSearch(Vector2 screenPosition, PortView connectedPort = null)
        {
            m_SearchProvider.connectedPort = connectedPort;
            SearchWindow.Open(new SearchWindowContext(screenPosition), m_SearchProvider);
        }
    }
}
