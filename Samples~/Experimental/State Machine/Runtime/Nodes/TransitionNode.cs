using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node]
    [Tags("State Machine")]
    public class TransitionNode : Node
    {
        [Input("From", Multiple = true)] 
        public StateToTransition fromState;

        [Output("To", Multiple = true)] 
        public TransitionToState toState;

        [Input("Condition")] 
        public bool condition;

        public override object OnRequestValue(Port port) => toState;
    }
}
