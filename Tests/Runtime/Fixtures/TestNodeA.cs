
namespace BlueGraph.Tests
{
    public class TestNodeA : AbstractNode
    {
        public int aValue1;
        public bool aValue2;
    
        public TestNodeA() : base()
        {
            name = "Test Node A";
        }

        public override void OnAddedToGraph()
        {
            base.OnAddedToGraph();
        
            AddPort(new Port { name = "Input" });
            AddPort(new Port { name = "Output" });
        }
    }

    public class InheritedTestNodeA : TestNodeA
    {
        
    }
}
