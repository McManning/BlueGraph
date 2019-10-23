
using UnityEngine;
using BlueGraph;
using System;

namespace BlueGraphExamples.Math
{
    [Node("Constant (int)", module = "Math")]
    public class ConstantInt : AbstractNode
    {
        [Input("")] public int value;
        [Output("")] readonly int m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
    }

    [Node("Constant (float)", module = "Math")]
    public class ConstantFloat : AbstractNode
    {
        [Input("")] public float value;
        [Output("")] readonly float m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
    }
    
    [Node("Constant (Vector2)", module = "Math")]
    public class ConstantVector2 : AbstractNode
    {
        [Input("")] public Vector2 value;
        [Output("")] readonly Vector2 m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
    }
    
    [Node("Constant (Vector3)", module = "Math")]
    public class ConstantVector3 : AbstractNode
    {
        [Input("")] public Vector3 value;
        [Output("")] readonly Vector3 m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
    }
    
    [Node("Constant (String)", module = "Math")]
    public class ConstantString : AbstractNode
    {
        [Input("")] public string value;
        [Output("")] readonly string m_Output;
        
        public override object GetOutputValue(string name) => GetInputValue("", value);
    }
}
