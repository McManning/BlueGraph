using UnityEngine;
using BlueGraph;
using System.Collections.Generic;

namespace BlueGraphSamples
{
    /// <summary>
    /// Example in breaking an input Vec3 into an input per field.
    /// 
    /// This is an example of dynamically editing IO of nodes
    /// from within the editor (see BreakStructNodeView) and then
    /// using that dynamic configuration from within OnRequestValue.
    /// </summary>
    [Node("Break Struct Test", Path = "Experimental")]
    public class BreakStruct : Node
    {
        // By default, the node has a single Vec3 input.
        // But this could be removed from within the editor
        // and replaced with x, y, z inputs.
        [Input("Vec3")] public Vector3 vec;

        // Cache locations for the alternative representation.
        // Unfortunately, has to be on the node directly to support
        // inline editing. For now.
        protected float x;
        protected float y;
        protected float z;

        [Output] protected float length;
            
        /// <summary>
        /// Composite input port values into a single list
        /// </summary>
        public override object OnRequestValue(Port port)
        {
            // Check if the Vec3 port still exists on this node.
            // If not, it was broken in the editor and we want
            // to instead read independent x, y, z inputs
            Vector3 v;
            var input = GetPort("Vec3");
            if (input != null)
            {
                v = port.GetValue(vec);
            }
            else
            {
                v = new Vector3(
                    GetInputValue("x", x),
                    GetInputValue("y", y),
                    GetInputValue("z", z)
                );
            }

            return v.magnitude;
        }
    }
}
