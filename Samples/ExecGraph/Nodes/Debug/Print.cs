
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Output a string to console and an arbitrary object (if it supports ToString())
    /// </summary>
    [Node(module = "ExecGraph/Debug")]
    public class Print : ExecNode, ICanCompile
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        };

        [Input] public string value;
        [Input] public object obj;
        
        [Editable] public LogLevel level = LogLevel.Info;

        public override ICanExec Execute(ExecData data)
        {
            string value = GetInputValue("Value", this.value);
            object obj = GetInputValue("Obj", this.obj);

            string message = $"{value}{obj}";
            switch (level)
            {
                case LogLevel.Info: Debug.Log(message); break;
                case LogLevel.Warning: Debug.LogWarning(message); break;
                case LogLevel.Error: Debug.LogError(message); break;
            }

            return base.Execute(data);
        }
        
        public void Compile(CodeBuilder builder)
        {
            NodePort valuePort = GetInputPort("Value");
            NodePort objPort = GetInputPort("Obj");
            
            var valueVar = builder.PortToValue(valuePort, value);
            var objVar = builder.PortToValue(objPort, obj);
            
            if (objVar.value != "null")
            {
                objVar.value = $" + {objVar}.ToString()";
            } 
            else
            {
                objVar.value = "";
            }

            string method = "Log";
            switch (level)
            {
                case LogLevel.Info: method = "Log"; break;
                case LogLevel.Warning: method = "LogWarning"; break;
                case LogLevel.Error: method = "LogError"; break;
            }

            builder.AppendLine($"Debug.{method}({valueVar}{objVar});");
            
            // Continue to next executable node
            var next = GetNextExec();
            if (next is ICanCompile nextNode)
            {
                nextNode.Compile(builder);
            }
        }
    }
}
