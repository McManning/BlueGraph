
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
                direction = PortDirection.Input,
                type = typeof(Vector3) 
            });

            AddPort(new Port
            { 
                name = "Output",
                direction = PortDirection.Output,
                type = typeof(string) 
            });
        }
        
        public override object OnRequestValue(Port port)
        {
            throw new System.NotImplementedException();
        }
    }
}
