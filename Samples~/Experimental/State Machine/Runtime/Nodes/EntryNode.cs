using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Entry into the state machine
    /// </summary>
    [Node(Deletable = false)]
    [Tags("Hidden")]
    public class EntryNode : Node
    {
        [Output("To", Multiple = true)]
        public StateToTransition toTransition;

        public override object OnRequestValue(Port port) => toTransition;
    }
}
