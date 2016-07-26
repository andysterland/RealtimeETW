using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtwConsole
{
    public class EtwEventInfo
    {
        private string _description; 
        public EtwEventInfo()
        {

        }

        public EtwEventInfo(Guid ProviderGuid, string Name, string Description, string Color)
        {
            this.ProviderGuid = ProviderGuid;
            this.Name = Name;
            this.Description = Description;
            this.Color = Color;
        }

        public Guid ProviderGuid { set; get; }
        public string Name { set; get; }
        public string Description { set { this._description = value; } get { return String.Format(this._description, this.Name); } }
        public string Color { set; get; }
    }
}
