
using UnityEngine;
using BlueGraph;
using System.Collections.Generic;

namespace BlueGraphSamples
{
    /// <summary>
    /// Accept a dynamic number of input floats and generate
    /// a single list containing the inputs as the output.
    /// 
    /// This is an example of dynamically editing IO of nodes
    /// from within the editor (see ToFloatArrayView) and then
    /// using that dynamic configuration from within OnRequestValue.
    /// </summary>
    [Node("To Array (float)", module = "Experimental")]
    public class ToFloatArray : AbstractNode
    {
        // Define a single initial input value. Additional input
        // ports will be dynamically added by the custom view. 
        [Input("Arr[0]", editable = false)] public float arr;

        [Output] readonly List<float> result = new List<float>();
            
        /// <summary>
        /// Composite input port values into a single list
        /// </summary>
        public override object OnRequestValue(Port port)
        {   
            // One optimization you can do here is to cache 
            // the array of ports on deserialize and simply iterate
            // that array when constructing the output instead of
            // GetPort + string concatenation each execution.
            
            result.Clear();

            int i = 0;
            while ((port = GetPort($"Arr[{i}]")) != null)
            {
                result.Add(port.GetValue<float>());
                i++;
            }

            return result;
        }
    }
}
