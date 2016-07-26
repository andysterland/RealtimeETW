using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Eventing.EventLib
{
    public struct PropertyItem
    {
        public string Key;
        public object Value;
        public PropertyItem(string k, object v)
        {
            Key = k;
            Value = v;
        }
    }

}
