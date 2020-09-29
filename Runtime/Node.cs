using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    [Serializable]
    public abstract class Node : ISerializationCallbackReceiver
    {
        public event Action OnValidateEvent;
        public event Action OnErrorEvent;

        [SerializeField] private string id;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        [SerializeField] private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [SerializeField] private Graph graph;

        public Graph Graph
        {
            get { return graph; }
            internal set { graph = value; }
        }

        [SerializeField] private Vector2 position;

        /// <summary>
        /// Where this node is located on the Graph in CanvasView
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        [SerializeField] private List<Port> ports;

        [NonSerialized] private string error;

        /// <summary>
        /// Error information associated with this node
        /// </summary>
        public string Error
        {
            get
            {
                return error;
            }
            set
            {
                error = value;
                OnError();
                OnErrorEvent?.Invoke();
            }
        }

        /// <summary>
        /// Accessor for ports and their connections to/from this node.
        /// </summary>
        public IList<Port> Ports
        {
            get { return ports.AsReadOnly(); }
        }

        public Node()
        {
            ID = Guid.NewGuid().ToString();
            ports = new List<Port>();
        }

        public virtual void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(Name))
                return;
            if (Graph == null)
            {
                throw new Exception(
                    $"[{Name} - {ID}] Node OnEnable without a graph reference. " +
                    $"This could point to a potential memory leak"
                );
            }
        }

        public virtual void OnBeforeSerialize() { }

        public void Enable()
        {
            if (string.IsNullOrEmpty(Name))
                return;

            // Ports are enabled first to ensure they're fully loaded
            // prior to enabling the node itself, in case the node needs
            // to query port data during OnEnable.
            foreach (var port in ports)
            {
                // Add a backref to each child port of this node.
                // We don't store this in the serialized copy to avoid cyclic refs.
                port.Node = this;
                port.OnEnable();
            }

            OnEnable();
        }

        /// <summary>
        /// Called when the Graph's ScriptableObject gets the OnEnable message
        /// or when the node is added to the graph via <c>Graph.AddNode</c>
        /// </summary>
        public virtual void OnEnable() { }

        public void Disable()
        {
            if (string.IsNullOrEmpty(Name))
                return;
            OnDisable();
        }

        /// <summary>
        /// Called when the Graph's ScriptableObject gets the OnDisable message
        /// or when the node is removed from the graph via <c>Graph.RemoveNode</c>
        /// </summary>
        public virtual void OnDisable() { }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                return;
            // Same as Enable(), we do ports first to make sure
            // everything is ready for the node's OnValidate
            foreach (var port in ports)
            {
                port.Node = this;
                port.OnValidate();
            }

            OnValidate();
            OnValidateEvent?.Invoke();
        }

        /// <summary>
        /// Called in the editor when the node or graph is revalidated. 
        /// </summary>
        public virtual void OnValidate() { }

        /// <summary>
        /// Called when added via <c>Graph.AddNode</c>. 
        /// 
        /// This comes <b>before</b> <c>OnEnable</c>
        /// </summary>
        public virtual void OnAddedToGraph() { }

        /// <summary>
        /// Called when added via <c>Graph.RemoveNode</c>. 
        /// 
        /// This comes <b>after</b> <c>OnDisable</c>. 
        /// 
        /// <c>Node.Graph</c> property is still valid during this call. 
        /// </summary>
        public virtual void OnRemovedFromGraph() { }

        /// <summary>
        /// Called when the <c>Error</c> property is modified.
        /// </summary>
        public virtual void OnError() { }

        /// <summary>
        /// Resolve the return value associated with the given port. 
        /// </summary>
        public abstract object OnRequestValue(Port port);

        /// <summary>
        /// Get either an input or output port by name.
        /// </summary>
        public Port GetPort(string name)
        {
            return ports.Find((port) => port.Name == name);
        }

        /// <summary>
        /// Add a new port to this node.
        /// </summary>
        public void AddPort(Port port)
        {
            var existing = GetPort(port.Name);
            if (existing != null)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> A port named `{port.Name}` already exists"
                );
            }

            ports.Add(port);
            port.Node = this;
        }

        /// <summary>
        /// Remove an existing port from this node.
        /// </summary>
        public void RemovePort(Port port)
        {
            port.DisconnectAll(); 
            port.Node = null;

            ports.Remove(port);
        }

        /// <summary>
        /// Safely remove every edge going in and out of this node.
        /// </summary>
        public void DisconnectAllPorts()
        {
            foreach (var port in ports)
            {
                port.DisconnectAll();
            }
        }

        /// <summary>
        /// Get the value returned by an output port connected to the given port.
        /// 
        /// This will return <c>defaultValue</c> if the port is disconnected.
        /// </summary>
        public T GetInputValue<T>(string portName, T defaultValue = default)
        {
            var port = GetPort(portName);
            return GetInputValue<T>(port, defaultValue);
        }

        /// <summary>
        /// Get the value returned by an output port connected to the given port.
        /// 
        /// This will return <c>defaultValue</c> if the port is disconnected.
        /// </summary>
        public T GetInputValue<T>(Port port, T defaultValue = default)
        {
            if (port == null)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> Null input port parameter"
                );
            }

            if (port.Direction == PortDirection.Output)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> Wrong input port direction `{port.Name}`"
                );
            }

            return port.GetValue(defaultValue);
        }

        /// <summary>
        /// Get a list of output values for all output ports connected
        /// to the given input port. 
        /// 
        /// This will return an empty list if the port is disconnected.
        /// </summary>
        public IEnumerable<T> GetInputValues<T>(string portName)
        {
            var port = GetPort(portName);
            return GetInputValues<T>(port);
        }

        /// <summary>
        /// Get a list of output values for all output ports connected
        /// to the given input port. 
        /// 
        /// This will return an empty list if the port is disconnected.
        /// </summary>
        public IEnumerable<T> GetInputValues<T>(Port port)
        {
            if (port == null)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> Null input port parameter"
                );
            }

            if (port.Direction == PortDirection.Output)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> Wrong input port direction `{port.Name}`"
                );
            }

            return port.GetValues<T>();
        }

        /// <summary>
        /// Get the calculated value of a given output port.
        /// </summary>
        public T GetOutputValue<T>(string portName)
        {
            var port = GetPort(portName);
            return GetOutputValue<T>(port);
        }
        /// <summary>
        /// Get the calculated value of a given output port.
        /// </summary>
        public T GetOutputValue<T>(Port port)
        {
            if (port == null)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> Null output port parameter"
                );
            }

            if (port.Direction == PortDirection.Input)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> Wrong output port direction `{port.Name}`"
                );
            }

            return port.GetValue(default(T));
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return "NullNode";
            return $"{GetType()}({Name}, {ID})";
        }
    }
}
