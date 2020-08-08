
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

            AddPort(new Port
            { 
                name = "Input",
                isInput = true,
                ConnectionType = typeof(Vector3) 
            });

            AddPort(new Port
            { 
                name = "Output",
                isInput = false,
                ConnectionType = typeof(string) 
            });
        }
        
        public override object OnRequestValue(Port port)
        {
            throw new System.NotImplementedException();
        }
    }
}
