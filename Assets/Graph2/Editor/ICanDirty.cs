using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph2
{
    public interface ICanDirty
    {
        void OnDirty();
        void OnUpdate();
    }
}
