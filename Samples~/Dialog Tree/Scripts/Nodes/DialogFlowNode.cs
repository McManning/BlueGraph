using UnityEngine;
using BlueGraph;
using System.Linq;
using System.Collections;
using System;

namespace BlueGraphSamples
{
    /// <summary>
    /// Node that exposes an dialog flow port for both IO. 
    /// Inherit to make a node executable for forward execution. 
    /// </summary>
    [Output("DialogFlowOut", typeof(DialogFlowData), Multiple = false)]
    public abstract class DialogFlowNode : Node, ICanExecuteDialogFlow, ICanBreak
    {
        [Input("DialogFlowIn", Multiple = true)] public DialogFlowData data;

        #region ICanBreak Implementation
        [NonSerialized] private bool hasBreakpoint;

        public bool HasBreakpoint { 
            get { return hasBreakpoint; }
            set { hasBreakpoint = value; }
        }
        
        [NonSerialized] private bool isBreakpointPaused;

        public bool IsBreakpointPaused {
            get { return isBreakpointPaused; }
            set { isBreakpointPaused = value; }
        }

        
        [NonSerialized] private Action onBrekapointPause;
        
        public Action OnBreakpointPause {
            get { return onBrekapointPause; }
            set { onBrekapointPause = value; }
        }
        
        [NonSerialized] private Action onBreakpointContinue;
        
        public Action OnBreakpointContinue {
            get { return onBreakpointContinue; }
            set { onBreakpointContinue = value; }
        }
        #endregion

        #region ICanExecuteDialogFlow Implementation
        public abstract IEnumerator Execute(DialogFlowData data);

        /// <summary>
        /// Get the next node that should be executed along the edge
        /// </summary>
        /// <returns></returns>
        public virtual ICanExecuteDialogFlow GetNext(DialogFlowData data)
        {
            var port = GetPort("DialogFlowOut");
            return port.ConnectedPorts.FirstOrDefault()?.Node as ICanExecuteDialogFlow;
        }
        #endregion

        /// <remarks>
        /// Some nodes may have no outputs but execution flow, so we 
        /// can avoid the required OnRequestValue() impl on each one.
        /// </remarks>
        public override object OnRequestValue(Port port) => null;
    }
}
