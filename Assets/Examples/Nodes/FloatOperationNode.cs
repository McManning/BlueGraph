using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Math/Operation (float)")]
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

        public override object GetOutput(string name)
        {
            float[] x = GetInputValues("x", this.x);
            float y = GetInputValue("y", this.y);

            return 0f;
        }
    }
}
