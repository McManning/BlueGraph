using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
{
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

        [Editable] public Transform transformInput;
    
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
