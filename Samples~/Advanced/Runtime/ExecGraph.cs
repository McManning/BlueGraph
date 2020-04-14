using System;
using System.Collections.Generic;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Execution data that passes through execution ports on nodes
    /// </summary>
    public class ExecData
    {
        // Whatever you want can go here. This data 
        // will be passed through each executed node
    }
    
    /// <summary>
    /// Example of a graph that supports forward execution of nodes
    /// through a specialized "Execution edge", similar to UE4 Blueprints.
    /// 
    /// This technique allows us to create nodes that can have more
    /// complex flow control within a graph (branching, looping, etc)
    /// </summary>
    [CreateAssetMenu(menuName = "BlueGraph Samples/Advanced/Executable Graph", fileName = "New ExecGraph")]
    [IncludeModules("Math", "Logic", "Debug", "Flow Control", "Subgraph")]
    public class ExecGraph : Graph
    {
        /// <summary>
        /// Node that acts as the first point of execution. 
        /// </summary>
        EntryPoint entryPoint;
        
        public ExecGraph()
        {
            // Create a new EntryPoint node for all new graph instances
            entryPoint = new EntryPoint();
            AddNode(entryPoint);
        }

        /// <summary>
        /// Execute the graph, starting at the EntryPoint node.
        /// </summary>
        public void Execute()
        {
            // Load an entry point node if one isn't already set
            if (entryPoint == null)
            {
                entryPoint = FindNode<EntryPoint>();
            }

            if (entryPoint == null)
            {
                Debug.LogError($"<b>[{name}]</b> No EntryPoint node found", this);
                return;
            }

            ExecData data = new ExecData(); 
            // Fill in data here with whatever you want.
            
            ExecuteSubtree(entryPoint, data);
        }

        /// <summary>
        /// Execute the graph starting from the given parent node
        /// </summary>
        /// <param name="parent"></param>
        internal void ExecuteSubtree(ICanExec parent, ExecData data)
        {
            // Execute through the graph until we run out of nodes to execute
            ICanExec next = parent;
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
    }
}
