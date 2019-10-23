using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.FuncNodes
{
    /// <summary>
    /// Common quaternion operations
    /// </summary>
    [FuncNodeModule("Unity/Vector")]
    public static class VectorNodes
    {
        // Factories / Conversions

        public static Vector2 MakeVector2(float x, float y) => new Vector2(x, y);
        public static Vector3 MakeVector3(float x, float y, float z) => new Vector3(x, y, z);
        public static Vector3 MakeVector4(float x, float y, float z, float w) => new Vector4(x, y, z, w);
        
        [FuncNode("Break (Vector2)")]
        public static float BreakVector2(Vector2 v, out float y)
        {
            y = v.y;
            return v.x;
        }
        
        [FuncNode("Break (Vector3)")]
        public static float BreakVector3(Vector3 v, out float y, out float z)
        {
            y = v.y;
            z = v.z;
            return v.x;
        }
        
        [FuncNode("Break (Vector4)")]
        public static float BreakVector4(Vector4 v, out float y, out float z, out float w)
        {
            y = v.y;
            z = v.z;
            w = v.w;
            return v.x;
        }
    }
}
