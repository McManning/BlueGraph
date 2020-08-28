using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// A subgraph containing its own state machine
    /// </summary>
    [Node]
    [Tags("State Machine")]
    public class SubStateMachineNode : SubgraphNode<StateMachineGraph>
    {
        [Input("From", Multiple = true)]
        public TransitionToState fromTransition;

        [Output("To", Multiple = true)]
        public StateToTransition toTransition;

        public override object OnRequestValue(Port port) => toTransition;
    }
}
