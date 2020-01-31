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
        static Dictionary<string, Func<object[], object>> k_DelegateCache = new Dictionary<string, Func<object[], object>>();

        object[] m_ArgsCache;
        
        // TODO: Not public (but still serialized). Needed by NodeReflection but 
        // I can probably just call CreateLambda directly.
        public string className;
        public string methodName;
        public bool hasReturnValue;

        // Number of arguments to pass into the method.
        // TODO: This is kind of a hack to support adding new ports via a class interface
        // that are in addition to the standard wrapped method ports. 
        public int argCount; 
        
        Func<object[], object> m_Func;
        
        object m_ReturnValue;

        public virtual void Awake()
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

            // Return value is always the LAST port in the list
            // TODO: Technically a slot in args - we just don't write back. Should we?
            // Can then eliminate this check here and just use the loop below. 
            // Can't assume it's called "Result" either, could be anything.
            // Also - not necessarily last port if we add ports via [Output]
            if (name == ports[ports.Count - 1].name)
            {
                return m_ReturnValue;
            }

            // Other out argument, find matching index
            for (int i = 0; i < ports.Count - 1; i++) 
            {
                if (!ports[i].isInput && ports[i].name == name)
                {
                    return m_ArgsCache[i];
                }
            }
            
            // TODO: Better error handling
            throw new Exception($"<b>[{this.name}]</b> Missing requested output slot {name}");
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
                try
                {
                    Type classType = Type.GetType(className);
                    if (classType == null)
                    {
                        throw new Exception($"Missing class {className}");
                    }

                    MethodInfo method = classType.GetMethod(methodName);
                    if (method == null)
                    {
                        throw new Exception($"Missing method {className}.{methodName}");
                    }

                    CreateDelegate(method);
                } 
                catch (Exception e)
                {
                    Debug.LogError($"<b>[{name}]</b> Failed to create delegate: {e}");
                }
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
            hasReturnValue = method.ReturnType != typeof(void);

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
            
            int paramsLen = method.GetParameters().Length;

            Expression[] paramsExps = new Expression[paramsLen];
            List<Expression> blockExps = new List<Expression>();
            
            // Iterate through parameters of the method and construct either a 
            // mapping between args[] and the correct param position of the function
            // or a temp variable + assignment for `out` parameters. 
            for (int i = 0; i < paramsLen; i++)
            {
                Port port = ports[i];

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

                    Debug.Log($"Out: {port.name} of type {port.type}");

                    // Create a temp variable to store the out parameter
                    // (float f)
                    ParameterExpression outVariable = Expression.Variable(port.type, port.fieldName);

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
            
            if (hasReturnValue)
            {
                // Add another output variable for return value of the method
                ParameterExpression ret = Expression.Variable(method.ReturnType, "ret");
                outputs.Add(ret);
            
                // Add the actual method call as the *first* expression in the block
                // after all the temp variable declarations have been made. 
                blockExps.Insert(0, Expression.Assign(ret, Expression.Call(method, paramsExps)));
            
                // Last expression in the block becomes the lambda output.
                // Make sure it's the result of the method.
                blockExps.Add(Expression.Convert(ret, typeof(object)));
            }
            else
            {
                // Return void, just insert the method call at the beginning with no assignment
                blockExps.Insert(0, Expression.Call(method, paramsExps));
                
                // TODO: Ideally, we don't do this fake integer assignment here
                // call it as an Action<object[]>. It'd require some refactoring though.
                ParameterExpression ret = Expression.Variable(typeof(int), "ret");
                outputs.Add(ret);
                
                blockExps.Add(Expression.Assign(ret, Expression.Constant(0)));
                blockExps.Add(Expression.Convert(ret, typeof(object)));
            }
            
            BlockExpression block = Expression.Block(outputs, blockExps);
            foreach (var exp in block.Expressions)
            {
                Debug.Log(exp);
            }
            
            // Finally compile the expression into a callable delegate
            LambdaExpression lambdaExp = Expression.Lambda(block, argsExp);
            
            m_Func = (Func<object[], object>)lambdaExp.Compile();
            
            Debug.Log(lambdaExp);
            Debug.Log(m_Func);
            
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

            int argsLen = hasReturnValue ? ports.Count - 1 : ports.Count;

            if (m_ArgsCache == null)
            {
                m_ArgsCache = new object[argsLen];
            }
            
            for (int i = 0; i < argsLen; i++)
            {
                var port = ports[i];

                if (port.isInput)
                {
                    object value = GetInputValue(port.name);
                    if (value != null)
                    {
                        try
                        {
                            m_ArgsCache[i] = Convert.ChangeType(value, port.type);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(
                                $"<b>[{name}]</b>: Cannot convert input port " +
                                $"`{port.name}` value to type `{port.type}`"
                            );
                        }
                    }
                    // TODO: if value is null and port.type is a value type, ChangeType will
                    // throw an exception. How do we handle this case? Error out the node?
                }
                else
                {
                    m_ArgsCache[i] = null;
                }
            }
            
            try
            {
                m_ReturnValue = m_Func(m_ArgsCache);
            } 
            catch (Exception e)
            {
                Debug.LogError($"<b>[{name}]</b>: Delegate exception: {e}");
                // TODO: Don't LogError, just throw an error status onto the node UI
                // (done in the view during execution - not the nodes themselves?)
            }
        }
    }
}
