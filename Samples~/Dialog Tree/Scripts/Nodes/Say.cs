using System.Collections;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Dialog")]
    [Tags("Dialog")]
    public class Say : DialogFlowNode
    {
        [Input("Name")] public string speakerName;

        [Input("Portrait")] public Texture2D portrait;
        [Input("Position")] public PortraitPosition portraitPosition = PortraitPosition.Left;

        /// <summary>Characters-per-second to render the text</summary>
        [Input("Text Speed")] public float textSpeed = 0.02f;
        
        public string text;

        public override IEnumerator Execute(DialogFlowData data)
        {
            // Pull inputs for the node
            var speakerName = GetInputValue("Name", this.speakerName);
            var portrait = GetInputValue("Portrait", this.portrait);
            var portraitPosition = GetInputValue("Position", this.portraitPosition);
            var textSpeed = GetInputValue("Text Speed", this.textSpeed);
            
            // Update the UI to show our speaker
            if (portrait != null)
            {
                data.ui.SetPortrait(portraitPosition, portrait);
            }

            var btn = data.ui.ContinueButton.gameObject;
            btn.SetActive(false);

            // Dump text, letter-by-letter, into the UI
            int charCount = 0;
            while (charCount++ < text.Length)
            {
                data.ui.ShowMessage(text.Substring(0, charCount), speakerName);
                yield return new WaitForSeconds(textSpeed);
            }

            // Wait for the user to click continue before leaving this node
            btn.SetActive(true);
            yield return new WaitForUIButtons(data.ui.ContinueButton);

            // Cleanup for the next node
            data.ui.ClearMessage();

            yield return null;
        }
    }
}
