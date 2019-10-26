
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Generic constant type. Attempts to generate an IL-optimized constant value.
    /// 
    /// Note that each constant node needs to be in its own file for Unity to
    /// find them through the ScriptableObject loader. :[
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConstantValue<T> : AbstractNode, ICanCompile
    {
        // TODO: Hide the input port somehow.
        [Input("")] public T value;
        [Output("")] readonly T m_Output;
        
        public override object GetOutputValue(string name) => value;
        
        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariableName(GetOutputPort(""));
            var constVal = builder.Constant(value);
            string type = builder.HoistNamespace(typeof(T));
            string constKeyword = constVal.isConstant ? "const " : "";

            // If we're declaring something that can be optimized for future operations, track it.
            if (constVal.isConstant) 
            {
                builder.AddConstToScope(varName);
            }

            builder.AppendLine($"{constKeyword}{type} {varName} = {constVal};");
        }
    }
}
