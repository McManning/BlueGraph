using BlueGraph;
using System.Collections;
using UnityEngine;

namespace BlueGraphSamples
{
    [Node(Path = "Dialog", Help = "Set all portraits at once. Unset portraits will be cleared.")]
    [Tags("Dialog")]
    public class SetPortraits : DialogFlowNode
    {
        [Input("Left")] public Texture2D left;
        [Input("Center")] public Texture2D center;
        [Input("Right")] public Texture2D right;

        [Editable("Keep Existing If Empty")] public bool keepExisting;

        public override IEnumerator Execute(DialogFlowData data)
        {
            var left = GetInputValue("Left", this.left);
            var center = GetInputValue("Center", this.center);
            var right = GetInputValue("Right", this.right);

            if (left != null || !keepExisting)
                data.ui.SetPortrait(PortraitPosition.Left, left);
            
            if (center != null || !keepExisting)
                data.ui.SetPortrait(PortraitPosition.Center, center);
            
            if (right != null || !keepExisting)
                data.ui.SetPortrait(PortraitPosition.Right, right);

            yield return null;
        }
    }
}
