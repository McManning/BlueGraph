
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

using UnityEditor.Experimental.GraphView;
using GraphViewNode = UnityEditor.Experimental.GraphView.Node;

namespace BlueGraph.Editor
{
    public class NodeView : GraphViewNode, ICanDirty
    {
        public Node target;
        
        public List<PortView> inputs = new List<PortView>();
        public List<PortView> outputs = new List<PortView>();
        
        protected EdgeConnectorListener m_ConnectorListener;
        protected SerializedProperty m_SerializedNode;
        protected NodeReflectionData m_ReflectionData;
        protected CanvasView m_Canvas;
        
        public void Initialize(Node node, CanvasView canvas, EdgeConnectorListener connectorListener)
        {
            viewDataKey = node.id;
            target = node;
            m_Canvas = canvas;
            m_ReflectionData = NodeReflection.GetNodeType(node.GetType());
            m_ConnectorListener = connectorListener;
            
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/NodeView"));
            AddToClassList("nodeView");
            
            SetPosition(new Rect(node.position, Vector2.one));
            title = node.name;
            
            if (!m_ReflectionData.deletable)
            {
                capabilities &= ~Capabilities.Deletable;
            }
            
            // Custom OnDestroy() handler via https://forum.unity.com/threads/request-for-visualelement-ondestroy-or-onremoved-event.718814/
            RegisterCallback<DetachFromPanelEvent>((e) => OnDestroy());
            RegisterCallback<TooltipEvent>(OnTooltip);

            UpdatePorts();

            OnInitialize();
        }

        /// <summary>
        /// Executed once this node has been added to the canvas
        /// </summary>
        protected virtual void OnInitialize()
        {

        }
        
        /// <summary>
        /// Executed when we're about to detach this element from the graph. 
        /// </summary>
        protected virtual void OnDestroy()
        {
            
        }
        
        /// <summary>
        /// Make sure our list of PortViews and editables sync up with our NodePorts
        /// </summary>
        protected void UpdatePorts()
        {
            foreach (var port in target.Ports)
            {
                if (port.direction == PortDirection.Input)
                {
                    AddInputPort(port);
                }
                else
                {
                    AddOutputPort(port);
                }
            }
            
            var reflectionData = NodeReflection.GetNodeType(target.GetType());
            if (reflectionData != null) 
            {
                foreach (var editable in reflectionData.editables)
                {
                    AddEditableField(editable);
                }
            }
            
            // Toggle visibility of the extension container
            RefreshExpandedState();

            // Update state classes
            EnableInClassList("hasInputs", inputs.Count > 0);
            EnableInClassList("hasOutputs", outputs.Count > 0);
        }

        protected void AddEditableField(EditableReflectionData editable)
        {
            var field = editable.GetControlElement(this);
            extensionContainer.Add(field);
        }

        protected virtual void AddInputPort(Port port)
        {
            var view = PortView.Create(port, m_ConnectorListener);
            
            // If we're exposing a control element via reflection: include it in the view
            var reflection = NodeReflection.GetNodeType(target.GetType());
            var element = reflection.GetPortByName(port.name)?.GetControlElement(this);

            if (element != null)
            {
                var container = new VisualElement();
                container.AddToClassList("property-field-container");
                container.Add(element);

                view.SetEditorField(container);
            }

            inputs.Add(view);
            inputContainer.Add(view);
        }

        protected virtual void AddOutputPort(Port port)
        {
            var view = PortView.Create(port, m_ConnectorListener);
            
            outputs.Add(view);
            outputContainer.Add(view);
        }

        public PortView GetInputPort(string name)
        {
            return inputs.Find((port) => port.portName == name);
        }

        public PortView GetOutputPort(string name)
        {
            return outputs.Find((port) => port.portName == name);
        }
        
        public PortView GetCompatibleInputPort(PortView output)
        { 
            return inputs.Find((port) => port.IsCompatibleWith(output));
        }
    
        public PortView GetCompatibleOutputPort(PortView input)
        {
            return outputs.Find((port) => port.IsCompatibleWith(input));
        }

        /// <summary>
        /// A property has been updated, either by a port or a connection 
        /// </summary>
        public virtual void OnPropertyChange()
        {
            m_Canvas?.Dirty(this);
        }
        
        /// <summary>
        /// Dirty this node in response to a change in connectivity. Invalidate
        /// any cache in prep for an OnUpdate() followup call. 
        /// </summary>
        public virtual void OnDirty()
        {
            // Dirty all ports so they can refresh their state
            inputs.ForEach(port => port.OnDirty());
            outputs.ForEach(port => port.OnDirty());
        }

        /// <summary>
        /// Called when this node was dirtied and the UI is redrawing. 
        /// </summary>
        public virtual void OnUpdate()
        {
            // Propagate update to all ports
            inputs.ForEach(port => port.OnUpdate());
            outputs.ForEach(port => port.OnUpdate());
        }

        public override Rect GetPosition()
        {
            // The default implementation doesn't give us back a valid position until layout is resolved.
            // See: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/GraphViewEditor/Elements/Node.cs#L131
            Rect position = base.GetPosition();
            if (position.width > 0 && position.height > 0)
            {
                return position;
            }
            
            return new Rect(target.position, Vector2.one);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            target.position = newPos.position;
        }
        
        protected void OnTooltip(TooltipEvent evt)
        {
            // TODO: Better implementation that can be styled
            if (evt.target == titleContainer.Q("title-label"))
            {
                var typeData = NodeReflection.GetNodeType(target.GetType());
                evt.tooltip = typeData?.help;
                
                // Float the tooltip above the node title bar
                var bound = titleContainer.worldBound;
                bound.x = 0;
                bound.y = 0;
                bound.height *= -1;
                
                evt.rect = titleContainer.LocalToWorld(bound);
            }
        }
    }
}
