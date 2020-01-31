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
            original.AddNode(new TestNodeA());
            original.AddNode(new TestNodeB());

            var clone = Object.Instantiate(original);

            Assert.AreEqual(2, clone.nodes.Count);
            
            // Check class deserialization
            Assert.IsInstanceOf<TestNodeA>(clone.nodes[0]);
            Assert.IsInstanceOf<TestNodeB>(clone.nodes[1]);

            Assert.AreNotSame(original.nodes[0], clone.nodes[0]);
            Assert.AreNotSame(original.nodes[1], clone.nodes[1]);

            // Check port deserialization
            Assert.AreEqual("Input", clone.nodes[0].ports[0].name);
            Assert.AreEqual("Output", clone.nodes[0].ports[1].name);
            
            Assert.AreEqual("Input", clone.nodes[1].ports[0].name);
            Assert.AreEqual("Output", clone.nodes[1].ports[1].name);
        }
        
        /// <summary>
        /// Test to check for proper polymorphic node serialization through 
        /// Unity's [SerializeReference] attribute through JSONUtility
        /// </summary>
        [Test]
        public void CanJsonSerializePolymorphicNodes()
        {
            var original = ScriptableObject.CreateInstance<Graph>();
            original.AddNode(new TestNodeA());
            original.AddNode(new TestNodeB());

            var json =JsonUtility.ToJson(original, true);

            var clone = ScriptableObject.CreateInstance<Graph>();
            JsonUtility.FromJsonOverwrite(json, clone);
            
            Assert.AreEqual(2, clone.nodes.Count);
            
            // Check class deserialization
            Assert.IsInstanceOf<TestNodeA>(clone.nodes[0]);
            Assert.IsInstanceOf<TestNodeB>(clone.nodes[1]);

            Assert.AreNotSame(original.nodes[0], clone.nodes[0]);
            Assert.AreEqual(original.nodes[0].id, clone.nodes[0].id);

            Assert.AreNotSame(original.nodes[1], clone.nodes[1]);
            Assert.AreEqual(original.nodes[1].id, clone.nodes[1].id);

            // Check port deserialization
            Assert.AreEqual("Input", clone.nodes[0].ports[0].name);
            Assert.AreEqual("Output", clone.nodes[0].ports[1].name);
            
            Assert.AreEqual("Input", clone.nodes[1].ports[0].name);
            Assert.AreEqual("Output", clone.nodes[1].ports[1].name);
        }
    }
}
