
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    public enum LogMode
    {
        Debug,
        Warning,
        Error
    };

    [Node(module = "Debug")]
    public class DebugLog : ExecNode 
    { 
        [Input] public string message;
        [Input(editable = false)] public object obj;
        [Input] public Object context;

        [Editable] public LogMode mode;

        public override ICanExec Execute(ExecData data)
        {
            string msg = GetInputValue("Message", message);

            if (obj != null)
            {
                msg += obj.ToString();
            }
            
            switch (mode)
            { 
                case LogMode.Debug:
                    Debug.Log(msg, context);
                    break;
                case LogMode.Warning:
                    Debug.LogWarning(msg, context);
                    break;
                case LogMode.Error:
                    Debug.LogError(msg, context);
                    break;
                default: break;
            }

            return base.Execute(data);
        }
    }
}
