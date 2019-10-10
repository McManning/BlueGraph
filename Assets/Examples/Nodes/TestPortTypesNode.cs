using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    /// <summary>
    /// Node that just dumps every type available for CSS checking
    /// </summary>
    [Node(Name = "Test Port Types")]
    public class TestPortTypesNode : AbstractNode
    {
        // TODO: Inputs don't present an editable field inline
        // unless they're public. Again, Unity serialization stuff.
        [Input] public int int32Input;
        [Input] public long int64Input;

        [Input] public float floatInput;
        [Input(Name="V2 Input")] public Vector2 vector2Input;
        [Input(Name="V3 Input")] public Vector3 vector3Input;
        [Input(Name="V4 Input")] public Vector4 vector4Input;
    
        [Input] public Transform transformInput;
        [Input] public Matrix4x4 mat4x4Input;

        [Input] public KeyCode keyInput;

        [Input] public Color colorInput;
        [Input] public string stringInput;
        [Input] public bool boolInput;
        [Input] public Texture2D texture2DInput;

        [Input] public AnimationCurve animCurveInput = new AnimationCurve();
        [Input] public GameObject gameObjectInput;

    
        [Output] int int32Output;
        [Output] long int64Output;

        [Output] float floatOutput;
        [Output] Vector2 vector2Output;
        [Output] Vector3 vector3Output;
        [Output] Vector4 vector4Output;
    
        [Output] Transform transformOutput;
        [Output] Matrix4x4 mat4x4Output;
    
        [Output] KeyCode keyOutput;
        
        [Output] Color colorOutput;
        [Output] string stringOutput;
        [Output] bool boolOutput;
        [Output] Texture2D texture2DOutput;

        [Output] AnimationCurve animCurveOutput;
        [Output] GameObject gameObjectOutput;
    }
}
