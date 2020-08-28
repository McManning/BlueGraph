using UnityEngine;
using BlueGraph;
using System.Runtime.InteropServices;
using System.Collections;
using System;

#if UNITY_EDITOR
using BlueGraph.Editor;
#endif 

namespace BlueGraphSamples
{
    /// <summary>
    /// Data to be passed between ICanExecuteDialogFlow nodes
    /// </summary>
    public class DialogFlowData
    {
        /// <summary>
        /// The Unity UI containing elements that we can control via nodes
        /// (choice buttons, text messages, portraits, etc)
        /// </summary>
        public DialogUI ui;
    }

    public interface ICanExecuteDialogFlow
    {
        /// <summary>
        /// Run the logic for this node as a coroutine, yielding if pending work
        /// </summary>
        IEnumerator Execute(DialogFlowData data);

        /// <summary>
        /// Get the next node that should be executed in the flow
        /// </summary>
        ICanExecuteDialogFlow GetNext(DialogFlowData data);
    }

    /// <summary>
    /// A graph for representing a conversation between two or more characters
    /// and the branching options that the player can pick for the conversation.
    /// </summary>
    [CreateAssetMenu(menuName = "BlueGraph Samples/Dialog Graph", fileName = "New DialogGraph")]
    [IncludeTags("Dialog", "Player", "Math")]
    public class DialogGraph : Graph
    {
        #if UNITY_EDITOR
        public void OnEnable()
        {
            if (GetNode<StartDialog>() == null)
            {
                var start = NodeReflection.Instantiate<StartDialog>();
                AddNode(start);
            }

            if (GetNode<EndDialog>() == null)
            {
                var end = NodeReflection.Instantiate<EndDialog>();
                end.Position = new Vector2(300, 0);
                AddNode(end);
            }
        }
        #endif

        /// <summary>
        /// Coroutine to start the conversation
        /// </summary>
        public IEnumerator Execute(DialogUI ui)
        {
            var data = new DialogFlowData
            {
                ui = ui
            };
            
            var current = GetNode<StartDialog>() as ICanExecuteDialogFlow;

            while (current != null)
            {
                // Check for a breakpoint on the node
                if (current is ICanBreak breakable)
                {
                    if (breakable.HasBreakpoint)
                    {
                        breakable.IsBreakpointPaused = true;
                        breakable.OnBreakpointPause();

                        Debug.LogError("Paused on breakpoint. Right click the paused node to continue.", this);
                        yield return new WaitUntil(() => !breakable.IsBreakpointPaused);

                        breakable.OnBreakpointContinue();
                    }
                }

                yield return current.Execute(data);
                current = current.GetNext(data);
            }

            yield return null;
        }
    }
}
