
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using BlueGraph.Editor;
using UnityEditor.Experimental.GraphView;
using Port = BlueGraph.Port;

namespace BlueGraphSamples
{
    [CustomNodeView(typeof(OutputNode))]
    public class OutputNodeView : NodeView
    {
        TextField m_NameField;
        PopupField<string> m_TypeField;
        PortView m_Port;
        
        /// <summary>
        /// Input types allowed for this subgraph
        /// </summary>
        static Type[] k_SupportedTypes = new Type[]
        {
            typeof(float),
            typeof(string),
            typeof(bool),
            typeof(Vector2),
            typeof(Vector3)
        };

        /// <summary>
        /// Add custom classes and field editors on initialize
        /// </summary>
        protected override void OnInitialize()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/OutputNodeView"));
            AddToClassList("subgraphOutput");

            // Replace the title bar with an editable name
            m_NameField = new TextField
            {
                value = target.name
            };

            m_NameField.RegisterValueChangedCallback((change) => OnSettingsChange());
            m_NameField.RegisterCallback<FocusOutEvent>((e) => { 
                if (m_NameField.value.Length < 1)
                {
                    m_NameField.value = "New Output";
                }
            });
            
            var icon = new VisualElement();
            icon.name = "icon";

            var title = this.Q("title");
            title.Clear();

            title.Add(m_NameField);
            title.Add(icon);
        }
        
        /// <summary>
        /// Add a custom PopupField associated with the input port 
        /// to dynamically modify the type of input for this node 
        /// </summary>
        /// <param name="port"></param>
        protected override void AddInputPort(Port port)
        {
            m_Port = PortView.Create(port, port.Type, m_ConnectorListener);
            
            var defaultIndex = 0;
            var names = new List<string>();
            for (var i = 0; i < k_SupportedTypes.Length; i++)
            {
                names.Add(k_SupportedTypes[i].Name);

                if (k_SupportedTypes[i] == port.Type) 
                {
                    defaultIndex = i;
                }
            }

            m_TypeField = new PopupField<string>(names, defaultIndex);
            m_TypeField.RegisterValueChangedCallback((change) => OnSettingsChange());

            var container = new VisualElement();
            container.AddToClassList("property-field-container");
            container.Add(m_TypeField);

            m_Port.SetEditorField(container);
            m_Port.hideEditorFieldOnConnection = false;

            inputs.Add(m_Port);
            inputContainer.Add(m_Port);
        }

        void OnSettingsChange()
        {
            title = m_NameField.value;
            target.name = m_NameField.value;

            foreach (var type in k_SupportedTypes)
            {
                if (type.Name == m_TypeField.value)
                {
                    UpdateInputType(type);
                }
            }
        }

        void UpdateInputType(Type type)
        {
            // If the port is already of the same type, don't regenerate
            var port = m_Port.target;
            if (port.Type == type)
            {
                return;
            }

            // TODO: Cleaner way of refactoring a port. This is awful.

            // Disconnect all existing connections. 
            // This has to be done from the canvas view.
            var canvas = GetFirstAncestorOfType<CanvasView>();
            var edges = new List<Edge>(m_Port.connections);
            foreach (var edge in edges) 
            {
                canvas.RemoveEdge(edge, false);
            }

            // Remove references
            inputContainer.Remove(m_Port);
            inputs.Remove(m_Port);
            target.RemovePort(port);
            m_Port = null;

            // Create a new replacement port of the new type
            port = new Port
            {
                isInput = true,
                Type = type,
                acceptsMultipleConnections = false
            };
            
            target.AddPort(port);
            AddInputPort(port);
        }
    }
}
