
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(module = "Math")]
    public class FloatOperation : AbstractNode
    {
        // Define two input ports, A and B
        [Input] public float a;
        [Input] public float b;
        
        // Define different output ports, one for each operation
        [Output("MIN")] readonly float min;
        [Output("MAX")] readonly float max;
        [Output("+")] readonly float sum;
        [Output("*")] readonly float multiply;
        
        /// <summary>
        /// Handle the request for a value from an output port
        /// </summary>
        public override object OnRequestValue(Port port)
        {
            float af = GetInputValue("A", a);
            float bf = GetInputValue("B", b);

            // Return a different result based on the requested port
            switch (port.name)
            {
                case "MIN": return Mathf.Min(af, bf);
                case "MAX": return Mathf.Max(af, bf);
                case "*": return af * bf;
                case "+": return af + bf;
                default: throw new Exception($"Invalid output {port.name}");
            }
        }
    }
}
