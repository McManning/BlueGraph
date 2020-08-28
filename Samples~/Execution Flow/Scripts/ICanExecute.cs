namespace BlueGraphSamples
{
    /// <summary>
    /// Interface for a node with one or more ExecutionFlowData ports 
    /// </summary>
    public interface ICanExecute
    {
        /// <summary>
        /// Execute this node and return the next node to be executed.
        /// Override with your custom execution logic. 
        /// </summary>
        ICanExecute Execute(ExecutionFlowData data);
    }
}
