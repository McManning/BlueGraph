
namespace BlueGraph.Tests
{
    public class TestNodeA : AbstractNode
    {
        [Input("Input")]
        public int aValue1 = 5;

        [Output("Output")]
        public int aValue2;
    
        public TestNodeA() : base()
        {
            name = "Test Node A";

            AddPort(new Port
            { 
                name = "Input",
                fieldName = "aValue1",
                isInput = true,
                Type = typeof(int) 
            });
            
            AddPort(new Port
            { 
                name = "Output",
                fieldName = "aValue2",
                isInput = false,
                Type = typeof(int) 
            });
        }

        /// <summary>
        /// Simply increments the input value by one
        /// </summary>
        public override object OnRequestValue(Port port)
        {
            var a = GetInputValue("Input", aValue1);
            return a + 1;
        }
    }

    public class InheritedTestNodeA : TestNodeA
    {
        
    }
}
