
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// Primary component that manages graph editor interactions
/// </summary>
public class GraphViewElement : VisualElement
{
    /// <summary>
    /// Listeners for when the graph changes from the last save state
    /// </summary>
    public Action onDirty;
    
    /// <summary>
    /// Listeners for when the underlying asset has synced with the graph
    /// </summary>
    public Action<SerializableGraph> onSave;
    
    bool m_Dirty;
    
    NodeGraphView m_GraphView;
    SearchProvider m_SearchProvider;
    
    // TODO: The only reason we keep this reference is for calculating local coordinates
    // from the screen. Is there a cleaner way of doing that?
    readonly EditorWindow m_EditorWindow;

    readonly EdgeConnectorListener m_EdgeListener;

    readonly GraphSerializer m_Serializer;

    public GraphViewElement(EditorWindow window)
    {
        m_EditorWindow = window;

        // TODO: Less hardcoded of a path
        StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Graph/Editor/Styles/GraphViewElement.uss"
        );
        
        styleSheets.Add(styles);

        CreateToolbar();
        CreateSearchProvider();
        CreateGraph();
        
        m_EdgeListener = new EdgeConnectorListener(m_SearchProvider);
        m_Serializer = new GraphSerializer(m_EdgeListener);
        
        RegisterCallback<GeometryChangedEvent>(OnFirstResize);
    }
    
    /// <summary>
    /// Event handler to frame the graph view on initial layout
    /// </summary>
    private void OnFirstResize(GeometryChangedEvent evt)
    {
        Debug.Log("First resize");

        // On first load only, frame all nodes. 
        UnregisterCallback<GeometryChangedEvent>(OnFirstResize);
        m_GraphView.FrameAll();
    }
    
    private void CreateToolbar()
    {
        IMGUIContainer toolbar = new IMGUIContainer(() => {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton))
            {
                Save();
            }
        
            // Other toolbar buttons here.
    
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });
        
        Add(toolbar);
    }
   
    /// <summary>
    /// Sync the underlying asset with the graph
    /// </summary>
    private void Save()
    {
        var graph = m_Serializer.CreateIndependentGraph(
            m_GraphView.graphElements.ToList()
       );

        m_Dirty = false;
        
        // Notify listeners with the new graph
        onSave?.Invoke(graph);
    }
    
    /// <summary>
    /// Mark the graph as dirty (different than the underlying asset)
    /// </summary>
    private void Dirty()
    {
        if (!m_Dirty)
        {
            m_Dirty = true;
            onDirty?.Invoke();
        }
    }
    
    /// <summary>
    /// Create and configure a new graph window for node editing
    /// </summary>
    private void CreateGraph()
    {
        Debug.Log("Create graph");

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
        m_GraphView.nodeCreationRequest = (ctx) =>
        {
            SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), m_SearchProvider);
        };
        
        // Add handlers for (de)serialization
        m_GraphView.serializeGraphElements = OnSerializeGraphElements;
        m_GraphView.canPasteSerializedData = OnTryPasteSerializedData;
        m_GraphView.unserializeAndPaste = OnUnserializeAndPaste;
        
        VisualElement content = new VisualElement { name = "Content" };
        content.Add(m_GraphView);
        
        Add(content);
    }

    /// <summary>
    /// Setup an instance of the SearchProvider used to lookup available nodes to add to the graph
    /// </summary>
    private void CreateSearchProvider()
    {
        m_SearchProvider = ScriptableObject.CreateInstance<SearchProvider>();
        m_SearchProvider.onCreateNode = OnCreateNode;
    }

    // TODO: There's an underlying Guid lookup system (GraphView.GetNodeByGuid), 
    // but it uses .viewDataKey of VisualElement. Seems a bit sketch so I'm hesitant to use it just yet.
    private Dictionary<AbstractNode, NodeView> m_NodeElements = new Dictionary<AbstractNode, NodeView>();

    public void Load(SerializableGraph graph)
    {
        var elements = m_Serializer.Unserialize(graph);
        foreach (var element in elements)
        {
            m_GraphView.AddElement(element);
        }
    }
    
    /// <summary>
    /// Load the initial serialized data into the graph instance
    /// </summary>
    private void PopulateGraph()
    {
        // Add nodes and edges to the graph
        
        /*
        NodeView element = new NodeView();
        m_GraphView.AddElement(element);
        
        var type = NodeReflection.GetNodeType(typeof(TestNode));

        element.Initialize(type, Vector2.zero, m_EdgeListener);

        var element2 = new NodeView();
        m_GraphView.AddElement(element2);
        
        element2.Initialize(type, Vector2.right * 100f, m_EdgeListener);
        
        // Note that elements need to be added to both the graph 
        // and the group for it to render properly. There's no way to 
        // add/remove though from the group manually (yet)
        var group = new Group(); // group is just a named Scope
        group.title = "Foo bar";
        group.AddElement(element);
        group.AddElement(element2);
        m_GraphView.AddElement(group);
        */

    }

    /// <summary>
    /// Dirty the underlying asset based on changes to the editable graph.
    /// Event is fired whenever we move nodes or add/remove nodes from the GraphView.
    /// </summary>
    /// <param name="change"></param>
    /// <returns></returns>
    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        // change has edgesToCreate, movedElements (nodes), elementsToRemove (nodes and edges)
        
        // TODO: track changes? Or just rescan the entire graph on a save.
        // For now we'll just persist the whole graph on save performance takes a hit. 
        
        // TODO: Seems edgesToCreate isn't populated when we add an edge
        // thorugh EdgeConnectorListener. I guess because it's a manual action? 
        
        Dirty();
        
        return change;
    }
    
    /// <summary>
    /// Handle custom keyboard shortcuts on the graph 
    /// </summary>
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
    
    /// <summary>
    /// Add a group around selected nodes
    /// </summary>
    private void GroupSelection()
    {
        var group = new Group();
        group.title = "New Group";
        
        // TODO: deal with nodes that are already in a group since we can't nest groups
        // (probably just re-assignment)
        m_GraphView.selection.ForEach((e) => group.AddElement(e as GraphElement));
        m_GraphView.AddElement(group);
    }
    
    /// <summary>
    /// Event handler for when a node is selected by the search provider
    /// </summary>
    public void OnCreateNode(NodeType nodeType, NodePort fromPort, Vector2 screenPosition)
    {
        // Translate cursor position to local to this graph
        var windowRoot = m_EditorWindow.rootVisualElement;
        var windowMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo(
            windowRoot.parent, 
            screenPosition - m_EditorWindow.position.position
        );
        
        var graphMousePosition = m_GraphView.contentViewContainer.WorldToLocal(windowMousePosition);
        
        // Add a new instance of the linked data
        var node = Activator.CreateInstance(nodeType.InstanceType) as AbstractNode;
        node.InitializeFromType(nodeType);

        Debug.Log(node);

        // Add a new element to the graph 
        var element = new NodeView();
        m_GraphView.AddElement(element);
        element.Initialize(node, graphMousePosition, m_EdgeListener);

        // If we're connecting to a pre-existing node, find a compatible
        // port in the correct dimension on the node and add an edge.
        if (fromPort != null)
        {
            Port toPort = null;
            if (fromPort.direction == Direction.Output)
            {
                toPort = element.GetCompatibleInputPort(fromPort);
            }
            else
            {
                toPort = element.GetCompatibleOutputPort(fromPort);
            }
            
            if (toPort != null)
            {
                var edge = fromPort.ConnectTo(toPort);
                m_GraphView.AddElement(edge);
            }
        }

        Dirty();
    }
    
    /// <summary>
    /// Handler for deserializing a node from a string payload
    /// </summary>
    /// <param name="operationName"></param>
    /// <param name="data"></param>
    private void OnUnserializeAndPaste(string operationName, string data)
    {
        Debug.Log("Operation name: " + operationName);

        var elements = m_Serializer.Unserialize(data);

        // TODO: Overlap detection / duplication handling / etc.
        foreach (var element in elements)
        {
            m_GraphView.AddElement(element);
        }
        
        Dirty();
    }

    private bool OnTryPasteSerializedData(string data)
    {
        return m_Serializer.CanUnserialize(data);
    }

    /// <summary>
    /// Serialize a selection to support cut/copy/duplicate
    /// </summary>
    private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
    {
        return m_Serializer.Serialize(elements);
    }
}
