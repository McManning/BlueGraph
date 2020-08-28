using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Transition checks that can happen at any time, regardless of current state.
    /// </summary>
    [Node]
    [Tags("State Machine")]
    public class AnyStateNode : Node
    {
        [Input("From", Multiple = true)]
        public TransitionToState fromTransition;

        [Output("To", Multiple = true)]
        public StateToTransition toTransition;

        public override object OnRequestValue(Port port) => toTransition;
    }
}
