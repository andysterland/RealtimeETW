using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OM;
using System.IO;
using System.Xml.Serialization;

namespace UnitTests
{
    [TestClass]
    public class ProgamTests
    {
        [TestMethod]
        public void CreateTestXmlFile()
        {
            Guid provider = Guid.Parse("{9e3b3947-ca5d-4614-91a2-7b624e0e7244}");
            string name = "Mshtml_DOM_CustomSiteEvent";
            string description = "{0} - Fired";
            string color = "red";

            EtwEventInfo etw = new EtwEventInfo(provider, name, description, color);
            List<EtwEventInfo> etwEvents = new List<EtwEventInfo>();
            etwEvents.Add(etw);
            Settings settings = new Settings(etwEvents); 

            XmlSerializer x = new XmlSerializer(settings.GetType());
            TextWriter textWriter = new StreamWriter(@"c:\temp\events.xml");
            x.Serialize(textWriter, settings);
            textWriter.Close();
        }
    }
}
