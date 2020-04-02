
using UnityEngine;

namespace BlueGraph.Tests
{
    /// <summary>
    /// Node without any ports or fields to test with
    /// </summary>
    public class EmptyNode : AbstractNode
    {
        public EmptyNode() : base()
        {
            name = "Empty Node";
        }

        public override object OnRequestValue(Port port)
        {
            throw new System.NotImplementedException();
        }
    }
}
