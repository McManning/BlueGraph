
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    public enum BlendMode
    {
        Add,
        Multiply,
        Average,
        Min,
        Max,
        Abs
    };

    [Node(Path = "Heightmap/Transform")]
    public class Blend : HeightmapTransform
    {
        [Input] public Heightmap background;
        [Input] public float opacity = 1f;

        [Editable] public BlendMode mode;

        protected void Add(Heightmap background, float opacity)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = (result[i] * opacity) + background[i];
        }

        protected void Multiply(Heightmap background, float opacity)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = (result[i] * opacity) * background[i];
        }

        protected void Average(Heightmap background, float opacity)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = ((result[i] * opacity) + background[i]) * 0.5f;
        }

        protected void Min(Heightmap background)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = Mathf.Min(result[i], background[i]);
        }

        protected void Max(Heightmap background)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = Mathf.Max(result[i], background[i]);
        }
        
        protected void Abs()
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = Mathf.Abs(result[i]);
        }

        public override void Execute()
        {
            Heightmap background = GetInputValue("background", this.background);
            float opacity = GetInputValue("opacity", this.opacity);

            // No output until we have both inputs
            if (background == null)
            {
                result = null;
                return;
            }

            switch (mode)
            {
                case BlendMode.Add: 
                    Add(background, opacity);
                    break;
                case BlendMode.Multiply:
                    Multiply(background, opacity);
                    break;
                case BlendMode.Average:
                    Average(background, opacity);
                    break;
                case BlendMode.Min:
                    Min(background);
                    break;
                case BlendMode.Max:
                    Max(background);
                    break;
                case BlendMode.Abs:
                    Abs();
                    break;
            }
        }
    }
}
