
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(module = "Math")]
    public class AddFloats : AbstractNode
    {
        // Define two input ports, A and B
        [Input] public float a;
        [Input] public float b;

        // Define one output port: Result
        [Output] readonly float result;
        
        /// <summary>
        /// Handle the request for a value from an output port
        /// </summary>
        public override object OnRequestValue(Port port)
        {
            // Read the input port values, defaulting to
            // the valued stored on this node instance if
            // there is no port connection.
            float af = GetInputValue("A", a);
            float bf = GetInputValue("B", b);

            // We only have one output port, so just return
            // the expected result. See FloatOperation for
            // an example of handling multiple outputs. 
            return af + bf;
        }
    }
}
