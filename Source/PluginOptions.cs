using System;
using System.Collections.Generic;
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
        private static readonly string pluginPath  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
        private static readonly string optionsPath = pluginPath + "Options.xml";
        private static readonly bool   x64         = Environment.Is64BitOperatingSystem;

        internal static string WebDriverDirectory => pluginPath + "webdrivers\\";

        internal static readonly Dictionary<BrowserType, string> WebDriverFileNames = new Dictionary<BrowserType, string> {
            { BrowserType.IE, x64 ? "IEDriverServer64.exe" : "IEDriverServer32.exe" },
            { BrowserType.Edge, "EdgeWebDriver.exe" },
            { BrowserType.Chrome, "ChromeDriver.exe" }
        };

        // user-side options
        public static BrowserType BrowserType = BrowserType.None;

        public static bool LaunchAtStartup;
        public static bool LaunchHidden = true;

        public static int BrowserWindowX;
        public static int BrowserWindowY;
        public static int BrowserWindowW;
        public static int BrowserWindowH;

        // internals

        static PluginOptions() {
            if (File.Exists(optionsPath)) {
                LoadOptionsFromXml();
            }
        }

        private static void LoadOptionsFromXml() {
            var optionsXml = new XmlDocument();
            optionsXml.Load(optionsPath);
            XmlNodeList options = optionsXml.DocumentElement?.SelectNodes("Option");
            if (options != null) {
                foreach (XmlNode option in options) {
                    string optionValue = option?.Attributes["value"].Value;
                    switch (option.Attributes["name"].Value) {
                        case nameof(BrowserType):
                            Enum.TryParse(optionValue, out BrowserType);
                            break;
                        case nameof(LaunchAtStartup):
                            bool.TryParse(optionValue, out LaunchAtStartup);
                            break;
                        case nameof(LaunchHidden):
                            bool.TryParse(optionValue, out LaunchHidden);
                            break;
                        //
                        case nameof(BrowserWindowX):
                            int.TryParse(optionValue, out BrowserWindowX);
                            break;
                        case nameof(BrowserWindowY):
                            int.TryParse(optionValue, out BrowserWindowY);
                            break;
                        case nameof(BrowserWindowW):
                            int.TryParse(optionValue, out BrowserWindowW);
                            break;
                        case nameof(BrowserWindowH):
                            int.TryParse(optionValue, out BrowserWindowH);
                            break;
                    }
                }
            }
        }

        public static void SaveOptionsToXml() {
            using (var writer = new XmlTextWriter(optionsPath, new UTF8Encoding()) {
                Formatting  = Formatting.Indented,
                Indentation = 4
            }) {
                writer.WriteStartDocument();
                writer.WriteComment(nameof(Browser) + " plugin options");
                writer.WriteStartElement("Options");
                {
                    WriteOption(writer, nameof(BrowserType),     BrowserType.ToString("G"));
                    WriteOption(writer, nameof(LaunchAtStartup), LaunchAtStartup.ToString());
                    WriteOption(writer, nameof(LaunchHidden),    LaunchHidden.ToString());
                    //
                    WriteOption(writer, nameof(BrowserWindowX), BrowserWindowX.ToString());
                    WriteOption(writer, nameof(BrowserWindowY), BrowserWindowY.ToString());
                    WriteOption(writer, nameof(BrowserWindowW), BrowserWindowW.ToString());
                    WriteOption(writer, nameof(BrowserWindowH), BrowserWindowH.ToString());
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
        None,
        IE,
        Edge,
        Chrome,
        Firefox
    }
}