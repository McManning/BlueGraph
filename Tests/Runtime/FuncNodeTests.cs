using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace BlueGraph.Tests
{ 
    public static class TestFuncs
    {
        public static float OneArgFunc(float f)
        {
            return f + 1f;
        }

        public static float TwoArgFunc(float a, float b)
        {
            return a + b;
        }

        public static int Max(int x, int y)
        {
            return Math.Max(x, y);
        }

        public static float ExceptionThrower(float x)
        {
            throw new Exception($"Expected exception. x = {x}");
        }
    }

    public class FuncNodeTests
    {
        [Test]
        public void CanBindOneArg()
        {
            var mi = typeof(TestFuncs).GetMethod("OneArgFunc");
            var node = new FuncNode(mi);
            
            Assert.AreEqual(2, node.Ports.Count);
            Assert.AreSame(typeof(float), node.GetPort("f").Type);
            Assert.AreSame(typeof(float), node.GetPort("Result").Type);
        }
        
        [Test]
        public void CanBindTwoArgs()
        {
            var mi = typeof(TestFuncs).GetMethod("TwoArgFunc");
            var node = new FuncNode(mi);
            
            Assert.AreEqual(3, node.Ports.Count);
            Assert.AreSame(typeof(float), node.GetPort("a").Type);
            Assert.AreSame(typeof(float), node.GetPort("b").Type);
            Assert.AreSame(typeof(float), node.GetPort("Result").Type);
        }
        
        [Test]
        public void CanBindThreeArgs()
        {
            var mi = typeof(Mathf).GetMethod("Lerp");
            var node = new FuncNode(mi);
            
            Assert.AreEqual(4, node.Ports.Count);
            Assert.AreSame(typeof(float), node.GetPort("a").Type);
            Assert.AreSame(typeof(float), node.GetPort("b").Type);
            Assert.AreSame(typeof(float), node.GetPort("t").Type);
            Assert.AreSame(typeof(float), node.GetPort("Result").Type);
        }
        
        [Test]
        public void CheckStack()
        {
            var mi = typeof(TestFuncs).GetMethod("ExceptionThrower");
            var node = new FuncNode(mi);
            
            // This'll throw. I just want to see the stack.
            float result = node.GetOutputValue<float>("Result");

            Assert.AreEqual(2, node.Ports.Count);
        }
        
        [Test]
        public void CallPerformanceTest()
        {
            // Setup is slow, but calls should be faster than 
            // the lambda wrapper/boxing method. 
            // .. in theory.
            
            var mi = typeof(TestFuncs).GetMethod("Max");
            var node = new FuncNode(mi);
            
            var boxedParams = new object[] { 1, 5 };

            int iterations = 3000000;
            
            // ---- direct call ----

            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int result = TestFuncs.Max(1, 5);
            }
            sw1.Stop();

            // ---- direct delegate ----

            var sw2 = Stopwatch.StartNew();
            var delegateFunc = (Func<int, int, int>)mi.CreateDelegate(typeof(Func<int, int, int>));
            for (int i = 0; i < iterations; i++)
            {
                int result = delegateFunc(1, 5);
            }
            sw2.Stop();

            // ---- Boxing the delegate ----
            var sw3 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                // object result = (object)delegateFunc... 30ms
                // int result = delegateFunc... 4ms 
                // Boxing is slow (copy to heap), unboxing isn't as slow 
                // (since the ptr is already in the "box" on the heap)
                // that's why the arguments are fine but going float -> object
                // is slow af. 
                object result = (object)delegateFunc((int)boxedParams[0], (int)boxedParams[1]);
            }
            sw3.Stop();
            
            // ---- Invoke ---- 

            var sw4 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int result = (int)mi.Invoke(null, boxedParams);
            }
            sw4.Stop();

            // ---- Boxed wrapper lambda ----
            
            var sw5 = Stopwatch.StartNew();
            Func<object[], object> boxedWrapper = (object[] p) => delegateFunc((int)p[0], (int)p[1]);

            for (int i = 0; i < iterations; i++)
            {
                int result = (int)boxedWrapper(boxedParams);
            }
            sw5.Stop();

            
            // ---- via func node ----

            var sw6 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int result = node.GetOutputValue<int>("Result");
            }
            sw6.Stop();

            // ----- via resolver ----
            
            var sw7 = Stopwatch.StartNew();
            /*var port = new OutputResolverPort(typeof(int));
            port.SetResolver(() => Math.Max((int)boxedParams[0], (int)boxedParams[1]));
            
            for (int i = 0; i < iterations; i++)
            {
                int result = port.GetValue<int>();
            }*/
            sw7.Stop();
            
            Debug.Log($"Direct Time -         {sw1.Elapsed.TotalMilliseconds} ms");
            Debug.Log($"Delegate Time -       {sw2.Elapsed.TotalMilliseconds} ms");
            Debug.Log($"Boxed Delegate Time - {sw3.Elapsed.TotalMilliseconds} ms");
            Debug.Log($"Invoke Time -         {sw4.Elapsed.TotalMilliseconds} ms");
            Debug.Log($"Boxed Wrapper Time -  {sw5.Elapsed.TotalMilliseconds} ms");
            Debug.Log($"Node Time -           {sw6.Elapsed.TotalMilliseconds} ms");
            Debug.Log($"Direct Resolver -     {sw7.Elapsed.TotalMilliseconds} ms");

            
		    // Conclusion.
		    double factor2 = (double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds;
		    double factor3 = (double)sw3.ElapsedMilliseconds / sw1.ElapsedMilliseconds;
		    double factor4 = (double)sw4.ElapsedMilliseconds / sw1.ElapsedMilliseconds;
		    double factor5 = (double)sw5.ElapsedMilliseconds / sw1.ElapsedMilliseconds;
		    double factor6 = (double)sw6.ElapsedMilliseconds / sw1.ElapsedMilliseconds;
		    double factor7 = (double)sw7.ElapsedMilliseconds / sw1.ElapsedMilliseconds;

		    Debug.Log("---------------------");
		    Debug.Log($"Delegate slower times:          {factor2:0.00}");
		    Debug.Log($"Boxed Delegate slower times:    {factor3:0.00}");
		    Debug.Log($"Invoke slower times:            {factor4:0.00}");
		    Debug.Log($"Boxed Wrapper slower times:     {factor5:0.00}");
		    Debug.Log($"Node slower times:              {factor6:0.00}");
		    Debug.Log($"Direct Resolver slower times:   {factor7:0.00}");
        }
    }
}
