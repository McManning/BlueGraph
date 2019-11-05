
using UnityEngine;
using BlueGraph;
using System;

namespace BlueGraphExamples.ExecGraph
{
    [Node(module = "ExecGraph/GameObject")]
    public class Instantiate : ExecNode, ICanCompile
    {
        [Input] public GameObject prefab;
        [Input] public Vector3 localPosition;
        [Input] public Quaternion localRotation;
        [Input] public GameObject parent;

        [Output] GameObject instance;

        public override ICanExec Execute(ExecData data)
        {
            GameObject go = GetInputValue<GameObject>("Prefab", null);
            GameObject pgo = GetInputValue<GameObject>("Parent", null);
            Vector3 position = GetInputValue("Local Position", localPosition);
            Quaternion rotation = GetInputValue("Local Rotation", localRotation);

            if (go)
            {
                instance = Instantiate(go, position, rotation, pgo?.transform);
            }
            
            return base.Execute(data);
        }
        
        public void Compile(CodeBuilder builder)
        {
            string prefabVar = ConstantOrVariable<GameObject>(builder, "Prefab", null);
            string parentVar = null;
            string localPositionVar = ConstantOrVariable(builder, "Local Position", localPosition);
            string localRotationVar = ConstantOrVariable(builder, "Local Rotation", localRotation);
            
            NodePort port = GetInputPort("Parent");
            if (port.IsConnected)
            {
                NodePort outputPort = port.GetConnection(0);
                builder.CompileInputs(port);

                parentVar = builder.PortToVariableName(outputPort);
            }
            
            if (parentVar != null)
            {
                builder.AppendLine($"GameObject TODO = Instantiate(" +
                    $"{prefabVar}, {localPositionVar}, {localRotationVar}, {parentVar}.transform" +
                    $");"
                );
            }
            else // Shorter version without a parent transform
            {
                builder.AppendLine($"GameObject TODO = Instantiate(" +
                    $"{prefabVar}, {localPositionVar}, {localRotationVar}" +
                    $");"
                );
            }
            
            // Continue to next executable node
            // TODO: This would be duplicated for every ExecNode. 
            // Put it in ExecNode or something as the base behavior
            var next = GetNextExec();
            if (next is ICanCompile nextNode)
            {
                nextNode.Compile(builder);
            }
        }

        /// <summary>
        /// If the given input port has a connection, compile and return the connection's
        /// output variable name. If not, return a constant representing the default (inline) value.
        /// </summary>
        private string ConstantOrVariable<T>(CodeBuilder builder, string portName, T defaultValue = default)
        {
            NodePort port = GetInputPort(portName);
            return builder.PortToValue(port, defaultValue).ToString();
        }
    }
}
