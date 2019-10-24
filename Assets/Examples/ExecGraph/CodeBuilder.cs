
using System;
using System.Collections.Generic;
using System.Text;
using BlueGraph;
using UnityEngine;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// An experiment in compiling nodes into a high performance / AOT-supported function.
    /// 
    /// Supports graphs composed entirely of FuncNodes or nodes that implement ICanCompile
    /// </summary>
    public class CodeBuilder
    {
        /// <summary>
        /// Number of spaces to indent scopes
        /// </summary>
        public int indent = 4;

        /// <summary>
        /// Generated class name
        /// </summary>
        public string className;

        StringBuilder m_Builder = new StringBuilder();
        readonly Scope m_Root;
        Scope m_CurrentScope;

        /// <summary>
        /// Namespaces hoisted out of FuncNode calls
        /// </summary>
        HashSet<string> m_Namespaces = new HashSet<string>();

        public class Scope
        {
            public int depth = 1;
            public Scope parent;

            // Nodes executed in this scope
            public List<AbstractNode> nodes = new List<AbstractNode>();
            
            public Scope(Scope parent = null)
            {
                this.parent = parent;

                if (parent != null)
                {
                    depth = parent.depth + 1;
                }
            }

            /// <summary>
            /// Was the input node executed in this scope or a parent scope
            /// </summary>
            public bool AlreadyExecutedInScope(AbstractNode node)
            {
                if (nodes.Contains(node)) return true;

                if (parent != null) return parent.AlreadyExecutedInScope(node);

                return false;
            }
        }

        public CodeBuilder()
        {
            m_Root = new Scope();
            m_CurrentScope = m_Root;
        }

        /// <summary>
        /// Start a new scope for code injection. Variable declarations and function calls
        /// will only occur within this scope when calling Append/AppendLine. 
        /// 
        /// Opening brace is automatically added when entering a new scope.
        /// </summary>
        public void BeginScope()
        {
            AppendLine("{");
            m_CurrentScope = new Scope(m_CurrentScope);
            AppendLine("// Scope: " + m_CurrentScope.GetHashCode());
        }

        /// <summary>
        /// End a scope, returning to the parent scope. Closing brace is automatically added.
        /// </summary>
        public void EndScope()
        {
            m_CurrentScope = m_CurrentScope.parent;
            AppendLine("}");
        }

        /// <summary>
        /// Append code without creating a new line or automatic indenting
        /// </summary>
        /// <param name="value"></param>
        public void Append(string value)
        {
            m_Builder.Append(value);
        }

        /// <summary>
        /// Add a blank line
        /// </summary>
        public void AppendLine()
        {
            m_Builder.AppendLine(new string(' ', m_CurrentScope.depth * indent));
        }

        /// <summary>
        /// Add a new line of code, automatically indenting to the current scope
        /// </summary>
        public void AppendLine(string line)
        {
            m_Builder.AppendLine(new string(' ', m_CurrentScope.depth * indent) + line);
        }
        
        /// <summary>
        /// Convert a NodePort to a unique C# variable name 
        /// </summary>
        public string PortToVariable(NodePort port)
        {
            // TODO: Drastically improve this 
            return $"{port.portName ?? "Unnamed"}_{port.node.guid.Substring(0, 8)}";
        }

        /// <summary>
        /// Convert any (within reason) value to a printable constant
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Constant(object value)
        {
            if (value == null)
            {
                return "null";
            }

            switch (value)
            {
                case float f: return $"{f}f";
                case int i: return i.ToString();
                case bool b: return b.ToString();
                case string s: return $"\"{s}\"";
                // and so on?
            }

            return $"CONST`{value.ToString()}`";
        }

        /// <summary>
        /// Convert default(type) to a printable constant
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public string DefaultValue(Type type) 
        {
            if (type.IsValueType)
            {
                return Constant(Activator.CreateInstance(type));
            }

            return Constant(null);
        }

        /// <summary>
        /// Walk up the input tree and add node results. 
        /// </summary>
        /// <param name="outputPort"></param>
        public void CompileInputs(NodePort outputPort)
        {
            if (outputPort.node is FuncNode funcNode)
            {
                CompileFuncNode(funcNode);
            }
        }

        /// <summary>
        /// Has the given node already had code generation happen in / above current scope.
        /// </summary>
        /// <param name="node"></param>
        public bool AlreadyInScope(AbstractNode node)
        {
            return m_CurrentScope.AlreadyExecutedInScope(node);
        }

        public void CompileFuncNode(FuncNode node)
        {
            if (AlreadyInScope(node)) 
            {
                AppendLine($"// Already compiled {node.name} ({node.guid.Substring(0, 8)}) in scope");
                return;
            }

            m_CurrentScope.nodes.Add(node);

            string className = HoistNamespace(node.className);
            string methodName = node.methodName;
            string returnValue = "";
            
            // Declare outputs / get inputs
            List<string> args = new List<string>();
            for (int i = 0; i < node.ports.Count; i++) 
            {
                NodePort port = node.ports[i];
                string variableName = PortToVariable(port);

                // Handle return value separately, since it's not an argument
                if (node.hasReturnValue && i == node.ports.Count - 1)
                {
                    returnValue = $"{HoistNamespace(port.type)} {variableName} = ";
                    continue;
                }

                // Declare out variables
                if (!port.isInput)
                {
                    AppendLine($"{HoistNamespace(port.type)} {variableName};");
                    args.Add($"out {variableName}");
                }
                else
                {
                    if (port.connections.Count > 0)
                    {
                        NodePort outputPort = port.connections[0].node.GetOutputPort(
                            port.connections[0].portName
                        );

                        CompileInputs(outputPort);
                        args.Add(PortToVariable(outputPort));
                    }
                    else
                    {
                        // Constant default (actual inline editables / non default() not yet supported)
                        args.Add(DefaultValue(port.type));
                    }
                }
            }
            

            // Execute the underlying function
            AppendLine($"{returnValue}{className}.{methodName}({string.Join(", ", args)});");
        }
        
        /// <summary>
        /// Hoist the namespace out of the class name and into global `using` statements
        /// </summary>
        public string HoistNamespace(string className)
        {
            return HoistNamespace(Type.GetType(className));
        }

        public string HoistNamespace(Type type)
        {
            string ns = type.Namespace;
            m_Namespaces.Add(ns);

            return type.FullName.Substring(ns.Length + 1);
        }

        /// <summary>
        /// Compile a ready to use static class for this node. 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder code = new StringBuilder();
            
            code.AppendLine("// Autogenerated - Yadda yadda");
            foreach (var ns in m_Namespaces)
            {
                code.AppendLine($"using {ns};");
            }

            code.AppendLine();
            code.AppendLine("public static class GraphAOT {");
            code.Append(m_Builder);
            code.AppendLine("}");
            
            return code.ToString();
        }
    }
}
