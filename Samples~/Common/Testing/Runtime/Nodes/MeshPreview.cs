using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Node that displays a mesh in a scene view within the node.
    /// 
    /// For the bulk of the implementation, see <c>Editor/MeshPreviewNodeView</c>
    /// </summary>
    [Node("Mesh Preview", Path = "Experimental")]
    public class MeshPreview : Node
    {
        [Input, Output("")] public Mesh mesh;
        [Input] public Material material;
        
        public override object OnRequestValue(Port port) => mesh;
    }
}
