using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    /// <summary>
    /// Nodes for creating/working with Dynamic Vectors
    /// </summary>
    [FuncNodeModule("Unity/Vector")]
    public static class DynamicVectorNodes
    {
        // Factories
        [FuncNode("Create Dynamic (float)")]
        public static DynamicVector FromFloat(float f) => f;
        
        [FuncNode("Create Dynamic (Vector2)")]
        public static DynamicVector FromVector2(Vector2 v) => v;
        
        [FuncNode("Create Dynamic (Vector3)")]
        public static DynamicVector FromVector3(Vector3 v) => v;
        
        [FuncNode("Create Dynamic (Vector4)")]
        public static DynamicVector FromVector4(Vector4 v) => v;
        
        // Explicit type conversions
        public static float AsFloat(DynamicVector v) => v;
        public static Vector2 AsVector2(DynamicVector v) => v;
        public static Vector3 AsVector3(DynamicVector v) => v;
        public static Vector4 AsVector4(DynamicVector v) => v;
    }
}
