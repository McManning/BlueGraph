using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    public enum PrintMode
    {
        Log,
        Warning,
        Error
    }

    [Node(Path = "Debug", Help = "Print input value to console every update")]
    [Tags("Basic")]
    public class Print : Node
    {
        /// <summary>Accept any input value</summary>
        [Input] public object value;
        [Output("")] protected object output;

        [Editable] public PrintMode mode;

        public override object OnRequestValue(Port port)
        {
            switch (mode)
            {
                case PrintMode.Log:
                    Debug.Log($"<b>[Debug Node] Requested value: `{value}`");
                    break;
                case PrintMode.Warning:
                    Debug.LogWarning($"<b>[Debug Node] Requested value: `{value}`");
                    break;
                case PrintMode.Error:
                    Debug.LogError($"<b>[Debug Node] Requested value: `{value}`");
                    break;
                default: break;
            }

            return value;
        }
    }
}
