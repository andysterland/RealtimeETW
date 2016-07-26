using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Eventing.EventLib;

namespace OM
{
    public class EtwEventInfo
    {
        public EtwEventInfo()
        {

        }

        public EtwEventInfo(Guid ProviderGuid, string Task, string Description, string Color)
        {
            this.ProviderGuid = ProviderGuid;
            this.Task = Task;
            this.DescriptionFormat = Description;
            this.Color = Color;
        }

        public EtwEvent GetEtwEvent(uint PID, string Name, List<PropertyItem> Properties, DateTime Timestamp)
        {
            string etwProperty = DescriptionFormat;
            foreach(PropertyItem prop in Properties)
            {   
                string propToken = "{" + prop.Key + "}";
                if (etwProperty.Contains(propToken, StringComparison.InvariantCultureIgnoreCase))
                {
                    etwProperty = etwProperty.Replace(propToken, (string)prop.Value);
                }
            }
            EtwEvent etw = new EtwEvent(PID, Name, etwProperty, Timestamp);

            return etw;
        }

        public Guid ProviderGuid { set; get; }
        public string Task { set; get; }
        public string DescriptionFormat { set; get; }
        public string Color { set; get; }
    }
}
