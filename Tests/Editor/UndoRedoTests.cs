using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace BlueGraph.EditorTests
{
    public class UndoRedoTests
    {
        [Test]
        public void CanUndoAddNode()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            var node1 = new TestNode();
            var node2 = new TestNode();
            
            graph.AddNode(node1);
            
            Undo.RegisterCompleteObjectUndo(graph, "Add Node 2");
            
            graph.AddNode(node2);

            Undo.PerformUndo();
            
            Assert.AreEqual(1, graph.nodes.Count);

            // Not the same instance anymore due to undo - but the same data.
            Assert.AreEqual(graph.nodes[0].id, node1.id);
        }
        
        [Test]
        public void CanUndoAddEdge()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            var node1 = new TestNode();
            var node2 = new TestNode();
            
            graph.AddNode(node1);
            graph.AddNode(node2);
            
            Undo.RegisterCompleteObjectUndo(graph, "Add Edge 1 -> 2");
            
            graph.AddEdge(
                node1.GetPort("Output"),
                node2.GetPort("Input")
            );
            
            Undo.PerformUndo();
            
            Assert.AreEqual(2, graph.nodes.Count);
            Assert.AreEqual(graph.nodes[0].id, node1.id);
            Assert.AreEqual(graph.nodes[1].id, node2.id);

            Assert.AreEqual(0, graph.nodes[0].GetPort("Output").Connections.Length);
            Assert.AreEqual(0, graph.nodes[1].GetPort("Input").Connections.Length);
        }

        /// <summary>
        /// Make sure an undo operation after adding a node/edge does not destroy
        /// unrelated connections and cleanly resets connections between nodes 
        /// to their previous state (i.e. no dangling edges)
        /// </summary>
        [Test] 
        public void UndoAddNodeDoesNotAffectUnrelatedConnections()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            var node1 = new TestNode();
            var node2 = new TestNode();
            var node3 = new TestNode();
            
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddEdge(
                node1.GetPort("Output"), 
                node2.GetPort("Input")
            );
            
            Undo.RegisterCompleteObjectUndo(graph, "Add Node 3 and Edge 2 -> 3");
            
            graph.AddNode(node3);
            
            graph.AddEdge(
                node2.GetPort("Output"),
                node3.GetPort("Input")
            );
            
            Undo.PerformUndo();
            
            // Make sure an undo operation did not destroy unrelated connections and
            // cleanly reset connections to their previous state (no dangling edges)
            var outputs = graph.nodes[0].GetPort("Output").Connections;
            var inputs = graph.nodes[1].GetPort("Input").Connections;
            
            Assert.AreEqual(2, graph.nodes.Count);
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(1, inputs.Length);
            
            Assert.AreSame(graph.nodes[0], inputs[0].node);
            Assert.AreSame(graph.nodes[1], outputs[0].node);
        }
    }
}
