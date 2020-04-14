
namespace BlueGraphSamples
{
    /// <summary>
    /// Interface for a node with one or more Exec ports 
    /// </summary>
    public interface ICanExec
    {
        /// <summary>
        /// Execute this node and return the next node to be executed.
        /// Override with your custom execution logic. 
        /// </summary>
        ICanExec Execute(ExecData data);
    }
}
