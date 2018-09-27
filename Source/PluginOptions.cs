using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Browser {
    /// <summary>
    ///     Plugin options will be loaded automatically when this is created.
    /// </summary>
    public static class PluginOptions {
        // this is used to load and save options to the correct folder
        private static readonly string optionsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Options.xml";

        // user-side options
        public static BrowserType BrowserType         = BrowserType.IE;

        // internals

        static PluginOptions() {
            if (File.Exists(optionsPath)) {
                LoadOptionsFromXml();
            }
        }

        private static void LoadOptionsFromXml() {
            var optionsXml = new XmlDocument();
            optionsXml.Load(optionsPath);
            foreach (XmlNode option in optionsXml.DocumentElement.SelectNodes("Option")) {
                string optionValue = option.Attributes["value"].Value;
                switch (option.Attributes["name"].Value) {
                    case nameof(BrowserType):
                        Enum.TryParse(optionValue, out BrowserType);
                        break;
                }
            }
        }

        public static void SaveOptionsToXml() {
            using (var writer = new XmlTextWriter(optionsPath, new UTF8Encoding()) {
                Formatting  = Formatting.Indented,
                Indentation = 4
            }) {
                writer.WriteStartDocument();
                writer.WriteComment("Serial plugin options");
                writer.WriteStartElement("Options");
                {
                    WriteOption(writer, nameof(BrowserType),         BrowserType.ToString("G"));
                }
                writer.WriteEndElement(); // options
                writer.WriteEndDocument();
            }
        }

        private static void WriteOption(XmlWriter writer, string nodeName, string nodeValue) {
            writer.WriteStartElement("Option");
            writer.WriteAttributeString("name",  nodeName);
            writer.WriteAttributeString("value", nodeValue);
            writer.WriteEndElement();
        }
    }

    public enum BrowserType {
        IE,
        Edge,
        Chrome
    }
}