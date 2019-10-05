
using System;
using UnityEngine;

// Various test samples

[GraphNode("Group1/Group2/Test")]
public class TestNode : AbstractNode
{
    [Input(NodeInputConstraint.Any)] public float x;
    [Input] public float y;
    
    [Output] float z;

    public float publicSerializable;

    [SerializeField] float privateSerializable;

    public TestNode()
    {
        publicSerializable = 1.5f;
        privateSerializable = 0.42f;
        Debug.Log("Hi from a test node instance ");
    }
}

[GraphNode("Int Math")]
public class IntMathNode : AbstractNode
{
    [Input] public int a;
    [Input] public int b;
    [Output] public int result;
}

/// <summary>
/// Node that just dumps every type available for CSS checking
/// </summary>
[GraphNode("Type Test")]
public class TypeTestNode : AbstractNode
{
    [Input] int int32Input;
    [Input] long int64Input;

    [Input] float floatInput;
    [Input] Vector2 vector2Input;
    [Input] Vector3 vector3Input;
    [Input] Vector4 vector4Input;
    
    [Input] Matrix4x4 mat4x4Input;

    [Input] KeyCode keyInput;

    [Input] Color colorInput;
    [Input] string stringInput;
    [Input] bool boolInput;
    [Input] Texture2D texture2DInput;

    [Input] AnimationCurve animCurveInput = new AnimationCurve();
    [Input] GameObject gameObjectInput;

    
    [Output] int int32Output;
    [Output] long int64Output;

    [Output] float floatOutput;
    [Output] Vector2 vector2Output;
    [Output] Vector3 vector3Output;
    [Output] Vector4 vector4Output;
    
    [Output] Matrix4x4 mat4x4Output;
    
    [Input] KeyCode keyOutput;

    [Output] Color colorOutput;
    [Output] string stringOutput;
    [Output] bool boolOutput;
    [Output] Texture2D texture2DOutput;

    [Output] AnimationCurve animCurveOutput;
    [Output] GameObject gameObjectOutput;
}
