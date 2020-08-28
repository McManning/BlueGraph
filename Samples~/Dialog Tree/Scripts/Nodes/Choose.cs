using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlueGraph;
using UnityEngine.UI;

namespace BlueGraphSamples
{
    [Node(Path = "Dialog")]
    [Tags("Dialog")]
    public class Choose : Node, ICanExecuteDialogFlow
    {        
        [Input("DialogFlowIn", Multiple = true)] readonly DialogFlowData data;
        
        [Input] public string option0;
        [Input] public bool enableOption0 = true;
        [Output] public DialogFlowData onOption0;

        [Input] public string option1;
        [Input] public bool enableOption1;
        [Output] public DialogFlowData onOption1;

        [Input] public string option2;
        [Input] public bool enableOption2;
        [Output] public DialogFlowData onOption2;

        private int selected;
        
        public override object OnRequestValue(Port port) => null;

        public IEnumerator Execute(DialogFlowData data)
        {
            string[] textConsts = new string[] { option0, option1, option2 };
            bool[] enabledConsts = new bool[] { enableOption0, enableOption1, enableOption2 };
            
            var mapping = new Dictionary<Button, int>(); // Button -> Option index
            int index = 0;

            // Display a button for each enabled option
            for (int i = 0; i < 3; i++)
            {
                var text = GetInputValue($"option{i}", textConsts[i]);
                var enabled = GetInputValue($"enableOption{i}", enabledConsts[i]);
                
                if (text.Length > 0 && enabled)
                {
                    var button = data.ui.ShowChoice(index, text);
                    mapping[button] = i;
                    index++;
                }
            }

            // Wait for one of the buttons to be pressed, and set our selected index
            // to the index mapped to that pressed button
            yield return new WaitForUIButtons(mapping.Keys.ToArray()).ReplaceCallback((button) =>
            {
                selected = mapping[button];
            });

            // Cleanup for the next node
            data.ui.ClearChoices();

            yield return null;
        }
        
        public ICanExecuteDialogFlow GetNext(DialogFlowData data)
        {
            // A different output is picked based on what they choose
            var port = GetPort($"onOption{selected}");
            return port.ConnectedPorts.FirstOrDefault()?.Node as ICanExecuteDialogFlow;
        }
    }
}
