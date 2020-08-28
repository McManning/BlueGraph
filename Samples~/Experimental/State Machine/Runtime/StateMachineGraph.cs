
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Data edge from a State to a Transition node
    /// </summary>
    public class StateToTransition
    {
        // Metadata needed for this transition goes here
    }

    /// <summary>
    /// Data edge from a Transition to a State node
    /// </summary>
    public class TransitionToState
    {
        // Metadata needed for this transition goes here
    }

    [CreateAssetMenu(
        menuName = "BlueGraph Samples/State Machine Graph", 
        fileName = "New StateMachineGraph"
    )]
    [IncludeTags("State Machine")]
    public class StateMachineGraph : Graph
    {

    }
}
