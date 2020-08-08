
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlueGraph.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type nodeType;

        public CustomNodeViewAttribute(Type nodeType)
        {
            this.nodeType = nodeType; 
        }
    }
    
    public class NodeView : Node, ICanDirty
    {
        public AbstractNode target;
        
        public List<PortView> inputs = new List<PortView>();
        public List<PortView> outputs = new List<PortView>();
        
        protected EdgeConnectorListener m_ConnectorListener;
        protected SerializedProperty m_SerializedNode;
        
        public void Initialize(AbstractNode node, SerializedProperty serializedNode, EdgeConnectorListener connectorListener)
        {
            viewDataKey = node.id;
            target = node;

            m_SerializedNode = serializedNode;
            m_ConnectorListener = connectorListener;
            
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/NodeView"));
            AddToClassList("nodeView");
            
            SetPosition(new Rect(node.graphPosition, Vector2.one));
            title = node.name;
            
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
                if (port.isInput)
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
            var field = ControlElementFactory.CreateControl(
                editable.field, 
                this, 
                editable.displayName
            );

            extensionContainer.Add(field);
        }

        protected virtual void AddInputPort(Port port)
        {
            var view = PortView.Create(port, port.Type, m_ConnectorListener);

            // If we want to display an inline editable field as part 
            // of the port, instantiate a new inline editor control 
            if (port.fieldName != null && port.fieldName.Length > 0)
            {
                var reflection = NodeReflection.GetNodeType(target.GetType());
                var field = reflection.GetControlElement(this, port.fieldName);

                if (field != null)
                {
                    // field.RegisterCallback((FocusOutEvent e) => OnPropertyChange());

                    var container = new VisualElement();
                    container.AddToClassList("property-field-container");
                    container.Add(field);

                    view.SetEditorField(container);
                }
            }
            
            inputs.Add(view);
            inputContainer.Add(view);
        }

        protected virtual void AddOutputPort(Port port)
        {
            var view = PortView.Create(port, port.Type, m_ConnectorListener);
            
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
            // TODO: Cache. This lookup will be slow.
            var canvas = GetFirstAncestorOfType<CanvasView>();
            canvas?.Dirty(this);
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
            
            return new Rect(target.graphPosition, Vector2.one);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            target.graphPosition = newPos.position;
        }
        
        protected void OnTooltip(TooltipEvent evt)
        {
            // TODO: Better implementation that can be styled
            if (evt.target == titleContainer.Q("title-label"))
            {
                var typeData = NodeReflection.GetNodeType(target.GetType());
                evt.tooltip = typeData?.tooltip;
                
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
