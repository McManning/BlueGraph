using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Exit a state machine (or a subgraph of a state machine)
    /// </summary>
    [Node]
    [Tags("State Machine")]
    public class ExitNode : Node
    {
        [Input("From", Multiple = true)]
        public TransitionToState fromTransition;

        public override object OnRequestValue(Port port) => null;
    }
}
