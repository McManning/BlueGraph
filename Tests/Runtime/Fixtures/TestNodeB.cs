
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

        public override void OnAddedToGraph()
        {
            base.OnAddedToGraph();
        
            AddPort(new Port { name = "Input" });
            AddPort(new Port { name = "Output" });
        }
    }
}
