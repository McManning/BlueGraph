
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;

namespace BlueGraphEditor
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
        
        public CommentView comment;

        protected EdgeConnectorListener m_ConnectorListener;
        protected SerializedObject m_SerializedNode;

        public virtual void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            viewDataKey = node.guid;
            target = node;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/NodeView"));
            AddToClassList("nodeView");
            
            SetPosition(new Rect(node.position, Vector2.one));
            m_ConnectorListener = connectorListener;
            title = node.name;

            Debug.Log("NODE POS: " + GetPosition());
            
            m_SerializedNode = new SerializedObject(node);
            
            // Custom OnDestroy() handler via https://forum.unity.com/threads/request-for-visualelement-ondestroy-or-onremoved-event.718814/
            RegisterCallback<DetachFromPanelEvent>((e) => OnDestroy());
            RegisterCallback<TooltipEvent>(OnTooltip);

            UpdatePorts();

            // TODO: Don't do it this way.
            if (node is FuncNode func)
            {
                func.Awake();
            }
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
            foreach (var port in target.ports)
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
            
            // TODO: Support FuncNode since GetNodeType won't work for those
            // as they're registered under a different type. 
            var reflectionData = NodeReflection.GetNodeType(target.GetType());
            if (reflectionData != null) 
            {
                foreach (var editable in reflectionData.editables)
                {
                    AddEditableField(m_SerializedNode.FindProperty(editable.fieldName));
                }
            }
            
            // Toggle visibility of the extension container
            RefreshExpandedState();
        }

        protected void AddEditableField(SerializedProperty prop)
        {
            var field = new PropertyField(prop);
            field.Bind(m_SerializedNode);
            
            extensionContainer.Add(field);
        }

        protected virtual void AddInputPort(NodePort port)
        {
            var view = PortView.Create(
                port, 
                m_SerializedNode.FindProperty(port.fieldName), 
                port.type,
                m_ConnectorListener,
                true
            );
            
            inputs.Add(view);
            inputContainer.Add(view);
        }
        
        protected virtual void AddOutputPort(NodePort port)
        {
            var view = PortView.Create(
                port,
                m_SerializedNode.FindProperty(port.fieldName), 
                port.type,
                m_ConnectorListener,
                false
            );
            
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
