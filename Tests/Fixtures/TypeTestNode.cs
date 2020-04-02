
using UnityEngine;

namespace BlueGraph.Tests
{
    public interface ITestClass
    {

    }

    public class BaseTestClass : ITestClass
    {
        public float value1;
        public float value2;
    }

    public class TestClass : BaseTestClass
    {
        public float value3;
    }

    public struct TestStruct
    {
        public float value1;
        public float value2;
    }

    /// <summary>
    /// Test fixture with a bunch of type options to check GetInputValue calls
    /// </summary>
    public class TypeTestNode : AbstractNode
    {
        public int intValue;
        public bool boolValue;
        public string stringValue;
        public float floatValue;
        public Vector3 vector3Value;
        public AnimationCurve curveValue;

        public TestClass testClassValue = new TestClass();
        public TestStruct testStructValue;
    
        public TypeTestNode() : base()
        {
            name = "Type Test Node";
            
            // Input (any)
            AddPort(new Port { isInput = true, name = "Input" });

            // Output types
            AddPort(new Port { name = "intval" });
            AddPort(new Port { name = "boolval" });
            AddPort(new Port { name = "stringval" });
            AddPort(new Port { name = "floatval" });
            AddPort(new Port { name = "vector3val" });
            AddPort(new Port { name = "curveval" });
            AddPort(new Port { name = "classval" });
            AddPort(new Port { name = "structval" });
        }

        public override object OnRequestValue(Port port)
        {
            switch (port.name) 
            {
                case "intval": return intValue;
                case "boolval": return boolValue;
                case "stringval": return stringValue;
                case "vector3val": return vector3Value;
                case "curveval": return curveValue;
                case "classval": return testClassValue;
                case "structval": return testStructValue;
                default: return null;
            }
        }
    }
}
