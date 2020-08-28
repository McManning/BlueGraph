
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;
using UnityEditor;
using System;

namespace BlueGraphSamples
{
    /// <summary>
    /// Base view for Dialog Graph nodes that support breakpoints
    /// and have execution flow inputs/outputs.
    /// </summary>
    [CustomNodeView(typeof(DialogFlowNode))]
    [CustomNodeView(typeof(StartDialog))]
    [CustomNodeView(typeof(Choose))]
    [CustomNodeView(typeof(EndDialog))]
    public class DialogNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/DialogNodeView"));
            AddToClassList("dialogNodeView");

            // Customize placement of the default flow ports
            GetInputPort("DialogFlowIn")?.AddToClassList("flowInPortView");
            GetOutputPort("DialogFlowOut")?.AddToClassList("flowOutPortView");


            // Bad hack to grab the message input in the extensions
            // and configure it as a multiline.
            // TODO: Improve.
            /*if (extensionContainer.Q(null, "unity-text-field") is TextField text)           
            {
                Debug.Log("Found input");
                text.multiline = true;
            }*/
            
            var breakpointVisualizer = new VisualElement();
            breakpointVisualizer.AddToClassList("breakpointVisualizer");
            Add(breakpointVisualizer);

            if (Target is ICanBreak breakable)
            {
                breakable.OnBreakpointPause += OnBreakpointPause;
                breakable.OnBreakpointContinue += OnBreakpointContinue;
            }
        }
        void OnBreakpointPause()
        {
            AddToClassList("isBreakpointPaused");
        }


        void OnBreakpointContinue()
        {
            RemoveFromClassList("isBreakpointPaused");
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            // Add buttons to toggle breakpoints on nodes that support 'em
            if (Target is ICanBreak breakable)
            {
                evt.menu.AppendAction(
                    (breakable.HasBreakpoint) ? "Clear Breakpoint" : "Set Breakpoint",
                    (a) => ToggleBreakpoint()
                );

                if (breakable.IsBreakpointPaused)
                {
                    // TODO: Make this a visible button instead.
                    evt.menu.AppendAction(
                        "Continue from Breakpoint",
                        (a) => breakable.IsBreakpointPaused = false
                    );
                }
            }
        }

        void ToggleBreakpoint()
        {
            if (Target is ICanBreak breakable)
            {
                if (breakable.HasBreakpoint)
                {
                    breakable.HasBreakpoint = false;
                    RemoveFromClassList("hasBreakpoint");
                }
                else
                {
                    breakable.HasBreakpoint = true;
                    AddToClassList("hasBreakpoint");
                }
            }
        }
    }
}
