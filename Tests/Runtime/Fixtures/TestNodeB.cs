
using UnityEngine;

namespace BlueGraph.Tests
{
    public class TestNodeB : AbstractNode
    {
        public Vector3 bValue1;
        public string bValue2;
    
        public TestNodeB() : base()
        {
            name = "Test Node B";
        }

        public override void OnRequestPorts()
        {
            AddPort(new InputPort<Vector3> { name = "Input" });
            AddPort(new OutputPort<float> { name = "Output" });
        }
    }
}
