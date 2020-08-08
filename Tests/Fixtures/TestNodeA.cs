
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
                direction = PortDirection.Input,
                type = typeof(int) 
            });
            
            AddPort(new Port
            { 
                name = "Output",
                direction = PortDirection.Output,
                type = typeof(int) 
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
