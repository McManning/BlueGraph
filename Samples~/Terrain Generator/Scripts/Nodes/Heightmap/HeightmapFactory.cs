
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{ 
    /// <summary>
    /// Base class for any operation that creates a new Heightmap.
    /// </summary>
    /// <remarks>
    /// Ensures the Result heightmap is either passed forward to connected
    /// nodes by reference for additional operations, or is copied in the
    /// case of multiple outputs, where each output needs a clean copy of
    /// the resulting Heightmap after the operation executes.
    /// </remarks>
    [Tags("Heightmap")]
    public abstract class HeightmapFactory : Node
    {
        [Output, NonSerialized] public Heightmap result;

        public Action<Heightmap> onUpdateResult;
        
        public abstract void Execute();

        public override object OnRequestValue(Port port)
        {
            if (result == null)
            {
                Execute();
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
