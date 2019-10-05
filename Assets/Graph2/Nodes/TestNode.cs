using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
{
    public class TestNode : AbstractNode
    {
        [Input] public float x;
        [Input] public float y;

        [Output] public float result;

        [Editable] public float fooBarBuzz;

        public override object GetOutput(string name)
        {
            return x + y;
        }
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
}
