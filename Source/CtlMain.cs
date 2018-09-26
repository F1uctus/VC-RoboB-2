using System;
using System.Drawing;
using System.Windows.Forms;

namespace Browser {
    public partial class CtlMain : UserControl {
        private const string noPortsFound = "No available ports found.";

        public CtlMain() {
            InitializeComponent();


        }

        private void CtlMain_Load(object sender, EventArgs e) {
        }

        private void btSave_Click(object sender, EventArgs e) {
            // receive options from view

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