using BlueGraph;
using System.Collections;
using UnityEngine;

namespace BlueGraphSamples
{
    [Node(Path = "Dialog")]
    [Tags("Dialog")]
    public class Wait : DialogFlowNode
    {
        [Input("Seconds")] public float seconds;

        public override IEnumerator Execute(DialogFlowData data)
        {
            yield return new WaitForSeconds(
                GetInputValue("Seconds", this.seconds)
            );
        }
    }
}
