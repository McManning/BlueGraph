using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node("Mesh Preview", module = "Experimental")]
    public class MeshPreview : AbstractNode
    {
        [Input, Output("")] public Mesh mesh;
        [Input] public Material material;
        
        public override object OnRequestValue(Port port) => mesh;
    }
}
