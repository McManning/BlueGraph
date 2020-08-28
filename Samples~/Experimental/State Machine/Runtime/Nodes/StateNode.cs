using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// A discrete state within the state machine
    /// </summary>
    [Node]
    [Tags("State Machine")]
    public class StateNode : Node
    {
        [Input("From", Multiple = true)]
        public TransitionToState fromTransition;

        [Output("To", Multiple = true)]
        public StateToTransition toTransition;

        public override object OnRequestValue(Port port) => toTransition;
    }
}


