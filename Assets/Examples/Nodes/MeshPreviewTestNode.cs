using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Mesh Preview")]
    public class MeshPreviewTestNode : AbstractNode
    {
        [Input] public Mesh mesh;
        [Input] public Material material;
    }
}
