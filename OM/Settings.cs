using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace OM
{
    public class Settings
    {
        public const string FileName = "events.xml"; // path is relative
        public static Settings LoadFromFile(string FileName)
        {
            if (File.Exists(FileName))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                TextReader textReader = new StreamReader(FileName);
                Settings settings = (Settings)deserializer.Deserialize(textReader);
                textReader.Close();
                return settings;
            }
            return null;
        }

        public static void SaveToFile(string FileName, Settings Settings )
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            TextWriter textWriter = new StreamWriter(FileName);
            serializer.Serialize(textWriter, Settings);
            textWriter.Close();
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
