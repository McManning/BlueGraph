
using UnityEngine;
using BlueGraph;
using System.Text;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Execution data that can pass through execution ports on nodes
    /// </summary>
    public class ExecData
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

            if (entryPoint == null)
            {
                Debug.LogError($"<b>[{name}]</b> No EntryPoint node found", this);
                return;
            }

            ExecData data = new ExecData(); 
            
            // Execute through the graph until we run out of nodes to execute
            ICanExec next = entryPoint;
            int sanityCheck = 0;
            while (next != null)
            {
                next = next.Execute(data);

                // Just in case :)
                sanityCheck++;
                if (sanityCheck > 2000)
                {
                    Debug.LogError("Potential infinite loop detected. Stopping early.", this);
                    break;
                }
            }
        }

        /// <summary>
        /// Use CodeBuilder to compile this graph into a fast runtime
        /// </summary>
        public void Compile()
        {
            if (entryPoint == null)
            {
                Debug.LogError($"<b>[{name}]</b> No EntryPoint node found", this);
                return;
            }

            if (entryPoint is ICanCompile node)
            {
                CodeBuilder builder = new CodeBuilder
                {
                    // TODO: Better naming convention
                    className = name.Replace(" ", string.Empty) + "AOT"
                };

                node.Compile(builder);
                
                // TODO: Save out an asset or something
                Debug.Log(builder);
            }
            else 
            {
                Debug.LogError(
                    $"<b>[{name}]</b> Entry point node `{entryPoint.name}` " +
                    $"must implement interface `ICanCompile`", 
                    this
                );
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
