using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.UnityMath
{
    /// <summary>
    /// FuncNode wrappers around Unity.Mathf methods
    /// </summary>
    [FuncNodeModule("Unity/Mathf")]
    public static class FuncLibrary
    {
        public static float Abs(float f) => Mathf.Abs(f);
        public static bool Approximately(float a, float b) => Mathf.Approximately(a, b);
        public static float Ceil(float f) => Mathf.Ceil(f);
        public static float Floor(float f) => Mathf.Floor(f);
        public static float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);
        public static float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);

        // Custom node name + module override
        [FuncNode("Perlin Noise", module = "Unity/Mathf/Random")] 
        public static float PerlinNoise(float x, float y) => Mathf.PerlinNoise(x, y);

        // ... and so on.

        // TODO: Is there a faster registration method? Can I just slap FuncNodeModule
        // directly onto Mathf and call it a day? 
    }
}
