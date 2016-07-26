using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OM
{
    public class EtwEvent
    {
        public uint PID;
        public string Name;
        public string Text;
        public DateTime Timestamp;

        public EtwEvent(uint PID, string Name, string Text, DateTime Timestamp)
        {
            this.PID = PID;
            this.Name = Name;
            this.Text = Text;
            this.Timestamp = Timestamp;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
