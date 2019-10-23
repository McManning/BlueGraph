
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node(module = "ExecGraph")]
    public class Print : ExecNode
    {
        [Input] public string value;
        [Input] public object obj;

        public override ExecNode Execute(ExecData data)
        {
            string value = GetInputValue("Value", this.value);
            object obj = GetInputValue("Obj", this.obj);
            Debug.Log($"{value}: {obj}");

            return base.Execute(data);
        }
    }
}
