
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Generic math operation for primitive value types. 
    /// 
    /// These are all implemented as custom ICanCompile nodes in order to 
    /// generate code that could take advantage of C# compiler optimizations
    /// </summary>
    public abstract class Operation<T, R> : AbstractNode, ICanCompile
    {
        [Input] public T a;
        [Input] public T b;
        [Output("")] readonly R result;
        
        public abstract R OutputOperation(T a, T b);
        public abstract string CompileOperation(string a, string b);

        public override object GetOutputValue(string name)
        {
            T a = GetInputValue("A", this.a);
            T b = GetInputValue("B", this.b);
            return OutputOperation(a, b);
        }
        
        public void Compile(CodeBuilder builder)
        {
            var aVar = builder.PortToValue(GetInputPort("A"), a);
            var bVar = builder.PortToValue(GetInputPort("B"), b);

            string varName = builder.PortToVariableName(GetOutputPort(""));
            string constKeyword = (aVar.isConst && bVar.isConst) ? "const " : "";
            string returnType = builder.HoistNamespace(typeof(R));
            string operation = CompileOperation(aVar.value, bVar.value);

            // If the operation executes on two constants, also make the operation constant.
            if (aVar.isConst && bVar.isConst)
            {
                builder.AddConstToScope(varName);
            }

            builder.AppendLine($"{constKeyword}{returnType} {varName} = {operation};");
        }
    }
}
