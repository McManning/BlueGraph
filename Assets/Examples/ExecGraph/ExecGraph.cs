
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Execution data that can pass through execution ports on nodes
    /// </summary>
    public struct ExecData
    {
        // Whatever can go here.
    }

    /// <summary>
    /// Example of a graph that supports forward exec of nodes, similar to UE4. 
    /// 
    /// This technique allows us to create nodes that can have more
    /// complex flow control within a graph (branching, looping, etc)
    /// </summary>
    [CreateAssetMenu(menuName = "Example ExecGraph", fileName = "New ExecGraph")]
    [IncludeModules("Math", "Unity", "Test", "ExecGraph")]
    public class ExecGraph : Graph
    {
        /// <summary>
        /// Node that acts as the first point of execution. 
        /// </summary>
        public EntryPoint entryPoint;
        
        public void Execute()
        {
            // iterate nodes
            Debug.Log("Execute!");

            if (!entryPoint)
            {
                Debug.LogError($"<b>[{name}]</b> No EntryPoint node found");
            }

            ExecData data = new ExecData(); 
            
            // Execute through the graph until we run out of nodes to execute
            ExecNode next = entryPoint;
            int sanityCheck = 0;
            while (next != null)
            {
                next = next.Execute(data);

                // Just in case :)
                sanityCheck++;
                if (sanityCheck > 2000)
                {
                    Debug.LogError("Potential infinite loop detected. Stopping early.");
                    break;
                }
            }
        }

        public override void AddNode(AbstractNode node)
        {
            base.AddNode(node);
            
            // Track the last (hopefully only...) entry point node.
            // TODO: Make sure it's actually the *only*
            if (node is EntryPoint entry)
            {
                Debug.Log("Added entry point");
                entryPoint = entry;
            }
        }
    }
}
