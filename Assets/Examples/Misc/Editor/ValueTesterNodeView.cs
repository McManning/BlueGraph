
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using BlueGraph;
using BlueGraphEditor;
using System;

namespace BlueGraphExamples
{
    [CustomNodeView(typeof(ValueTester))]
    class ValueTesterNodeView : NodeView
    {
        Label m_CurrentValue;

        public override void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            base.Initialize(node, connectorListener);

            m_CurrentValue = new Label();

            extensionContainer.Add(m_CurrentValue);
            RefreshExpandedState();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

           // try
           // {
                m_CurrentValue.text = "f: " + target.GetInputValue<float>("F") + "\n" + 
                    "v2: " + target.GetInputValue<Vector2>("V2") + "\n" +         
                    "v3: " + target.GetInputValue<Vector3>("V3") + "\n" +
                    ""; // "dv: " + (DynamicVector)target.GetOutputValue("dv");
           /* } 
            catch (Exception e)
            {
                var v = target.GetOutputValue("f");

                m_CurrentValue.text = e.Message;

                if (v != null) m_CurrentValue.text += "\n" + v.GetType();
            }*/
            
        }
    }
}
