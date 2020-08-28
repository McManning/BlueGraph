
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    public enum CurveMode
    {
        Multiply,
        Add
    };

    [Node(Path = "Heightmap/Transform")]
    public class Curve : HeightmapTransform 
    {
        [Input] public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 1);
        [Editable] public CurveMode mode = CurveMode.Multiply;
        
        protected void Add(AnimationCurve curve)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] += curve.Evaluate(result[i]);
        }

        protected void Multiply(AnimationCurve curve)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] *= curve.Evaluate(result[i]);
        }

        public override void Execute()
        {
            AnimationCurve curve = GetInputValue("curve", this.curve);

            switch (mode)
            {
                case CurveMode.Add:
                    Add(curve);
                    break;
                case CurveMode.Multiply:
                    Multiply(curve);
                    break;
            }
        }
    }
}
