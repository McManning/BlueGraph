
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node(module = "ExecGraph/GameObject")]
    public class Instantiate : ExecNode
    {
        [Input] public GameObject prefab;
        [Input] public Vector3 localPosition;
        [Input] public Quaternion localRotation;
        [Input] public GameObject parent;

        [Output] GameObject instance;

        public override ExecNode Execute(ExecData data)
        {
            GameObject go = GetInputValue<GameObject>("Prefab", null);
            GameObject pgo = GetInputValue<GameObject>("Parent", null);
            Vector3 position = GetInputValue("Local Position", localPosition);
            Quaternion rotation = GetInputValue("Local Rotation", localRotation);

            if (go)
            {
                instance = Instantiate(go, position, rotation, pgo?.transform);
            }
            
            return base.Execute(data);
        }
    }
}
