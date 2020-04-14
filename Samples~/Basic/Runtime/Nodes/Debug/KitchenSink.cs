
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Example of every port type supported by default.
    /// </summary>
    [Node(module = "Debug")]
    public class KitchenSink : AbstractNode
    {
        // Value types
        [Input, Output("Float Out")] public float floatIn;
        [Input, Output("Int Out")] public int intIn;
        [Input, Output("Str Out")] public string strIn;
        [Input, Output("Bool Out")] public bool boolIn;
        [Input, Output("Double Out")] public double doubleIn;

        // Reference types
        [Input, Output("Obj Out")] public object objIn;

        // Common Unity structs
        [Input, Output("Rect Out")] public Rect rectIn;
        [Input, Output("Vec2 Out")] public Vector2 vec2In;
        [Input, Output("Vec3 Out")] public Vector3 vec3In;
        [Input, Output("Vec4 Out")] public Vector4 vec4In;
        [Input, Output("Mat Out")] public Matrix4x4 matIn;
        [Input, Output("Color Out")] public Color colorIn;
        [Input, Output("Quat Out")] public Quaternion quatIn;
        [Input, Output("Layers Out")] public LayerMask layersIn;

        // Common Unity classes
        [Input, Output("Curve Out")] public AnimationCurve curveIn;
        [Input, Output("Gradient Out")] public Gradient gradientIn;
        [Input, Output("GameObject Out")] public GameObject gameObjectIn;
        [Input, Output("Transform Out")] public Transform transformIn;

        // Common Unity enums
        [Input, Output("Keys Out")] public KeyCode keysIn;

        // Custom IConvertible struct
        [Input, Output("Dynamic Vec Out")] public DynamicVector dynamicVecIn;

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
