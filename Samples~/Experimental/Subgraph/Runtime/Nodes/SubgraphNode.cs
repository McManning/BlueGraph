using System;
using UnityEngine;
using BlueGraph;
using UnityEditor;

namespace BlueGraphSamples
{
    /// <summary>
    /// Concrete instance so that CustomNodeView can work for inherited classes
    /// </summary>
    public abstract class BaseSubgraphNode : Node { 
        public abstract Graph GetSubgraph();
    }

    /// <summary>
    /// Abstract class for any node that contains a subgraph. 
    /// 
    /// Inherit from this to make use of SubgraphNodeView
    /// </summary>
    public abstract class SubgraphNode<T> : BaseSubgraphNode where T : Graph
    {
        [SerializeField] protected T subgraph;
        
        /// <summary>
        /// Get the subgraph associated with this node, creating one if it does not exist.
        /// </summary>
        /// <returns></returns>
        public override Graph GetSubgraph()
        {
            // TODO: I want the retval to be `T` but I need
            // to declare the method in BaseSubgraphNode as a non-generic
            // in order to support a fallback SubgraphNodeView. 

            if (!subgraph)
            {
                subgraph = ScriptableObject.CreateInstance<T>();
                subgraph.name = ID;
                
                #if UNITY_EDITOR 
                    // We need to add the graph as a sub-asset, otherwise 
                    // it won't be persisted outside of the editor. 
                    AssetDatabase.AddObjectToAsset(
                        subgraph, 
                        AssetDatabase.GetAssetPath(Graph.GetInstanceID())
                    );
                #endif
            }

            return subgraph;
        }
    }
}
