
using UnityEngine;
using BlueGraph;
using System;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (int)", module = "ExecGraph/Constant")]
    public class ConstantInt : AbstractNode, ICanCompile
    {
        [Input("")] public int value;
        [Output("")] readonly int m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
        
        public void Compile(CodeBuilder builder)
        {
            // TODO: Try to inline constants instead of assigning to variables first
            // if it even matters (would const keyword just remove the variable at compile time?)
            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);

            builder.AppendLine($"const int {varName} = {constantValue};");
        }
    }

    [Node("Constant (float)", module = "ExecGraph/Constant")]
    public class ConstantFloat : AbstractNode, ICanCompile
    {
        [Input("")] public float value;
        [Output("")] readonly float m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
        
        public void Compile(CodeBuilder builder)
        {
            Debug.Log("c..constant compile?");

            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);

            builder.AppendLine($"const float {varName} = {constantValue};");
        }
    }
    
    [Node("Constant (String)", module = "ExecGraph/Constant")]
    public class ConstantString : AbstractNode, ICanCompile
    {
        [Input("")] public string value;
        [Output("")] readonly string m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);

        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);

            builder.AppendLine($"const string {varName} = {constantValue};");
        }
    }

    [Node("Constant (Vector2)", module = "ExecGraph/Constant")]
    public class ConstantVector2 : AbstractNode, ICanCompile
    {
        [Input("")] public Vector2 value;
        [Output("")] readonly Vector2 m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
        
        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);
            builder.AppendLine($"Vector2 {varName} = {constantValue};");
        }
    }
    
    [Node("Constant (Vector3)", module = "ExecGraph/Constant")]
    public class ConstantVector3 : AbstractNode, ICanCompile
    {
        [Input("")] public Vector3 value;
        [Output("")] readonly Vector3 m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
        
        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);
            builder.AppendLine($"Vector3 {varName} = {constantValue};");
        }
    }
    
    [Node("Constant (Vector4)", module = "ExecGraph/Constant")]
    public class ConstantVector4 : AbstractNode, ICanCompile
    {
        [Input("")] public Vector4 value;
        [Output("")] readonly Vector4 m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
        
        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);
            builder.AppendLine($"Vector4 {varName} = {constantValue};");
        }
    }
    
    [Node("Constant (Color)", module = "ExecGraph/Constant")]
    public class ConstantColor : AbstractNode, ICanCompile
    {
        [Input("")] public Color value;
        [Output("")] readonly Color m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
        
        public void Compile(CodeBuilder builder)
        {
            string varName = builder.PortToVariable(GetInputPort(""));
            string constantValue = builder.Constant(value);
            builder.AppendLine($"Color {varName} = {constantValue};");
        }
    }
}
