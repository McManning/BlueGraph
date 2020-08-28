using System.Collections.Generic;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Example of every port type supported by default.
    /// </summary>
    [Node(Path = "Debug")]
    [Tags("Basic")]
    [Output("Class Attr Float Out", typeof(float))]
    [Output("Class Attr Int Out", typeof(int))]
    public class KitchenSink : Node
    {
        // Value types
        [Input, Output("float")] public float floatIn;
        [Input, Output("int")] public int intIn;
        [Input, Output("string")] public string strIn;
        [Input, Output("bool")] public bool boolIn;
        [Input, Output("double")] public double doubleIn;

        // Reference types
        [Input, Output("object")] public object objIn;

        // Common Unity structs
        [Input, Output("Rect")] public Rect rectIn;
        [Input, Output("Vector2")] public Vector2 vec2In;
        [Input, Output("Vector3")] public Vector3 vec3In;
        [Input, Output("Vector4")] public Vector4 vec4In;
        [Input, Output("Matrix4x4")] public Matrix4x4 matIn;
        [Input, Output("Color")] public Color colorIn;
        [Input, Output("Quaternion")] public Quaternion quatIn;
        [Input, Output("LayerMask")] public LayerMask layersIn;

        // Common Unity classes
        [Input, Output("AnimationCurve")] public AnimationCurve curveIn;
        [Input, Output("Gradient")] public Gradient gradientIn;
        [Input, Output("GameObject")] public GameObject gameObjectIn;
        [Input, Output("Transform")] public Transform transformIn;

        // Common Unity enums
        [Input, Output("KeyCode")] public KeyCode keysIn;

        // Custom IConvertible struct
        [Input, Output("Dynamic Vec")] public DynamicVector dynamicVecIn;

        // Collections
        [Input, Output("float[]")] public List<float> floatListIn;
        [Input, Output("int[]")] public int[] intArrayIn;
        [Input, Output("string[]")] public string[] stringArrayIn;
        [Input, Output("bool[]")] public bool[] boolArrayIn;
        [Input, Output("Vector2[]")] public Vector2[] vec2ArrayIn;
        [Input, Output("Quaternion[]")] public Quaternion[] quatArrayIn;
        [Input, Output("Transform[]")] public Transform[] transformArrayIn;
        [Input, Output("GameObject[]")] public GameObject[] goArrayIn;

#region EDITABLES
        // Value types
        [Editable] public float floatVal;
        [Editable] public int intVal;
        [Editable] public string strVal;
        [Editable] public bool boolVal;
        [Editable] public double doubleVal;
        
        // Common Unity structs
        [Editable] public Rect rectVal;
        [Editable] public Vector2 vec2Val;
        [Editable] public Vector3 vec3Val;
        [Editable] public Vector4 vec4Val;
        [Editable] public Matrix4x4 matVal;
        [Editable] public Color colorVal;
        [Editable] public Quaternion quatVal;
        [Editable] public LayerMask layersVal;

        // Common Unity classes
        [Editable] public AnimationCurve curveVal;
        [Editable] public Gradient gradientVal;
        [Editable] public GameObject gameObjectVal;
        [Editable] public Transform transformVal;

        // Common Unity enums
        [Editable] public KeyCode keysVal;
#endregion

        /// <summary>
        /// This component just demos ports and does not actually
        /// output any values when connected.
        /// </summary>
        public override object OnRequestValue(Port port) => null;
    }
}
