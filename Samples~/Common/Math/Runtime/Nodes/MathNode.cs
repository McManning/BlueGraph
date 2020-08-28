using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Base class for everything MATHEMATICAL
    /// </summary>
    public abstract class MathNode : Node
    {
    
    }

    /// <summary>
    /// Automatic one-input one-output node
    /// </summary>
    public abstract class MathNode<TIn, TOut> : MathNode
    {
        [Input] public TIn value;
        [Output] public TOut result;
        
        public abstract TOut Execute(TIn value);

        public override object OnRequestValue(Port port) => Execute(
            GetInputValue("value", value)
        );
    }

    /// <summary>
    /// Automatic two-inputs one-output node
    /// </summary>
    public abstract class MathNode<TIn1, TIn2, TOut> : MathNode
    {
        [Input] public TIn1 value1;
        [Input] public TIn2 value2;
        [Output] public TOut result;
        
        public abstract TOut Execute(TIn1 value1, TIn2 value2);

        public override object OnRequestValue(Port port) => Execute(
            GetInputValue("value1", value1),
            GetInputValue("value2", value2)
        );
    }

    // TODO: These generics probably won't work on AOT out of the box.
    // We'd need to prime each one by calling it directly somewhere.
    // E.g. a PrimeForAOT() function that pre-instantiates each type
}
