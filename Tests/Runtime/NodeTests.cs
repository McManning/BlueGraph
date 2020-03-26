using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BlueGraph.Tests
{
    /// <summary>
    /// Test suite that focuses on AbstractNode methods
    /// </summary>
    public class NodeTests
    {
        [Test]
        public void CanAddPorts()
        {
            var node = new TestNodeA(); 
            var port1 = new OutputPort<float> { name = "Test 1" };
            var port2 = new OutputPort<float> { name = "Test 2" };

            node.AddPort(port1);
            node.AddPort(port2);
            
            // Test Node A comes with 2 ports by default
            Assert.AreEqual(4, node.Ports.Count);
            Assert.AreSame(port1, node.GetPort("Test 1"));
            Assert.AreSame(port2, node.GetPort("Test 2"));
        }
        
        [Test]
        public void CanRemovePorts()
        {
            var node = new TestNodeA();
            var port1 = new OutputPort<float> { name = "Test 1" };
            var port2 = new OutputPort<float> { name = "Test 2" };

            node.AddPort(port1);
            node.AddPort(port2);

            node.RemovePort(port1);
            
            // Test Node A comes with 2 ports by default
            Assert.AreEqual(3, node.Ports.Count);
            Assert.AreSame(port2, node.GetPort("Test 2"));
        }
        
        /// <summary>
        /// Ensure that calling RemovePort() will also remove edges to that port
        /// </summary>
        [Test]
        public void RemovingPortsAlsoRemovesEdges()
        {
            var graph = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new TestNodeA();
            var node2 = new TestNodeA();
            var node3 = new TestNodeA();
            
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);

            var portToRemove = node2.GetPort("Input");

            // Edge that should be deleted
            graph.AddEdge(
                node1.GetPort("Output"),
                node2.GetPort("Input")
            );
            
            // Unaffected edge
            graph.AddEdge(
                node2.GetPort("Output"), 
                node3.GetPort("Input")
            );
            
            node2.RemovePort(portToRemove);

            Assert.AreEqual(0, node1.GetPort("Output").Connections.Count);
            Assert.AreEqual(1, node2.GetPort("Output").Connections.Count);
            Assert.AreEqual(1, node3.GetPort("Input").Connections.Count);
        }
        
        [Test]
        public void CanGetPorts()
        {
            var node = new TestNodeA(); 
            node.AddPort(new OutputPort<float> { name = "Test 1" });
            node.AddPort(new OutputPort<float> { name = "Test 2" });
            
            var actual = node.GetPort("Test 2");
            
            Assert.AreSame(node, actual.node);
            Assert.AreSame("Test 2", actual.name);
        }
        
        [Test]
        public void AddDuplicatePortNameThrowsError()
        {
            var node = new TestNodeA();
            node.AddPort(new OutputPort<float> { name = "Test" });
            
            Assert.Throws<ArgumentException>(
                () => node.AddPort(new OutputPort<float> { name = "Test" })
            );
        }
        
        [Test]
        public void ReturnsNullOnInvalidPortName()
        {
            var node = new TestNodeA();
            
            var actual = node.GetPort("Bad Port");

            Assert.IsNull(actual);
        }
    }
}
