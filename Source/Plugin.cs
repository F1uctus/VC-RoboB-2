using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using PluginInterface;

namespace Browser {
    public class Plugin : IPlugin {

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

            try {
                switch (actionNameArray[1].ToUpper()) {
                    case "ACTION": {
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

        #endregion
    }
}