using System;
using System.Collections.Generic;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Demo of different ways to configure ports
    /// </summary>
    [Node]
    [Tags("Basic")]
    public class ExamplePortAttributes : Node
    {
        // Public inputs are editable constants in the graph editor
        [Input] public float a = 1.0f;

        // Non-public inputs are not editable, and are expected to have connections.
        [Input] protected float b;

        // You can specify a custom name to override the default
        [Input("C In")] protected float c;
    
        // An input port that can accept multiple output edges into itself. 
        // For these, use GetInputValues<T> to return an iterator of values.
        [Input(Name = "More Floats", Multiple = true)] protected float d;
    
        // Outputs still need to be assigned to a field, even if we don't use that field. 
        // In more advanced use cases, you can potentially use the field for memoization.
        [Output("Sum")] protected float sum;
        [Output("Multiply")] protected float multiply;
        
        public override object OnRequestValue(Port port)
        {
            // Get connection value from connection a. 
            // If a isn't connected, this defaults to the 
            // value stored in this node instance.
            float fa = GetInputValue("a", a);
            float fb = GetInputValue("b", b);

            // If you specified a custom name for [Input], use that name for GetInputValue.
            float fc = GetInputValue("C In", c); 
        
            // If a port supports multiple connections, you can 
            // access an iterator of values. 
            IEnumerable<float> moreFloats = GetInputValues<float>("More Floats");

            // "Sum" output port
            if (port.Name == "Sum")
            {
                float sum = fa + fb + fc;
                foreach (float f in moreFloats)
                {
                    sum += f;
                }

                return sum;
            }
            else if (port.Name == "Multiply")
            {
                float multiplied = fa + fb + fc;
                foreach (float f in moreFloats)
                {
                    multiplied *= f;
                }

                return multiplied;
            }

            throw new Exception($"Unhandled output port {port.Name}");
        }
    }
}
