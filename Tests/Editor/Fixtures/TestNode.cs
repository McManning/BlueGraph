
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

        public override void OnAddedToGraph()
        {
            base.OnAddedToGraph();
        
            AddPort(new Port { name = "Input" });
            AddPort(new Port { name = "Output" });
        }
    }
}
