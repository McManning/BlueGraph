
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Math/Operation (float)", Tooltip = "Perform some operation on float values")]
    public class FloatOperationNode : AbstractNode
    {
        public enum Operation
        {
            Add,
            Subtract,
            Multipy,
            Divide,
            Min,
            Max
        }

        [Input] public float x;
        [Input] public float y;

        [Output("")] public float result;

        [Editable] public Operation operation;

        [Editable, TextArea(5, 3)] public string comments;

        public override object GetOutput(string name)
        {
            float x = GetInputValue("x", this.x);
            float y = GetInputValue("y", this.y);

            switch (operation)
            {
                case Operation.Add:
                    return x + y;
                case Operation.Multipy:
                    return x * y;
                case Operation.Subtract:
                    return x - y;
                case Operation.Divide:
                    return x / y;
                case Operation.Min:
                    return Mathf.Min(x, y);
                case Operation.Max:
                    return Mathf.Max(x, y);
                default:
                    return 0f;
            }
        }
    }
}
