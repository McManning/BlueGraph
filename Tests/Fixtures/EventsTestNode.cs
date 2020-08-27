namespace BlueGraph.Tests
{
    /// <summary>
    /// Test node that tracks what events were fired during a test
    /// </summary>
    public class EventTestNode : Node
    {
        public int onAddedToGraphCount = 0;
        public int onRemovedFromGraphCount = 0;
        public int onBeforeSerializeCount = 0;
        public int onAfterDeserializeCount = 0;
    
        public EventTestNode() : base()
        {
            Name = "Test Node B";
            
            AddPort(new InputPort<float> { Name = "Input" });
            AddPort(new OutputPort<float> { Name = "Output" });
        }

        public override void OnAddedToGraph()
        {
            onAddedToGraphCount++;
            base.OnAddedToGraph();
        }

        public override void OnRemovedFromGraph()
        {
            onRemovedFromGraphCount++;
            base.OnRemovedFromGraph();
        }

        public override void OnBeforeSerialize()
        {
            onBeforeSerializeCount++;
            base.OnBeforeSerialize();
        }

        public override void OnAfterDeserialize()
        {
            onAfterDeserializeCount++;
            base.OnAfterDeserialize();
        }

        public override object OnRequestValue(Port port)
        {
            throw new System.NotImplementedException();
        }
    }
}
