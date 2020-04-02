
using UnityEngine;

namespace BlueGraph.Tests
{
    public class TestNodeB : AbstractNode
    {
        [Input("Input")]
        public Vector3 bValue1;

        [Output("Output")]
        public string bValue2;
    
        public TestNodeB() : base()
        {
            name = "Test Node B";
        }
        
        public override void OnRequestPorts()
        {
            // Ports need to be added manually here because we don't 
            // use the graph editor to instantiate nodes under test.
            if (GetPort("Input") == null)
            {
                AddPort(new Port
                { 
                    name = "Input",
                    fieldName = "bValue1",
                    isInput = true,
                    Type = typeof(Vector3) 
                });
            }
            
            if (GetPort("Output") == null)
            {
                AddPort(new Port
                { 
                    name = "Output",
                    fieldName = "bValue2",
                    isInput = false,
                    Type = typeof(string) 
                });
            }
        }

        public override object OnRequestValue(Port port)
        {
            throw new System.NotImplementedException();
        }
    }
}
