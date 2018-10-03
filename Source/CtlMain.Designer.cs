using System.ComponentModel;
using System.Windows.Forms;

namespace Browser
{
    partial class CtlMain
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btSave = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.comboBrowserType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbLaunchHidden = new System.Windows.Forms.CheckBox();
            this.cbLaunchAtStartup = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btSave.FlatAppearance.BorderSize = 0;
            this.btSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btSave.ForeColor = System.Drawing.Color.White;
            this.btSave.Location = new System.Drawing.Point(0, 0);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(450, 30);
            this.btSave.TabIndex = 5;
            this.btSave.Text = "Save options";
            this.btSave.UseVisualStyleBackColor = false;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 100;
            // 
            // comboBrowserType
            // 
            this.comboBrowserType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBrowserType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.comboBrowserType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBrowserType.ForeColor = System.Drawing.Color.Gainsboro;
            this.comboBrowserType.FormattingEnabled = true;
            this.comboBrowserType.Location = new System.Drawing.Point(87, 41);
            this.comboBrowserType.Name = "comboBrowserType";
            this.comboBrowserType.Size = new System.Drawing.Size(360, 23);
            this.comboBrowserType.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Browser type:";
            // 
            // cbLaunchHidden
            // 
            this.cbLaunchHidden.AutoSize = true;
            this.cbLaunchHidden.Location = new System.Drawing.Point(6, 101);
            this.cbLaunchHidden.Name = "cbLaunchHidden";
            this.cbLaunchHidden.Size = new System.Drawing.Size(197, 19);
            this.cbLaunchHidden.TabIndex = 8;
            this.cbLaunchHidden.Text = "Launch browser in hidden mode";
            this.cbLaunchHidden.UseVisualStyleBackColor = true;
            // 
            // cbLaunchAtStartup
            // 
            this.cbLaunchAtStartup.AutoSize = true;
            this.cbLaunchAtStartup.Location = new System.Drawing.Point(6, 76);
            this.cbLaunchAtStartup.Name = "cbLaunchAtStartup";
            this.cbLaunchAtStartup.Size = new System.Drawing.Size(225, 19);
            this.cbLaunchAtStartup.TabIndex = 9;
            this.cbLaunchAtStartup.Text = "Launch browser with program startup";
            this.cbLaunchAtStartup.UseVisualStyleBackColor = true;
            // 
            // CtlMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.Controls.Add(this.cbLaunchAtStartup);
            this.Controls.Add(this.cbLaunchHidden);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBrowserType);
            this.Controls.Add(this.btSave);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ForeColor = System.Drawing.Color.Gainsboro;
            this.MinimumSize = new System.Drawing.Size(450, 450);
            this.Name = "CtlMain";
            this.Size = new System.Drawing.Size(450, 450);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
        private Button btSave;
        private ToolTip toolTip;
        private ComboBox comboBrowserType;
        private Label label1;
        private CheckBox cbLaunchHidden;
        private CheckBox cbLaunchAtStartup;
    }
}
