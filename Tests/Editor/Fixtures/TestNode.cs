
namespace BlueGraph.EditorTests
{
    public class TestNode : AbstractNode
    {
        public int aValue1;
        public bool aValue2;
    
        public TestNode() : base()
        {
            name = "Test Node";
        }

        public override void OnRequestPorts()
        {
            AddPort(new Port { Type = typeof(int), name = "Input", isInput = true });
            AddPort(new Port { Type = typeof(int), name = "Output" });
        }
    }
}
