using System.Collections;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node]
    [Tags("Dialog")]
    public class EndDialog : Node, ICanExecuteDialogFlow
    {
        [Input("DialogFlowIn", Multiple = true)] readonly DialogFlowData data;
        
        public override object OnRequestValue(Port port) => null;

        public ICanExecuteDialogFlow GetNext(DialogFlowData data)
        {
            // No outputs, this is the end of the graph.
            return null;
        }
        
        public IEnumerator Execute(DialogFlowData data)
        {
            // This node doesn't do anything, it'll just mark the
            // end of the graph and let them return to the game
            yield return null;
        }
    }
}
