
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Non-template base class used as a hook for the custom node view
    /// </summary>
    public class ConstantValue : AbstractNode { }

    /// <summary>
    /// Generic constant type. Attempts to generate an IL-optimized constant value.
    /// 
    /// Note that each constant node needs to be in its own file for Unity to
    /// find them through the ScriptableObject loader. :[
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConstantValueImpl<T> : ConstantValue, ICanCompile
    {
        [Output("")] public T value;
        
        public override object GetOutputValue(string name) => value;
        
        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariableName(GetOutputPort(""));
            var constVal = builder.GetAssignable(value);
            string type = builder.HoistNamespace(typeof(T));
            string constKeyword = constVal.isConst ? "const " : "";

            // If we're declaring something that can be optimized for future operations, track it.
            if (constVal.isConst) 
            {
                builder.AddConstToScope(varName);
            }

            builder.AppendLine($"{constKeyword}{type} {varName} = {constVal};");
        }
    }
}
