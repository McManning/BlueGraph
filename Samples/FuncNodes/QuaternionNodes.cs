using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.FuncNodes
{
    /// <summary>
    /// Common quaternion operations
    /// </summary>
    [FuncNodeModule("Unity/Quaternion")]
    public static class QuaternionNodes
    {
        // Factories / Conversions

        [FuncNode("Identity (Quaternion)")]
        public static Quaternion Identity() => Quaternion.identity;

        public static Quaternion EulerToQuaternion(float x, float y, float z)
        {
            Quaternion rot = Quaternion.identity;
            rot.eulerAngles = new Vector3(x, y, z);
            return rot;
        }
        
        public static Vector3 QuaternionToEuler(Quaternion q) => q.eulerAngles;
        public static Quaternion EulerRotation(float x, float y, float z) => Quaternion.Euler(x, y, z);
        public static Quaternion AngleAxis(float angle, Vector3 axis) => Quaternion.AngleAxis(angle, axis);
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards) => Quaternion.LookRotation(forward, upwards);

        // Operations
        
        [FuncNode("Random (Quaternion)")]
        public static Quaternion Random() => UnityEngine.Random.rotation;
        
        [FuncNode("Dot (Quaternion)")]
        public static float Dot(Quaternion a, Quaternion b) => Quaternion.Dot(a, b);

        [FuncNode("Lerp (Quaternion)")]
        public static Quaternion Lerp(Quaternion a, Quaternion b, float t) => Quaternion.Lerp(a, b, t);

        [FuncNode("Multiply (Quaternion)")]
        public static Quaternion Multiply(Quaternion lhs, Quaternion rhs) => lhs * rhs;
        
        [FuncNode("Inverse (Quaternion)")]
        public static Quaternion Inverse(Quaternion q) => Quaternion.Inverse(q);
    }
}
