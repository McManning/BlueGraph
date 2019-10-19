using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace BlueGraph
{
    /// <summary>
    /// Node that exposes a pure static method to the graph
    /// </summary>
    public class FuncNode : AbstractNode
    {
        delegate object FuncWrapperDelegate(object[] args);
        static Dictionary<string, FuncWrapperDelegate> k_DelegateCache = new Dictionary<string, FuncWrapperDelegate>();

        object[] m_ArgsCache;
        
        // TODO: Not public (but still serialized). Needed by NodeReflection but 
        // I can probably just call CreateLambda directly.
        public string className;
        public string methodName;
        
        FuncWrapperDelegate m_Func;

        object m_ReturnValue;

        public void Awake()
        {
            CheckForDelegate();
        }

        /// <summary>
        /// Execute the wrapped method and get the named output value
        /// </summary>
        public override object GetOutputValue(string name)
        {
            // TODO: Don't re-execute unless we dirty inputs
            ExecuteMethod();
            
            if (name == "Result")
            {
                return m_ReturnValue;
            }

            // Other out argument, find matching index
            for (int i = 0; i < ports.Count - 1; i++) 
            {
                if (!ports[i + 1].isInput && ports[i + 1].portName == name)
                {
                    return m_ArgsCache[i];
                }
            }
            
            // TODO: Better error handling
            throw new Exception($"[{this.name}] Missing requested output slot {name}");
        }
        
        /// <summary>
        /// Make sure a delegate is created to wrap the bound method for in-graph execution
        /// </summary>
        protected void CheckForDelegate()
        {
            // Re-bind to the method if necessary
            if (m_Func == null && methodName != null && className != null)
            {
                // TODO: This is going to probably be slow due to each node
                // wanting to load it. Probably not cached well.
                MethodInfo method = Type.GetType(className).GetMethod(methodName);
                CreateDelegate(method);
            }
        }
        
        /// <summary>
        /// Construct a lambda expression to call the wrapped method.
        /// 
        /// At runtime, this ends up being substantially faster than MethodInfo.Invoke()
        /// but we end up with limited support (i.e. no AOT platform support)
        /// </summary>
        public void CreateDelegate(MethodInfo method)
        {
            methodName = method.Name;
            className = method.DeclaringType.FullName;

            // If a copy of the lambda delegate is already in cache, use that.
            string key = $"{className}|{methodName}";
            if (k_DelegateCache.ContainsKey(key))
            {
                return;
            }

            /* In short, this method will compile into the following lambda:
             
            (args) => {
                float ret;
                float a;
                float b;
                ret = MyMethod(args[0], args[1], out a, out b);
                args[2] = a;
                args[3] = b;
                return (object)ret;
            }
            
            This will accept an object[] containing input and output placeholders
            for the wrapped method, execute, replace output placeholders with the 
            resolved out parameter values and return the method's return value. 
            */

            // Input parameter list into the expression
            ParameterExpression argsExp = Expression.Parameter(typeof(object[]), "args");

            // List of out params for the method call. Each one will be assigned
            // to an interior variable and then copied back onto the input array.
            List<ParameterExpression> outputs = new List<ParameterExpression>();
            
            // First port is the return value. Skip as a param.
            Expression[] paramsExps = new Expression[ports.Count - 1];
            List<Expression> blockExps = new List<Expression>();
            
            // Iterate through parameters of the method and construct either a 
            // mapping between args[] and the correct param position of the function
            // or a temp variable + assignment for `out` parameters. 
            for (int i = 0; i < ports.Count - 1; i++)
            {
                // Skip first port while reading (return value)
                NodePort port = ports[i + 1];

                if (port.isInput)
                {
                    // Input case: Foo(object o) -> Foo(arg[i]) 
                    Expression constExp = Expression.Constant(i, typeof(int));
                    Expression argExp = Expression.ArrayIndex(argsExp, constExp);
                    paramsExps[i] = Expression.Convert(argExp, port.type);
                }
                else 
                {
                    // Output case: Foo(out object o) -> object o; Foo(o); args[i] = o;

                    // Create a temp variable to store the out parameter
                    // (float f)
                    ParameterExpression outVariable = Expression.Variable(port.type);

                    // Add an assignment line that'll copy the temp variable back into args[i]
                    // (args[i] = f)
                    BinaryExpression assignment = Expression.Assign(
                        Expression.ArrayAccess(argsExp, Expression.Constant(i, typeof(int))),
                        Expression.Convert(outVariable, typeof(object))
                    );

                    outputs.Add(outVariable);
                    blockExps.Add(assignment);
                    paramsExps[i] = outVariable;
                }
            }
            
            // Add another output variable for return value of the method
            ParameterExpression ret = Expression.Variable(method.ReturnType, "ret");
            outputs.Add(ret);
            
            // Add the actual method call as the *first* expression in the block
            // after all the temp variable declarations have been made. 
            blockExps.Insert(0, Expression.Assign(ret, Expression.Call(method, paramsExps)));
            
            // Last expression in the block becomes the lambda output.
            // Make sure it's the result of the method.
            // TODO: This assumes all methods return a non-void. Which should
            // be a safe assumption for nodes, but some warning would be nice here.
            blockExps.Add(Expression.Convert(ret, typeof(object)));

            BlockExpression block = Expression.Block(outputs, blockExps);
            foreach (var exp in block.Expressions)
            {
                Debug.Log(exp);
            }
            
            // Finally compile the expression into a callable delegate
            LambdaExpression lambdaExp = Expression.Lambda(block, argsExp);
            m_Func = lambdaExp.Compile() as FuncWrapperDelegate;
            
            Debug.Log(lambdaExp);
            
            k_DelegateCache.Add(key, m_Func);
        }
    
        /// <summary>
        /// Execute the wrapper delegate with current inputs and cache results
        /// </summary>
        protected void ExecuteMethod()
        {
            if (m_Func == null)
            {
                // TODO: Better error handling
                throw new Exception($"[{name}] Delegate does not exist");
            }

            if (m_ArgsCache == null)
            {
                m_ArgsCache = new object[ports.Count - 1];
            }
            
            for (int i = 0; i < m_ArgsCache.Length; i++)
            {
                var port = ports[i + 1];

                if (port.isInput)
                {
                    m_ArgsCache[i] = GetInputValue<object>(port.portName);
                }
                else
                {
                    m_ArgsCache[i] = null;
                }
            }

            m_ReturnValue = m_Func(m_ArgsCache);
        }
    }
}
