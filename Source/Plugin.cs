using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using PluginInterface;

namespace Browser {
    public class Plugin : IPlugin {
        private static IWebDriver  webDriver;
        private static Size        savedNormalSize;
        private static bool        isPageLoaded = false;
        private static bool        isHidden;
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
                MainCtl      = (CtlMain) MainInterface;
            }
        }

        public UserControl MainInterface { get; }

        #endregion

        public Plugin() {
            MainCtl       = new CtlMain();
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
            var          ar            = new actionResult();
            const string unknownAction = "Unknown " + nameof(Browser) + " plugin action.";
            if (actionNameArray.Length < 2) {
                ar.setError(unknownAction);
                return ar;
            }
            string actionName1 = actionNameArray[1].ToUpper();
            if (webDriver == null
             && actionName1 != "START") {
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
                            if (!actionParameters[0].StartsWith("http://") ||
                                !actionParameters[0].StartsWith("https://")) {
                                actionParameters[0] = "https://" + actionParameters[0];
                            }
                            try {
                                isPageLoaded = false;
                                Task.Run(
                                    () => {
                                        webDriver.Navigate().GoToUrl(actionParameters[0]);
                                        isPageLoaded = true;
                                        HostInstance.triggerEvent("Browser.PageLoaded", new List<string>(0));
                                    }
                                );
                            }
                            catch (Exception ex) {
                                ar.setError("Failed to open specified URL:\n" + ex);
                                break;
                            }
                            ar.setInfo("Page loading started.");
                        }
                        break;
                    }

                    case "WAIT": {
                        while (!isPageLoaded) {
                            Thread.Sleep(10);
                        }
                        break;
                    }

                    // window size and location
                    case "GETSIZE": {
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

                    case "GETPOSITION": {
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
                    case "NORMALIZE": {
                        webDriver.Manage().Window.Size = savedNormalSize;
                        break;
                    }

                    case "MAXIMIZE": {
                        savedNormalSize = webDriver.Manage().Window.Size;
                        webDriver.Manage().Window.Maximize();
                        break;
                    }

                    case "MINIMIZE": {
                        savedNormalSize = webDriver.Manage().Window.Size;
                        webDriver.Manage().Window.Minimize();
                        break;
                    }

                    case "FULLSCREEN": {
                        savedNormalSize = webDriver.Manage().Window.Size;
                        webDriver.Manage().Window.FullScreen();
                        break;
                    }

                    case "ELEMENT": {
                        if (actionNameArray.Length < 3) {
                            ar.setError(unknownAction);
                            break;
                        }
                        switch (actionNameArray[2].ToUpper()) {
                            case "FIND": {
                                if (actionParameters.Length == 0) {
                                    ar.setError("'Selector' parameter missing.");
                                    return ar;
                                }
                                if (actionParameters.Length == 1) {
                                    ar.setError("'Selector value' parameter missing.");
                                    return ar;
                                }
                                // selector parameter
                                By selector;
                                switch (actionParameters[0].ToUpper()) {
                                    case "ID": {
                                        selector = By.Id(actionParameters[1]);
                                        break;
                                    }
                                    case "CLASS": {
                                        selector = By.ClassName(actionParameters[1]);
                                        break;
                                    }
                                    case "CSS": {
                                        selector = By.CssSelector(actionParameters[1]);
                                        break;
                                    }
                                    case "NAME": {
                                        selector = By.Name(actionParameters[1]);
                                        break;
                                    }
                                    case "TAG": {
                                        selector = By.TagName(actionParameters[1]);
                                        break;
                                    }
                                    case "LINKTEXT": {
                                        selector = By.LinkText(actionParameters[1]);
                                        break;
                                    }
                                    case "PARTIALLINKTEXT": {
                                        selector = By.PartialLinkText(actionParameters[1]);
                                        break;
                                    }
                                    case "XPATH": {
                                        selector = By.XPath(actionParameters[1]);
                                        break;
                                    }
                                    default: {
                                        ar.setError("Invalid 'Selector' parameter.");
                                        return ar;
                                    }
                                }
                                ReadOnlyCollection<IWebElement> elements = webDriver.FindElements(selector);
                                if (elements.Count == 0) {
                                    ar.setError("Cannot find element with specified value. Check your 'Selector value' parameter.");
                                }
                                else {
                                    currentElement = elements[0];
                                    ar.setInfo("Element found.");
                                }
                                break;
                            }
                            case "INPUT": {
                                if (currentElement == null) {
                                    ar.setError("No element selected.");
                                    return ar;
                                }
                                if (actionParameters.Length == 0) {
                                    ar.setError("'Text to write' parameter missing.");
                                    return ar;
                                }
                                currentElement.SendKeys(actionParameters[0]);
                                ar.setInfo("OK.");
                                break;
                            }
                            case "CLICK": {
                                if (currentElement == null) {
                                    ar.setError("No element selected.");
                                    return ar;
                                }
                                currentElement.Click();
                                ar.setInfo("OK.");
                                break;
                            }
                            case "GETATTR": {
                                // value of specified attribute for this element.
                                if (currentElement == null) {
                                    ar.setError("No element selected.");
                                    return ar;
                                }
                                if (actionParameters.Length == 1) {
                                    ar.setError("'Attribute name' parameter missing.");
                                    return ar;
                                }
                                string attrValue;
                                try {
                                    attrValue = currentElement.GetAttribute(actionParameters[0]);
                                }
                                catch (StaleElementReferenceException) {
                                    ar.setError("Reference to current element is no longer valid.");
                                    return ar;
                                }
                                ar.setSuccess(attrValue);
                                break;
                            }
                            case "GETPROP": {
                                // JavaScript property of the element.
                                if (currentElement == null) {
                                    ar.setError("No element selected.");
                                    return ar;
                                }
                                if (actionParameters.Length == 1) {
                                    ar.setError("'JS property name' parameter missing.");
                                    return ar;
                                }
                                string attrValue;
                                try {
                                    attrValue = currentElement.GetProperty(actionParameters[0]);
                                }
                                catch (StaleElementReferenceException) {
                                    ar.setError("Reference to current element is no longer valid.");
                                    return ar;
                                }
                                ar.setSuccess(attrValue);
                                break;
                            }
                            case "GETCSS": {
                                // value of CSS property of current element.
                                if (currentElement == null) {
                                    ar.setError("No element selected.");
                                    return ar;
                                }
                                if (actionParameters.Length == 1) {
                                    ar.setError("'CSS property name' parameter missing.");
                                    return ar;
                                }
                                string attrValue;
                                try {
                                    attrValue = currentElement.GetCssValue(actionParameters[0]);
                                }
                                catch (StaleElementReferenceException) {
                                    ar.setError("Reference to current element is no longer valid.");
                                    return ar;
                                }
                                ar.setSuccess(attrValue);
                                break;
                            }
                            case "GETPARENT": {
                                if (currentElement == null) {
                                    ar.setError("No element selected.");
                                    return ar;
                                }
                                try {
                                    currentElement.FindElement(By.XPath("//span/parent::*"));
                                }
                                catch (NoSuchElementException) {
                                    ar.setError("Cannot find parent of current element.");
                                }
                                break;
                            }
                            default: {
                                ar.setError(unknownAction);
                                break;
                            }
                        }
                        break;
                    }

                    #region Working with tabs

                    case "TAB": {
                        if (actionNameArray.Length < 3) {
                            ar.setError(unknownAction);
                            break;
                        }
                        switch (actionNameArray[2].ToUpper()) {
                            case "NEW": {
                                webDriver.ExecuteJavaScript("window.open('','_blank');");
                                webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());
                                break;
                            }

                            case "SELECT": {
                                if (actionParameters.Length == 0) {
                                    ar.setError("'Tab name' parameter missing.");
                                }
                                else if (SelectWindow(actionParameters[0])) {
                                    ar.setInfo("OK.");
                                }
                                else {
                                    ar.setError("Cannot find tab with specified name.");
                                }
                                break;
                            }

                            case "SELECTBYNUM": {
                                int tabNumber = 0;
                                if (actionParameters.Length == 0) {
                                    ar.setError("'Tab index' parameter missing.");
                                }
                                else if (int.TryParse(actionParameters[0], out tabNumber)
                                      || tabNumber - 1 > webDriver.WindowHandles.Count) {
                                    webDriver.SwitchTo().Window(webDriver.WindowHandles[tabNumber - 1]);
                                    ar.setInfo("OK.");
                                }
                                else {
                                    ar.setError("Cannot find tab with specified name.");
                                }
                                break;
                            }

                            case "CLOSE": {
                                webDriver.Close();
                                webDriver?.SwitchTo().Window(webDriver.WindowHandles.Last());
                                break;
                            }

                            case "TITLE": {
                                ar.setSuccess(webDriver.Title);
                                break;
                            }

                            case "URL": {
                                ar.setSuccess(webDriver.Url);
                                break;
                            }

                            case "GETTABS": {
                                ar.setSuccess(string.Join("\n", webDriver.WindowHandles));
                                break;
                            }

                            default: {
                                ar.setError(unknownAction);
                                break;
                            }
                        }
                        break;
                    }

                    #endregion

                    #region Browser start/stop

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
                IWindow window = webDriver.Manage().Window;
                PluginOptions.BrowserWindowX = window.Position.X;
                PluginOptions.BrowserWindowY = window.Position.Y;
                PluginOptions.BrowserWindowW = window.Size.Width;
                PluginOptions.BrowserWindowH = window.Size.Height;

                for (int i = 0; i < webDriver?.WindowHandles.Count; i++) {
                    webDriver.Close();
                }
                webDriver.Quit();
                webDriver.Dispose();
            }
            // kill all left browser processes
            foreach (var process in Process.GetProcessesByName(PluginOptions.WebDriverFileNames[PluginOptions.BrowserType].Replace(".exe", ""))) {
                process.Kill();
            }
        }

        #region Other methods

        internal static void StartWebDriver(bool headless) {
            isHidden = headless;
            switch (PluginOptions.BrowserType) {
                case BrowserType.IE: {
                    var driverService = InternetExplorerDriverService.CreateDefaultService(
                        PluginOptions.WebDriverDirectory,
                        PluginOptions.WebDriverFileNames[PluginOptions.BrowserType]
                    );
                    driverService.HideCommandPromptWindow = true;
                    //
                    webDriver = new InternetExplorerDriver(driverService);
                    break;
                }
                case BrowserType.Edge: {
                    if (headless) {
                        MessageBox.Show("Browser plugin: Microsoft Edge cannot work without window.", "Browser plugin error");
                        return;
                    }
                    var driverService = EdgeDriverService.CreateDefaultService(
                        PluginOptions.WebDriverDirectory,
                        PluginOptions.WebDriverFileNames[PluginOptions.BrowserType]
                    );
                    driverService.HideCommandPromptWindow = true;

                    webDriver = new EdgeDriver(driverService);
                    break;
                }
                case BrowserType.Chrome: {
                    var driverService = ChromeDriverService.CreateDefaultService(
                        PluginOptions.WebDriverDirectory,
                        PluginOptions.WebDriverFileNames[PluginOptions.BrowserType]
                    );
                    driverService.HideCommandPromptWindow = true;
                    var options = new ChromeOptions();
                    if (headless) {
                        options.AddArgument("--headless");
                    }

                    webDriver = new ChromeDriver(driverService, options);
                    // hide log: options.AddArgument("--log-level=3");
                    break;
                }
                case BrowserType.Firefox: {
                    var driverService = FirefoxDriverService.CreateDefaultService(
                        PluginOptions.WebDriverDirectory,
                        PluginOptions.WebDriverFileNames[PluginOptions.BrowserType]
                    );
                    driverService.HideCommandPromptWindow = true;
                    var options = new FirefoxOptions();
                    if (headless) {
                        options.AddArgument("--headless");
                    }

                    webDriver = new FirefoxDriver(driverService, options);
                    break;
                }
                default: {
                    MessageBox.Show(nameof(Browser) + " plugin error: Invalid browser selected: " + PluginOptions.BrowserType.ToString("G"));
                    break;
                }
            }
            webDriver.Manage().Window.Minimize();
            webDriver.Navigate().GoToUrl("http://www.google.com");
        }

        internal static bool SelectWindow(string titleSubstring) {
            string currentWindow = webDriver.CurrentWindowHandle;

            for (var i = 0; i < webDriver.WindowHandles.Count; i++) {
                if (webDriver.WindowHandles[i] != currentWindow) {
                    webDriver.SwitchTo().Window(webDriver.WindowHandles[i]);
                    if (webDriver.Title.Contains(titleSubstring)) {
                        return true;
                    }

                    webDriver.SwitchTo().Window(currentWindow);
                }
            }
            return false;
        }

        internal static void WaitWindowLoad() {
            new WebDriverWait(webDriver, webDriver.Manage().Timeouts().PageLoad).Until(
                d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete")
            );
        }

        #endregion
    }
}