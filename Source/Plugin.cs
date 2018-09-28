using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using PluginInterface;

namespace Browser {
    public class Plugin : IPlugin {
        private static IWebDriver webDriver;
        private static IWebElement currentElement; // TODO: use currentElement in actions

        internal static CtlMain MainCtl;

        #region Required plugin attributes

        // Declarations of all our internal plugin variables.
        // VoxCommando expects these to be here so don't remove them.

        // Properties binded to their definitions in
        // assembly information to simplify development.
        // Change properties in: (Visual Studio)
        // Project -> <Project> properties -> Application -> Assembly Information

        private static readonly Assembly pluginAsm = Assembly.GetExecutingAssembly();

        public string Name { get; } = nameof(Browser);

        public string Version { get; } = pluginAsm.GetName().Version.ToString();

        public string Description { get; } =
            pluginAsm
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault()?.Description;

        public string Author { get; } =
            pluginAsm
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)
                .OfType<AssemblyCompanyAttribute>()
                .FirstOrDefault()?.Company;

        public bool GenEvents { get; } = true;

        internal static IPluginHost HostInstance;

        public IPluginHost Host {
            get { return HostInstance; }
            set {
                HostInstance = value;
                MainCtl = (CtlMain)MainInterface;
            }
        }

        public UserControl MainInterface { get; }

        #endregion

        public Plugin() {
            MainCtl = new CtlMain();
            MainInterface = MainCtl;
        }

        /// <summary>
        ///     First function called by the host.
        ///     Put anything needed to start with here first.
        ///     At this point the host object exists.
        ///     This function is called in a thread so be careful to handle all errors.
        ///     e.g. udpListener.myHost = this.Host;
        /// </summary>
        public void Initialize() {
            StartWebDriver();
        }

        /// <summary>
        ///     This is the main method called by VoxCommando when performing plugin actions. All actions go through here.
        /// </summary>
        /// <param name="actionNameArray">
        ///     An array of strings representing action name. Example action: xbmc.send >>>
        ///     actionNameArray[0] is the plugin name (xbmc), actionNameArray[1] is "send".
        /// </param>
        /// <param name="actionParameters">An array of strings representing our action parameters.</param>
        /// <returns>an actionResult</returns>
        public actionResult doAction(string[] actionNameArray, string[] actionParameters) {
            var ar = new actionResult();
            const string unknownAction = "Unknown " + nameof(Browser) + " plugin action.";
            if (actionNameArray.Length < 2) {
                ar.setError(unknownAction);
                return ar;
            }
            string actionName1 = actionNameArray[1].ToUpper();
            if (webDriver == null
             && actionName1 != "SETUP") {
                ar.setError("No web browser opened or selected.");
                return ar;
            }
            try {
                switch (actionName1) {
                    // TODO: finish command list
                    // TODO: check for updates for all web driver executables.
                    case "GOTOURL": {
                        break;
                    }
                    // window state and location
                    case "SIZE": {
                        // get
                        if (actionParameters.Length == 0) {
                            var size = webDriver.Manage().Window.Size;
                            ar.setSuccess($"{size.Width},{size.Height}");
                        }
                        // set
                        else if (actionParameters.Length == 2) {
                            int width;
                            if (!int.TryParse(actionParameters[0], out width)) {
                                ar.setError("Invalid window width.");
                            }
                            int height;
                            if (!int.TryParse(actionParameters[1], out height)) {
                                ar.setError("Invalid window height.");
                            }
                            webDriver.Manage().Window.Size = new Size(width, height);
                        }
                        else {
                            ar.setError("Invalid parameters count.");
                        }
                        break;
                    }
                    case "LOCATION": {
                        // get
                        if (actionParameters.Length == 0) {
                            var position = webDriver.Manage().Window.Position;
                            ar.setSuccess($"{position.X},{position.Y}");
                        }
                        // set
                        else if (actionParameters.Length == 2) {
                            int x;
                            if (!int.TryParse(actionParameters[0], out x)) {
                                ar.setError("Invalid X position.");
                            }
                            int y;
                            if (!int.TryParse(actionParameters[1], out y)) {
                                ar.setError("Invalid Y position.");
                            }
                            webDriver.Manage().Window.Position = new Point(x, y);
                        }
                        else {
                            ar.setError("Invalid parameters count.");
                        }
                        break;
                    }
                    case "MAXIMIZE": {
                        webDriver.Manage().Window.Maximize();
                        break;
                    }
                    case "MINIMIZE": {
                        webDriver.Manage().Window.Minimize();
                        break;
                    }
                    case "FULLSCREEN": {
                        webDriver.Manage().Window.FullScreen();
                        break;
                    }
                    // manipulating tabs
                    case "SWITCHTAB": {
                        SwitchWindow(actionParameters[0]);
                        break;
                    }
                    case "CLOSETAB": {
                        webDriver.Close();
                        break;
                    }
                    case "NEWTAB": {
                        // SendKeys?
                        break;
                    }
                    case "GETTABS": {
                        //webDriver.
                        break;
                    }
                    // start/stop
                    case "START": {
                        StartWebDriver();
                        ar.setInfo("Web driver started.");
                        break;
                    }
                    case "STOP": {
                        webDriver.Quit();
                        ar.setInfo("Web driver stopped working.");
                        break;
                    }
                    default: {
                        ar.setError(unknownAction);
                        break;
                    }
                }
            }
            catch (Exception ex) {
                ar.setError(ex.ToString());
            }

            return ar;
        }

        /// <summary>
        ///     Releases all unmanaged resources (which implements IDisposable)
        /// </summary>
        public void Dispose() {
            // You must release all unmanaged resources here when program is stopped to prevent the memory leaks.
        }

        #region Other methods

        internal static void StartWebDriver() {
            switch (PluginOptions.BrowserType) {
                case BrowserType.IE: {
                    webDriver = new InternetExplorerDriver();
                    break;
                }
                case BrowserType.Edge: {
                    webDriver = new EdgeDriver();
                    break;
                }
                case BrowserType.Chrome: {
                    webDriver = new ChromeDriver();
                    break;
                }
                default: {
                    MessageBox.Show(nameof(Browser) + " plugin error: Invalid browser selected.");
                    break;
                }
            }
        }

        internal static bool SwitchWindow(string titleSubstring) {
            string currentWindow = webDriver.CurrentWindowHandle;
            var availableWindows = new List<string>(webDriver.WindowHandles);

            foreach (string w in availableWindows) {
                if (w != currentWindow) {
                    webDriver.SwitchTo().Window(w);
                    if (webDriver.Title.Contains(titleSubstring))
                        return true;

                    webDriver.SwitchTo().Window(currentWindow);

                }
            }
            return false;
        }

        #endregion
    }
}