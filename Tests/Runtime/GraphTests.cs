using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace BlueGraph.Tests
{
    /// <summary>
    /// Tests for manipulating nodes and edges on a graph 
    /// </summary>
    public class GraphTests
    {
        [Test]
        public void CanAddNodes()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            graph.AddNode(new TestNodeA());
            graph.AddNode(new TestNodeA());
            
            Assert.AreEqual(2, graph.nodes.Count);
        }
        
        [Test]
        public void CanFindNodeById()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var node2 = new TestNodeA();
            var expected = new TestNodeA();
            var node3 = new TestNodeA();

            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(expected);
            graph.AddNode(node3);
            
            var actual = graph.FindNodeById(expected.id);
            
            Assert.AreSame(expected, actual);
        }
        
        [Test]
        public void CanFindNodeByType()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var expected = new TestNodeB();
            var node2 = new TestNodeB();

            graph.AddNode(node1);
            graph.AddNode(expected);
            graph.AddNode(node2);
            
            var actual = graph.FindNode<TestNodeB>();

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void CanFindNodeAssignableFromType()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeB();
            var expected = new InheritedTestNodeA();

            graph.AddNode(node1);
            graph.AddNode(expected);
            
            var actual = graph.FindNode<TestNodeA>();

            Assert.AreSame(expected, actual);
        }
        
        [Test]
        public void CanFindMultipleNodesByType()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            graph.AddNode(new TestNodeA());
            graph.AddNode(new TestNodeB());
            graph.AddNode(new TestNodeA());
            graph.AddNode(new TestNodeB());
            
            List<TestNodeA> actual = graph.FindNodes<TestNodeA>();

            Assert.AreEqual(2, actual.Count);

            Assert.IsInstanceOf<TestNodeA>(actual[0]);
            Assert.IsInstanceOf<TestNodeA>(actual[1]);
        }

        [Test]
        public void ReturnsNullOnInvalidNodeId()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var actual = graph.FindNodeById("BAD ID");

            Assert.IsNull(actual);
        }
        
        [Test]
        public void CanAddEdges()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var node2 = new TestNodeA();
            
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddEdge(
                node1.GetPort("Output"), 
                node2.GetPort("Input")
            );
            
            var outputsFromNode1 = node1.GetPort("Output").Connections;
            var inputsToNode2 = node2.GetPort("Input").Connections;

            Assert.AreEqual(1, outputsFromNode1.Count);
            Assert.AreEqual(1, inputsToNode2.Count);
            
            Assert.AreSame(node2, outputsFromNode1[0].node);
            Assert.AreSame(node1, inputsToNode2[0].node);
        }

        [Test]
        public void CanRemoveNode()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var nodeToRemove = new TestNodeA();
            var node2 = new TestNodeA();
            
            graph.AddNode(node1);
            graph.AddNode(nodeToRemove);
            graph.AddNode(node2);
            
            graph.RemoveNode(nodeToRemove);
            
            Assert.AreEqual(2, graph.nodes.Count);
            Assert.IsNull(graph.FindNodeById(nodeToRemove.id));
        }
        
        // [Test]
        public void OnRemovedFromGraphExecutes()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var nodeToRemove = new TestNodeA();
            
            // TODO: No mock support. How do I test for this?

            graph.AddNode(nodeToRemove);
            graph.RemoveNode(nodeToRemove);
        }

        /// <summary>
        /// Ensure that edges to a removed node are also removed
        /// at the same time.
        /// </summary>
        [Test]
        public void RemovingNodeAlsoRemovesEdges()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var nodeToRemove = new TestNodeA();
            var node2 = new TestNodeA();
            
            graph.AddNode(node1);
            graph.AddNode(nodeToRemove);
            graph.AddNode(node2);

            graph.AddEdge(
                node1.GetPort("Output"),
                nodeToRemove.GetPort("Input")
            );
            
            graph.AddEdge(
                node2.GetPort("Output"), 
                nodeToRemove.GetPort("Input")
            );
            
            graph.RemoveNode(nodeToRemove);

            Assert.AreEqual(0, node1.GetPort("Output").Connections.Count);
            Assert.AreEqual(0, node2.GetPort("Output").Connections.Count);
            
            Assert.AreEqual(0, nodeToRemove.GetPort("Input").Connections.Count);
        }

        [Test]
        public void CanRemoveEdge()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var node2 = new TestNodeA();
            var node3 = new TestNodeA();
            
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);

            graph.AddEdge(
                node1.GetPort("Output"), 
                node2.GetPort("Input")
            );
            
            graph.AddEdge(
                node1.GetPort("Output"), 
                node3.GetPort("Input")
            );
            
            graph.RemoveEdge(
                node1.GetPort("Output"), 
                node3.GetPort("Input")
            );

            Assert.AreEqual(1, node1.GetPort("Output").Connections.Count);
            Assert.AreEqual(0, node3.GetPort("Input").Connections.Count);
        }
    }
}
