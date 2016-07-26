using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace EtwConsole
{
    public class Settings
    {
        public const string FileName = "events.xml"; // path is relative
        public static Settings Instance = null;
        public static void Init()
        {
            if (File.Exists(FileName))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                TextReader textReader = new StreamReader(FileName);
                Settings settings = (Settings)deserializer.Deserialize(textReader);
                textReader.Close();
                Settings.Instance = settings;
            }
            else
            {
                Settings.Instance = new Settings();
            }
        }


        public Settings()
        {

        }

        public Settings(List<EtwEventInfo> EtwEvents)
        {
            this.EtwEvents = EtwEvents;
        }

        public List<EtwEventInfo> EtwEvents;
    }
}
