using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
{
    [Node("Mesh Preview")]
    public class MeshPreviewTestNode : AbstractNode
    {
        [Input] public Mesh mesh;
        [Input] public Material material;
    }

    [Node("Math/Float Math")]
    public class MathNode : AbstractNode
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

        [Input(Editable = false)] public float x;
        [Input] public float y;

        [Output(Name = "")] public float result;

        [Editable] public Operation operation;

        public override object GetOutput(string name)
        {
            float[] x = GetInputValues("x", this.x);
            float y = GetInputValue("y", this.y);

            switch (operation) 
            { 
                
            }
        }
    }
    
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
    
        [Output] Matrix4x4 mat4x4Output;
    
        [Input] KeyCode keyOutput;

        [Output] Color colorOutput;
        [Output] string stringOutput;
        [Output] bool boolOutput;
        [Output] Texture2D texture2DOutput;

        [Output] AnimationCurve animCurveOutput;
        [Output] GameObject gameObjectOutput;
    }
    
    [Node(Name = "Test Field Types")]
    public class TestFieldTypesNode : AbstractNode
    {
        [Editable] public int int32Input;
        [Editable] public long int64Input;

        // Private fields need to be both serialized + editable
        [SerializeField]
        [Editable] 
        float floatInput;
        
        [Editable] public Vector2 vector2Input;
        [Editable] public Vector3 vector3Input;
        [Editable] public Vector4 vector4Input;
    
        [Editable] public Matrix4x4 mat4x4Input;

        [Editable] public KeyCode keyInput;

        [Editable] public Color colorInput;
        [Editable] public string stringInput;
        [Editable] public bool boolInput;
        [Editable] public Texture2D texture2DInput;

        [Editable] public AnimationCurve animCurveInput = new AnimationCurve();
        [Editable] public GameObject gameObjectInput;
    }
}
