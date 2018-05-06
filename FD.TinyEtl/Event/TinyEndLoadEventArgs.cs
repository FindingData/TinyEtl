using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FD.TinyEtl
{
    public class TinyEndLoadEventArgs : EventArgs
    {
        public object Source { get; set; }
    }
}
