using System;
using UnityEngine;

namespace Graph2
{
    [Node("Mesh Preview")]
    public class MeshPreviewTestNode : AbstractNode
    {
        [Input] public Mesh mesh;
        [Input] public Material material;
    }
}
