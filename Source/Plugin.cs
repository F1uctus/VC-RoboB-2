using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using PluginInterface;

namespace Browser {
    public class Plugin : IPlugin {
        private static IWebDriver webDriver;
        private static bool isHidden;
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
            if (PluginOptions.LaunchAtStartup) {
                StartWebDriver(PluginOptions.LaunchHidden);
            }
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
                    // ReSharper disable PossibleNullReferenceException
                    case "GOTOURL": {
                        if (actionParameters.Length == 0) {
                            ar.setError("'Url' parameter missing.");
                        }
                        else {
                            if (!actionParameters[0].StartsWith(""))
                            try {
                                webDriver.Navigate().GoToUrl(actionParameters[0]);
                            }
                            catch (Exception ex) {
                                ar.setError("Failed to open specified URL:\n" + ex);
                                break;
                            }
                            ar.setInfo("OK.");
                        }
                        break;
                    }

                    // window size and location
                    case "SIZE": {
                        // get
                        if (actionParameters.Length == 0) {
                            Size size = webDriver.Manage().Window.Size;
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
                            Point position = webDriver.Manage().Window.Position;
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

                    // change window state
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

                    #region Working with tabs

                    case "SELECTTAB": {
                        if (actionParameters.Length == 0) {
                            ar.setError("'Tab name' parameter missing.");
                        }
                        else {
                            if (SelectWindow(actionParameters[0])) {
                                ar.setInfo("OK.");
                            }
                            else {
                                ar.setError("Cannot find tab with specified name.");
                            }
                        }
                        break;
                    }

                    case "CLOSETAB": {
                        webDriver.Close();
                        break;
                    }

                    case "NEWTAB": {
                        //var a = Keys.Control | Keys.T;
                        //webDriver.FindElement(By.XPath("body")).SendKeys(a.ToString());
                        // SendKeys?
                        break;
                    }

                    case "GETTABS": {
                        ar.setSuccess(string.Join("\n", webDriver.WindowHandles));
                        break;
                    }

                    #endregion

                    #region Manipulating browsers

                    case "START": {
                        bool headless = PluginOptions.LaunchHidden;
                        if (actionParameters.Length > 0
                         && !bool.TryParse(actionParameters[0], out headless)) {
                            ar.setError("Invalid value for 'hidden' parameter.");
                            break;
                        }

                        try {
                            StartWebDriver(headless);
                        }
                        catch (Exception ex) {
                            ar.setError("Failed to start browser:\n" + ex);
                            break;
                        }
                        ar.setInfo("OK.");
                        break;
                    }

                    case "STOP": {
                        webDriver.Quit();
                        ar.setInfo("Web driver stopped working.");
                        break;
                    }

                    case "SWITCH": {
                        if (actionParameters.Length == 0) {
                            ar.setError("'Browser type' parameter missing.");
                        }
                        else {
                            BrowserType browserType;
                            if (Enum.TryParse(actionParameters[0], true, out browserType)) {
                                try {
                                    webDriver.Quit();
                                    webDriver.Dispose();
                                    PluginOptions.BrowserType = browserType;
                                    StartWebDriver(isHidden);
                                }
                                catch (Exception ex) {
                                    ar.setError($"Failed to switch to {browserType:G}:\n" + ex);
                                    break;
                                }
                                ar.setInfo("OK.");
                            }
                            else {
                                ar.setError("Invalid browser type.");
                            }
                        }
                        break;
                    }

                    #endregion

                    default: {
                        ar.setError(unknownAction);
                        break;
                    }
                    // ReSharper restore PossibleNullReferenceException
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
            if (webDriver != null) {
                PluginOptions.BrowserWindowX = webDriver.Manage().Window.Position.X;
                PluginOptions.BrowserWindowY = webDriver.Manage().Window.Position.Y;
                PluginOptions.BrowserWindowW = webDriver.Manage().Window.Size.Width;
                PluginOptions.BrowserWindowH = webDriver.Manage().Window.Size.Height;

                for (int i = 0; i < webDriver?.WindowHandles.Count; i++) {
                    webDriver.Close();
                }
                webDriver.Quit();
                webDriver.Dispose();
            }
        }

        #region Other methods

        internal static void StartWebDriver(bool headless) {
            isHidden = headless;
            switch (PluginOptions.BrowserType) {
                case BrowserType.IE: {
                    var driverService = InternetExplorerDriverService.CreateDefaultService();
                    driverService.HideCommandPromptWindow = true;
                    //
                    webDriver = new InternetExplorerDriver(driverService);
                    break;
                }
                case BrowserType.Edge: {
                    if (headless) {
                        MessageBox.Show("Browser plugin: Microsoft Edge cannot work without window yet.", "Browser plugin error");
                        return;
                    }
                    var driverService = EdgeDriverService.CreateDefaultService();
                    driverService.HideCommandPromptWindow = true;
                    //
                    webDriver = new EdgeDriver(driverService);
                    break;
                }
                case BrowserType.Chrome: {
                    var driverService = ChromeDriverService.CreateDefaultService();
                    driverService.HideCommandPromptWindow = true;
                    var options = new ChromeOptions();
                    if (headless) {
                        options.AddArgument("--headless");
                    }
                    //
                    webDriver = new ChromeDriver(driverService, options);
                    // hide log: options.AddArgument("--log-level=3");
                    break;
                }
                case BrowserType.Firefox: {
                    var driverService = FirefoxDriverService.CreateDefaultService();
                    driverService.HideCommandPromptWindow = true;
                    var options = new FirefoxOptions();
                    if (headless) {
                        options.AddArgument("--headless");
                    }
                    //
                    webDriver = new FirefoxDriver(driverService, options);
                    break;
                }
                default: {
                    MessageBox.Show(nameof(Browser) + " plugin error: Invalid browser selected.");
                    break;
                }
            }
            webDriver.Manage().Window.Minimize();
            webDriver.Navigate().GoToUrl("http://www.google.com");
        }

        internal static bool SelectWindow(string titleSubstring) {
            string currentWindow = webDriver.CurrentWindowHandle;

            foreach (string w in webDriver.WindowHandles) {
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