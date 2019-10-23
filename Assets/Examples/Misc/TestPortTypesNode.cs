using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Value Tester")]
    public class ValueTester : AbstractNode
    {
        [Input] public float f;
        [Output("f")] public float fo;
        
        [Input] public Vector2 v2;
        [Output("v2")] readonly Vector2 v2o;
        
        [Input] public Vector3 v3;
        [Output("v3")] readonly Vector3 v3o;
        
        [Input] public DynamicVector dv;
        [Output("dv")] readonly DynamicVector dvo;
        
        // Pipe it right through.
        public override object GetOutputValue(string name)
        {
            switch (name)
            {
                case "f": return GetInputValue("F", f);
                case "v2": return GetInputValue("V2", v2);
                case "v3": return GetInputValue("V3", v3);
                case "dv": return GetInputValue("Dv", v3);
                default: return null;
            }
        }
    }

    /// <summary>
    /// Node that just dumps every type available for CSS checking
    /// </summary>
    [Node("Test Port Types")]
    public class TestPortTypesNode : AbstractNode
    {
        // TODO: Inputs don't present an editable field inline
        // unless they're public. Again, Unity serialization stuff.
        [Input] public int int32Input;
        [Input] public long int64Input;
        

        [Input] public float floatInput;
        [Input("V2 Input")] public Vector2 vector2Input;
        [Input("V3 Input")] public Vector3 vector3Input;
        [Input("V4 Input")] public Vector4 vector4Input;
    
        [Input] public Transform transformInput;
        [Input] public Matrix4x4 mat4x4Input;

        [Input] public KeyCode keyInput;

        [Input] public Color colorInput;
        [Input] public string stringInput;
        [Input] public bool boolInput;
        [Input] public Texture2D texture2DInput;

        [Input] public AnimationCurve animCurveInput = new AnimationCurve();
        [Input] public GameObject gameObjectInput;
        
        [Input("")] public float noNameInput;
    

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
        
        [Input("")] public float noNameOutput;

        public override object GetOutputValue(string name)
        {
            // Would need to default to the input variable by name.. so still need introspection.
            // Woo.
            return GetInputValue<object>(name.Substring(0, name.Length - "Output".Length) + "Input");
        }
    }
}
