using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace BlueGraph
{
    /// <summary>
    /// Indicate that a class contains a suite of FuncNode methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FuncNodeModuleAttribute : Attribute
    {
        /// <summary>
        /// Default category for all contained FuncNode methods.
        /// Can be slash-delimited to denote subcategories. 
        /// </summary>
        public string category;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class FuncNodeAttribute : Attribute
    {
        /// <summary>
        /// Custom display name of the FuncNode
        /// </summary>
        public string name;

        /// <summary>
        /// Optional category name to override FuncNodeModule's category.
        /// Can be slash-delimited to denote subcategories.
        /// </summary>
        public string category;

        public FuncNodeAttribute(string name = null)
        {
            this.name = name;    
        }
    }

    /// <summary>
    /// Node that exposes a pure static method to the graph
    /// </summary>
    public class FuncNode : AbstractNode
    {
        [Serializable]
        public class Parameter : ISerializationCallbackReceiver
        {
            public string name;
            public Type type;
            public bool isOut;

            [NonSerialized]
            public object value;

            [SerializeField]
            string m_TypeName;

            public void OnAfterDeserialize()
            {
                type = Type.GetType(m_TypeName);
            }

            public void OnBeforeSerialize()
            {
                m_TypeName = type.FullName;
            }
        }
        
        [NonSerialized]
        Dictionary<string, object> m_OutputCache = new Dictionary<string, object>();
        
        // TODO: Not public 
        public string methodClass;
        public string methodName;
        
        Func<object[], object> m_Func;

        object m_ReturnValue;

        public void Awake()
        {
            CheckLambda();
        }

        /// <summary>
        /// Execute the wrapped method and get the named output value
        /// </summary>
        public override object GetOutput(string name)
        {
            // TODO: Don't re-execute unless we dirty inputs
            ExecuteMethod();
            
            if (!m_OutputCache.ContainsKey(name))
            {
                // TODO: Better error handling
                Debug.LogError($"[{this.name}] Missing output slot {name}");
                return null;
            }

            return m_OutputCache[name];
        }

        /// <summary>
        /// Create ports and a wrapper lambda for the given method
        /// </summary>
        /// <param name="method"></param>
       /* protected void BindMethod(MethodInfo method)
        {
            m_MethodClass = method.DeclaringType.FullName;
            m_MethodName = method.Name;

            FuncNodeAttribute attr = method.GetCustomAttribute<FuncNodeAttribute>();
            name = attr.name ?? method.Name;

            ParameterInfo[] parameters = method.GetParameters();

            // TODO: Don't clear. Diff and merge intelligently.
            ports.Clear();
            m_TypeCache.Clear();
            
            // Add an output port for the return value
            // TODO: IFF there actually is one. 
            ports.Add(new NodePort() {
                node = this,
                portName = "result",
                isMulti = true,
                isInput = false
            });

            m_TypeCache.Add(method.ReturnType);

            foreach (var parameter in parameters)
            {
                ports.Add(new NodePort() {
                    node = this,
                    portName = parameter.Name,
                    isMulti = parameter.IsOut,
                    isInput = !parameter.IsOut
                });

                m_TypeCache.Add(parameter.IsOut ? 
                    parameter.ParameterType.GetElementType() :
                    parameter.ParameterType
                );
            }
            
            CreateLambda(method);
        } */
        
        protected void CheckLambda()
        {
            // Re-bind to the method if necessary
            if (methodName != null && methodClass != null)
            {
                // TODO: This is going to probably be slow due to each node
                // wanting to load it. Probably not cached well.
                MethodInfo method = Type.GetType(methodClass).GetMethod(methodName);
                CreateLambda(method);
            }
        }
        
        /// <summary>
        /// Construct a lambda expression to call the wrapped method
        /// </summary>
        protected void CreateLambda(MethodInfo method)
        {
            // TODO: Caching so that we don't have duplicate lambdas for 
            // multiple instances of the same func

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
            
            This will accept an object[] containing inputs and output placeholders
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
            m_Func = (Func<object[], object>)lambdaExp.Compile();
            
            Debug.Log(lambdaExp);
        }
    
        /// <summary>
        /// Execute the wrapper delegate with current inputs and cache results
        /// </summary>
        protected void ExecuteMethod()
        {
            if (m_Func == null)
            {
                // TODO: Better error handling
                Debug.LogError($"[{name}] Delegate does not exist");
                return;
            }

            object[] args = new object[ports.Count - 1];
            for (int i = 0; i < args.Length; i++)
            {
                var port = ports[i + 1];

                if (port.isInput)
                {
                    args[i] = GetInputValue<object>(port.portName);
                }
                else
                {
                    args[i] = null;
                }
            }

            m_ReturnValue = m_Func(args);

            // Copy outputs back into our cache
            // TODO: This step seems redundant. Why not keep args @ the class level
            m_OutputCache.Clear();
            for (int i = 0; i < args.Length; i++)
            {
                var port = ports[i + 1];
                if (!port.isInput)
                {
                    m_OutputCache[port.portName] = args[i];
                }
            }
        }
    }
}
