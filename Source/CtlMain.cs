using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace Browser {
    public partial class CtlMain : UserControl {
        public CtlMain() {
            InitializeComponent();

            // Bind combobox to dictionary
            var browserTypes = new Dictionary<string, BrowserType> {
                { "Google Chrome (Chromium)", BrowserType.Chrome },
                { "Internet Explorer", BrowserType.IE }
            };

            string winVersionCaption = "";
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")) {
                foreach (var o in searcher.Get()) {
                    var obj = (ManagementObject) o;
                    winVersionCaption = obj["Caption"].ToString();
                }
            }
            // check for Windows 10
            if (winVersionCaption.Contains("Windows 10")) {
                browserTypes.Add("Microsoft Edge", BrowserType.Edge);
            }

            comboBrowserType.DataSource    = new BindingSource(browserTypes, null);
            comboBrowserType.DisplayMember = "Key";
            comboBrowserType.ValueMember   = "Value";
            comboBrowserType.SelectedItem  = browserTypes.First(kvp => kvp.Value == PluginOptions.BrowserType);
            cbLaunchAtStartup.Checked      = PluginOptions.LaunchAtStartup;
            cbLaunchHidden.Checked         = PluginOptions.LaunchHidden;
        }

        private void btSave_Click(object sender, EventArgs e) {
            // receive options from view
            PluginOptions.BrowserType     = ((KeyValuePair<string, BrowserType>) comboBrowserType.SelectedItem).Value;
            PluginOptions.LaunchAtStartup = cbLaunchAtStartup.Checked;
            PluginOptions.LaunchHidden    = cbLaunchHidden.Checked;

            // save options
            try {
                PluginOptions.SaveOptionsToXml();

                // show that settings have been saved
                Color  prevBtBackColor = btSave.BackColor;
                string prevBtMessage   = btSave.Text;
                btSave.Enabled   = false;
                btSave.BackColor = Color.FromArgb(202, 81, 0);
                btSave.Text      = "Options saved.";
                var timer = new Timer { Interval = 2500 };
                timer.Tick += delegate {
                    btSave.BackColor = prevBtBackColor;
                    btSave.Text      = prevBtMessage;
                    btSave.Enabled   = true;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch (Exception ex) {
                MessageBox.Show("An error occurred while saving options:\r\n" + ex);
            }
        }
    }
}