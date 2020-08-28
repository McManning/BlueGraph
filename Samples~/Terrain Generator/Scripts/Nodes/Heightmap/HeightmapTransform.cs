
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Base class for a node that takes an input heightmap `Map`,
    /// performs some transformation, and assigns to output port `Result`
    /// </summary>
    /// <remarks>
    /// Ensures the `Result` heightmap is either passed forward to connected
    /// nodes by reference for additional operations, or is copied in the
    /// case of multiple outputs, where each output needs a clean copy of
    /// the resulting Heightmap after the operation executes.
    /// </remarks>
    [Tags("Heightmap")]
    public abstract class HeightmapTransform : Node
    {
        [Input] public Heightmap map;        
        [Output, NonSerialized] public Heightmap result;

        public Action<Heightmap> onUpdateResult;

        /// <summary>
        /// Perform the transform on `result`.
        /// </summary>
        public abstract void Execute();

        public override object OnRequestValue(Port port)
        {
            if (result == null)
            {
                // Grab an updated input Heightmap, cache, and then run the
                // transform method on it and notify listeners once complete.
                map = GetInputValue<Heightmap>("map");
                if (map != null)
                {
                    result = map.Copy();
                    Execute();
                }

                onUpdateResult?.Invoke(result);
            }
            
            return result;
        }
        
        public void DirtyResult()
        {
            result = null;
        }
    }
}
