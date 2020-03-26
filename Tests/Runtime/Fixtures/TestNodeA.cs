
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

        public override void OnRequestPorts()
        {
            AddPort(new InputPort<int> { name = "Input" });
            AddPort(new OutputPort<int> { name = "Output" });
        }
    }

    public class InheritedTestNodeA : TestNodeA
    {
        
    }
}
