using BlueGraph;
using System.Collections;
using System.Linq;

namespace BlueGraphSamples
{
    [Node(Deletable = false)]
    [Tags("Hidden")]
    [Output("DialogFlowOut", typeof(DialogFlowData), Multiple = false)]
    public class StartDialog : Node, ICanExecuteDialogFlow
    {
        public override object OnRequestValue(Port port) => null;

        public ICanExecuteDialogFlow GetNext(DialogFlowData data)
        {
            var port = GetPort("DialogFlowOut");
            return port.ConnectedPorts.FirstOrDefault()?.Node as ICanExecuteDialogFlow;
        }

        public IEnumerator Execute(DialogFlowData data)
        {
            // No-op, this is just an entry point to the graph
            yield return null;
        }
    }
}
