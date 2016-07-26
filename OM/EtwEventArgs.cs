using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OM
{
    public class EtwEventArgs : EventArgs
    {
        public EtwEvent EtwEvent;

        public EtwEventArgs(EtwEvent EtwEvent)
        {
            this.EtwEvent = EtwEvent;
        }
    }
}
