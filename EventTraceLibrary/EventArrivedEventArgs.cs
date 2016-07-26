//------------------------------------------------------------------------------
// http://code.msdn.microsoft.com/EventTraceWatcher
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Eventing.EventLib
{
    public sealed class EventArrivedEventArgs : EventArgs
    {
        internal static List<PropertyItem> EmptyProperty = new List<PropertyItem>();

        public EventArrivedEventArgs(Exception exception)
            : this(Guid.Empty, string.Empty, EmptyProperty)
        {
            this.EventException = exception;
        }

        public EventArrivedEventArgs(Guid providerId, string eventName, IList<PropertyItem> properties)
        {
            this.EventName = eventName;
            this.ProviderId = providerId;
            this.Properties = properties;
        }

        public Exception EventException
        {
            private set;
            get;
        }

        public string EventName
        {
            private set;
            get;
        }

        public IList<PropertyItem> Properties
        {
            private set;
            get;
        }

        public Guid ProviderId
        {
            private set;
            get;
        }

        public string Message
        {
            set;
            get;
        }

        public DateTime LogDateTime
        {
            set;
            get;
        }

        public string Task
        {
            set;
            get;
        }

        public int Level
        {
            set;
            get;
        }

        public string LevelName
        {
            set;
            get;
        }

        public uint ProcessId
        {
            set;
            get;
        }

        public uint ThreadId
        {
            set;
            get;
        }

    }
}
