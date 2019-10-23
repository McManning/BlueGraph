using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.FuncNodes
{
    /// <summary>
    /// FuncNode wrappers around Unity.Mathf methods
    /// </summary>
    [FuncNodeModule("Unity/Mathf")]
    public static class MathfNodes
    {
        public static float Deg2Rad(float deg) => deg * Mathf.Deg2Rad;
        public static float Rad2Deg(float rad) => rad * Mathf.Rad2Deg;

        public static float Abs(float f) => Mathf.Abs(f);
        public static bool Approximately(float a, float b) => Mathf.Approximately(a, b);
        public static float Ceil(float f) => Mathf.Ceil(f);
        public static float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);
        public static float Cos(float f) => Mathf.Cos(f);
        public static float Exp(float power) => Mathf.Exp(power);
        public static float Floor(float f) => Mathf.Floor(f);

        [FuncNode("Lerp (float)")]
        public static float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);

        public static float Max(float a, float b) => Mathf.Max(a, b);
        public static float Min(float a, float b) => Mathf.Min(a, b);
        public static float Pow(float f, float p) => Mathf.Pow(f, p);
        
        public static float Round(float f) => Mathf.Round(f);
        public static float Sign(float f) => Mathf.Sign(f);
        public static float Sin(float f) => Mathf.Sin(f);
        public static float Sqrt(float f) => Mathf.Sqrt(f);
        public static float Tan(float f) => Mathf.Tan(f);

        [FuncNode("Perlin Noise")]
        public static float PerlinNoise(float x, float y) => Mathf.PerlinNoise(x, y);

        // Technically not Mathf, but close enough.
        [FuncNode("Random (float)")]
        public static float Random(float min, float max) => UnityEngine.Random.Range(min, max);
    }
}
