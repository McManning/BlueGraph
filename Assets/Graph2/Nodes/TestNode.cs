using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
{
    public class TestNode : AbstractNode
    {
        public float x;
        public float y;

        public float result;

        public override object GetOutput(string name)
        {
            return x + y;
        }
    }
}
