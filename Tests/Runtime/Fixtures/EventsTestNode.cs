
using UnityEngine;

namespace BlueGraph.Tests
{
    /// <summary>
    /// Test node that tracks what events were fired during a test
    /// </summary>
    public class EventTestNode : AbstractNode
    {
        public int onAddedToGraphCount = 0;
        public int onRemovedFromGraphCount = 0;
        public int onBeforeSerializeCount = 0;
        public int onAfterDeserializeCount = 0;
    
        public EventTestNode() : base()
        {
            name = "Test Node B";
        }

        public override void OnAddedToGraph()
        {
            onAddedToGraphCount++;
            base.OnAddedToGraph();
        
            AddPort(new Port { name = "Input" });
            AddPort(new Port { name = "Output" });
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
    }
}
