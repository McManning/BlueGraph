using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BlueGraph.Tests
{
    public class SerializationTests
    {
        /// <summary>
        /// Tests to check for proper polymorphic node serialization through 
        /// Unity's [SerializeReference] attribute by instantiating SOs
        /// 
        /// And tests to check that serialization performed during undo/redo
        /// events revert the graph to expected states. 
        /// </summary>
        [Test]
        public void CanCloneWithInstantiation()
        {
            var original = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new EmptyNode();
            var node2 = new EmptyNode();

            node1.AddPort(new Port { Type = typeof(float), name = "Output" });
            node2.AddPort(new Port { Type = typeof(float), name = "Input", isInput = true });
            
            original.AddNode(node1);
            original.AddNode(node2);
            original.AddEdge(
                node1.GetPort("Output"), 
                node2.GetPort("Input")
            );
            

            // ---- Clone via Instantiate ----

            var clone = Object.Instantiate(original);
            

            // ---- Check Integrity ----
            
            var cloneNode1 = clone.FindNodeById(node1.id);
            var cloneNode2 = clone.FindNodeById(node2.id);

            Assert.AreEqual(2, clone.nodes.Count);
            
            // Check class deserialization
            Assert.IsInstanceOf<EmptyNode>(clone.nodes[0]);
            Assert.IsInstanceOf<EmptyNode>(clone.nodes[1]);

            Assert.AreNotSame(cloneNode1, node1);
            Assert.AreEqual(node1.id, cloneNode1.id);
            
            Assert.AreNotSame(cloneNode2, node2);
            Assert.AreEqual(node2.id, cloneNode2.id);
            
            // Check port deserialization
            Assert.IsInstanceOf<Port>(cloneNode1.GetPort("Output"));
            Assert.IsInstanceOf<Port>(cloneNode2.GetPort("Input"));
            
            // Check connections
            var outputsFromNode1 = cloneNode1.GetPort("Output").Connections;
            var inputsToNode2 = cloneNode2.GetPort("Input").Connections;

            Assert.AreEqual(1, outputsFromNode1.Count);
            Assert.AreEqual(1, inputsToNode2.Count);
            
            Assert.AreSame(cloneNode2, outputsFromNode1[0].node);
            Assert.AreSame(cloneNode1, inputsToNode2[0].node);
        }
        
        /// <summary>
        /// Test to check for proper polymorphic node serialization through 
        /// Unity's [SerializeReference] attribute through JSONUtility
        /// </summary>
        [Test]
        public void CanJsonSerializePolymorphicNodes()
        {
            var original = ScriptableObject.CreateInstance<Graph>();
            
            var node1 = new EmptyNode();
            var node2 = new EmptyNode();

            node1.AddPort(new Port { Type = typeof(float), name = "Output" });
            node2.AddPort(new Port { Type = typeof(float), name = "Input", isInput = true });
            
            original.AddNode(node1);
            original.AddNode(node2);
            original.AddEdge(
                node1.GetPort("Output"), 
                node2.GetPort("Input")
            );
            
            // ---- Clone via JsonUtility ----

            var json =JsonUtility.ToJson(original, true);

            var clone = ScriptableObject.CreateInstance<Graph>();
            JsonUtility.FromJsonOverwrite(json, clone);


            // ---- Check Integrity ----
            
            var cloneNode1 = clone.FindNodeById(node1.id);
            var cloneNode2 = clone.FindNodeById(node2.id);

            Assert.AreEqual(2, clone.nodes.Count);
            
            // Check class deserialization
            Assert.IsInstanceOf<EmptyNode>(clone.nodes[0]);
            Assert.IsInstanceOf<EmptyNode>(clone.nodes[1]);

            Assert.AreNotSame(cloneNode1, node1);
            Assert.AreEqual(node1.id, cloneNode1.id);
            
            Assert.AreNotSame(cloneNode2, node2);
            Assert.AreEqual(node2.id, cloneNode2.id);
            
            // Check port deserialization
            Assert.IsInstanceOf<Port>(cloneNode1.GetPort("Output"));
            Assert.IsInstanceOf<Port>(cloneNode2.GetPort("Input"));
            
            // Check connections
            var outputsFromNode1 = cloneNode1.GetPort("Output").Connections;
            var inputsToNode2 = cloneNode2.GetPort("Input").Connections;

            Assert.AreEqual(1, outputsFromNode1.Count);
            Assert.AreEqual(1, inputsToNode2.Count);
            
            // These are pointing to node1 and node2 because
            // the graph reference stored and cloned still points
            // to the initial asset instance. 
            Assert.AreSame(cloneNode2, outputsFromNode1[0].node);
            Assert.AreSame(cloneNode1, inputsToNode2[0].node);
        }
    }
}
