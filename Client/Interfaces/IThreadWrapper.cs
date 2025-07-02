using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Interfaces
{
    public interface IThreadWrapper
    {
        void Start(Action action);
        void Join();
    }
}
