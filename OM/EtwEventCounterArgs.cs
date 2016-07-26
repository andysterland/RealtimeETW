using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OM
{
    public class EtwEventCounterArgs: EventArgs
    {
        public int TotalCount;
        public int FilteredCount;
        public DateTime Timestamp;

        public EtwEventCounterArgs(int TotalCount, int FilteredCount, DateTime Timestamp)
        {
            this.TotalCount = TotalCount;
            this.FilteredCount = FilteredCount;
            this.Timestamp = Timestamp;
        }
    }
}
