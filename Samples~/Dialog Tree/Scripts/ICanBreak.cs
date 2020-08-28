using System;

namespace BlueGraphSamples
{
    /// <summary>
    /// Interface for nodes that can have breakpoints set on 
    /// them and paused on during execution.
    /// </summary>
    public interface ICanBreak
    {
        bool HasBreakpoint { get; set; }

        bool IsBreakpointPaused { get; set; }

        /// <summary>
        /// Delegates to execute when the breakpoint is activated.
        /// 
        /// Set IsBreakpointPaused to false to continue.
        /// </summary>
        Action OnBreakpointPause { get; set; }

        /// <summary>
        /// Delegates to execute when we continue from a paused breakpoint
        /// </summary>
        Action OnBreakpointContinue { get; set; } 
    }
}
