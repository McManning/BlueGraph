
using UnityEngine;
using BlueGraph;
using System.Text;

namespace BlueGraphExamples.ExecGraph
{
    [Node(module = "ExecGraph")]
    public class Print : ExecNode, ICanCompile
    {
        [Input] public string value;
        [Input] public object obj;

        public override ExecNode Execute(ExecData data)
        {
            string value = GetInputValue("Value", this.value);
            object obj = GetInputValue("Obj", this.obj);
            Debug.Log($"{value}: {obj}");

            return base.Execute(data);
        }
        
        public void Compile(CodeBuilder builder)
        {
            NodePort valuePort = GetInputPort("Value");
            NodePort objPort = GetInputPort("Obj");
            
            // Get an empty string or a ToString() of an arbitrary input variable
            string objectVariable = "";
            if (objPort.IsConnected)
            {
                NodePort outputPort = objPort.GetConnection(0);
                builder.CompileInputs(outputPort);

                objectVariable = $" + {builder.PortToVariable(outputPort)}.ToString()";
            }
            
            // Get a constant or a variable from another evaluated port. 
            string valueVariable = builder.Constant(value);
            if (valuePort.IsConnected)
            {
                NodePort outputPort = valuePort.GetConnection(0);
                builder.CompileInputs(outputPort);

                valueVariable = builder.PortToVariable(outputPort);
            }
            
            builder.AppendLine($"Debug.Log({valueVariable}{objectVariable});");
            
            // Continue to next executable node
            var next = GetNextExec();
            if (next is ICanCompile nextNode)
            {
                nextNode.Compile(builder);
            }
        }
    }
}
