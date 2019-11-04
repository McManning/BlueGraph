
namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Node can be converted to direct C# code for higher performance & AOT support
    /// </summary>
    public interface ICanCompile
    {
        /// <summary>
        /// Add the variable declarations and function 
        /// calls for this node to the generated code.
        /// </summary>
        void Compile(CodeBuilder builder);
    }
}
