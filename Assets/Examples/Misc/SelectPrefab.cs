using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Select Prefab", module = "Unity/GameObject", help = "Return a prefab at random from the pool")]
    public class SelectPrefab : AbstractNode
    {
        [Input] public int seed;
        [Editable] public GameObject[] prefabs;

        [Output] GameObject selected;

        public override object GetOutputValue(string name)
        {
            if (prefabs.Length < 1)
            {
                return null;
            }

            // TODO: Where does seed make sense here? You wouldn't initialize each time,
            // otherwise it's not random. 
            // Random.InitState(seed);
            return prefabs[Random.Range(0, prefabs.Length)];
        }
    }
}
