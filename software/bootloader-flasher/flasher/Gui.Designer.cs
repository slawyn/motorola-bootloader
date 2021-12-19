using System;
using System.Windows.Forms;



namespace Bootloader
{


    partial class Gui
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gui));
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnConnect = new Bootloader.RoundButton();
            this.cboxComs = new Bootloader.RoundComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new Bootloader.RoundButton();
            this.cboxShowCommunication = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tboxSendInput = new Bootloader.RoundText();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnEeprom = new Bootloader.RoundButton();
            this.lHexNamePIC18F46K22 = new System.Windows.Forms.Label();
            this.btnLoadPicHex = new Bootloader.RoundButton();
            this.cboxProgramConfiguration = new System.Windows.Forms.CheckBox();
            this.btnBridgeToPic = new Bootloader.RoundButton();
            this.btnProgramPic = new Bootloader.RoundButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lHexNameMC09S08AC60 = new System.Windows.Forms.Label();
            this.btnInitBootloader = new Bootloader.RoundButton();
            this.btnEraseFlash = new Bootloader.RoundButton();
            this.btnProgramFlash = new Bootloader.RoundButton();
            this.btnLoadMotorolaHex = new Bootloader.RoundButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnClear = new Bootloader.RoundButton();
            this.tboxOutput = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(232, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Command";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnConnect);
            this.panel1.Controls.Add(this.cboxComs);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(878, 104);
            this.panel1.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(95, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Ports:";
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnect.Location = new System.Drawing.Point(12, 11);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(77, 71);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.HandlerConnectPort);
            // 
            // cboxComs
            // 
            this.cboxComs.BackColor = System.Drawing.SystemColors.HighlightText;
            this.cboxComs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboxComs.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cboxComs.FormattingEnabled = true;
            this.cboxComs.Location = new System.Drawing.Point(138, 12);
            this.cboxComs.Name = "cboxComs";
            this.cboxComs.Size = new System.Drawing.Size(100, 24);
            this.cboxComs.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRefresh);
            this.groupBox1.Controls.Add(this.cboxShowCommunication);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tboxSendInput);
            this.groupBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(3, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(348, 96);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(240, 9);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(102, 28);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.HandlerRefreshPorts);
            // 
            // cboxShowCommunication
            // 
            this.cboxShowCommunication.AutoSize = true;
            this.cboxShowCommunication.Location = new System.Drawing.Point(101, 74);
            this.cboxShowCommunication.Name = "cboxShowCommunication";
            this.cboxShowCommunication.Size = new System.Drawing.Size(134, 17);
            this.cboxShowCommunication.TabIndex = 5;
            this.cboxShowCommunication.Text = "Show Communication";
            this.cboxShowCommunication.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(92, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Debug:";
            // 
            // tboxSendInput
            // 
            this.tboxSendInput.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tboxSendInput.Location = new System.Drawing.Point(133, 41);
            this.tboxSendInput.Name = "tboxSendInput";
            this.tboxSendInput.Size = new System.Drawing.Size(209, 32);
            this.tboxSendInput.TabIndex = 14;
            this.tboxSendInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandlerSendInput);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnEeprom);
            this.groupBox3.Controls.Add(this.lHexNamePIC18F46K22);
            this.groupBox3.Controls.Add(this.btnLoadPicHex);
            this.groupBox3.Controls.Add(this.cboxProgramConfiguration);
            this.groupBox3.Controls.Add(this.btnBridgeToPic);
            this.groupBox3.Controls.Add(this.btnProgramPic);
            this.groupBox3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(602, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(271, 95);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "PIC18F46K22";
            // 
            // btnEeprom
            // 
            this.btnEeprom.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnEeprom.Location = new System.Drawing.Point(95, 32);
            this.btnEeprom.Name = "btnEeprom";
            this.btnEeprom.Size = new System.Drawing.Size(87, 30);
            this.btnEeprom.TabIndex = 17;
            this.btnEeprom.Text = "Eeprom";
            this.btnEeprom.UseVisualStyleBackColor = false;
            this.btnEeprom.Click += new System.EventHandler(this.HandlerProgramEepromPIC18F46K22);
            // 
            // lHexNamePIC18F46K22
            // 
            this.lHexNamePIC18F46K22.AutoSize = true;
            this.lHexNamePIC18F46K22.Location = new System.Drawing.Point(7, 70);
            this.lHexNamePIC18F46K22.Name = "lHexNamePIC18F46K22";
            this.lHexNamePIC18F46K22.Size = new System.Drawing.Size(0, 13);
            this.lHexNamePIC18F46K22.TabIndex = 16;
            // 
            // btnLoadPicHex
            // 
            this.btnLoadPicHex.Location = new System.Drawing.Point(180, 61);
            this.btnLoadPicHex.Name = "btnLoadPicHex";
            this.btnLoadPicHex.Size = new System.Drawing.Size(87, 29);
            this.btnLoadPicHex.TabIndex = 5;
            this.btnLoadPicHex.Text = "Load  .hex";
            this.btnLoadPicHex.UseVisualStyleBackColor = true;
            this.btnLoadPicHex.Click += new System.EventHandler(this.HandlerLoadPIC18F46K22Hex);
            // 
            // cboxProgramConfiguration
            // 
            this.cboxProgramConfiguration.AutoSize = true;
            this.cboxProgramConfiguration.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboxProgramConfiguration.Location = new System.Drawing.Point(163, 13);
            this.cboxProgramConfiguration.Name = "cboxProgramConfiguration";
            this.cboxProgramConfiguration.Size = new System.Drawing.Size(104, 17);
            this.cboxProgramConfiguration.TabIndex = 15;
            this.cboxProgramConfiguration.Text = "Configuration";
            this.cboxProgramConfiguration.UseVisualStyleBackColor = true;
            // 
            // btnBridgeToPic
            // 
            this.btnBridgeToPic.BackColor = System.Drawing.Color.LightBlue;
            this.btnBridgeToPic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBridgeToPic.Location = new System.Drawing.Point(5, 12);
            this.btnBridgeToPic.Name = "btnBridgeToPic";
            this.btnBridgeToPic.Size = new System.Drawing.Size(88, 50);
            this.btnBridgeToPic.TabIndex = 10;
            this.btnBridgeToPic.Text = "Bridge Bootloader";
            this.btnBridgeToPic.UseVisualStyleBackColor = false;
            this.btnBridgeToPic.Click += new System.EventHandler(this.HandlerBridgeToPIC18F46K22);
            // 
            // btnProgramPic
            // 
            this.btnProgramPic.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnProgramPic.Location = new System.Drawing.Point(181, 32);
            this.btnProgramPic.Name = "btnProgramPic";
            this.btnProgramPic.Size = new System.Drawing.Size(87, 30);
            this.btnProgramPic.TabIndex = 11;
            this.btnProgramPic.Text = "Program";
            this.btnProgramPic.UseVisualStyleBackColor = false;
            this.btnProgramPic.Click += new System.EventHandler(this.HandlerProgramPIC18F46K22);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lHexNameMC09S08AC60);
            this.groupBox2.Controls.Add(this.btnInitBootloader);
            this.groupBox2.Controls.Add(this.btnEraseFlash);
            this.groupBox2.Controls.Add(this.btnProgramFlash);
            this.groupBox2.Controls.Add(this.btnLoadMotorolaHex);
            this.groupBox2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(357, 1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(239, 96);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "MC9S08AC60";
            // 
            // lHexNameMC09S08AC60
            // 
            this.lHexNameMC09S08AC60.AutoSize = true;
            this.lHexNameMC09S08AC60.Location = new System.Drawing.Point(8, 70);
            this.lHexNameMC09S08AC60.Name = "lHexNameMC09S08AC60";
            this.lHexNameMC09S08AC60.Size = new System.Drawing.Size(0, 13);
            this.lHexNameMC09S08AC60.TabIndex = 5;
            // 
            // btnInitBootloader
            // 
            this.btnInitBootloader.BackColor = System.Drawing.Color.LightCoral;
            this.btnInitBootloader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInitBootloader.Location = new System.Drawing.Point(5, 13);
            this.btnInitBootloader.Margin = new System.Windows.Forms.Padding(0);
            this.btnInitBootloader.Name = "btnInitBootloader";
            this.btnInitBootloader.Size = new System.Drawing.Size(83, 54);
            this.btnInitBootloader.TabIndex = 6;
            this.btnInitBootloader.Text = "Initialize Bootloader";
            this.btnInitBootloader.UseVisualStyleBackColor = false;
            this.btnInitBootloader.Click += new System.EventHandler(this.HandlerInitBootloader);
            // 
            // btnEraseFlash
            // 
            this.btnEraseFlash.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnEraseFlash.Location = new System.Drawing.Point(151, 8);
            this.btnEraseFlash.Name = "btnEraseFlash";
            this.btnEraseFlash.Size = new System.Drawing.Size(83, 27);
            this.btnEraseFlash.TabIndex = 8;
            this.btnEraseFlash.Text = "Erase ";
            this.btnEraseFlash.UseVisualStyleBackColor = false;
            this.btnEraseFlash.Click += new System.EventHandler(this.HandlerEraseMC9S08AC60);
            // 
            // btnProgramFlash
            // 
            this.btnProgramFlash.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnProgramFlash.Location = new System.Drawing.Point(151, 33);
            this.btnProgramFlash.Name = "btnProgramFlash";
            this.btnProgramFlash.Size = new System.Drawing.Size(83, 30);
            this.btnProgramFlash.TabIndex = 9;
            this.btnProgramFlash.Text = "Program";
            this.btnProgramFlash.UseVisualStyleBackColor = false;
            this.btnProgramFlash.Click += new System.EventHandler(this.HandlerProgramMC9S08AC60);
            // 
            // btnLoadMotorolaHex
            // 
            this.btnLoadMotorolaHex.Location = new System.Drawing.Point(151, 61);
            this.btnLoadMotorolaHex.Name = "btnLoadMotorolaHex";
            this.btnLoadMotorolaHex.Size = new System.Drawing.Size(83, 29);
            this.btnLoadMotorolaHex.TabIndex = 7;
            this.btnLoadMotorolaHex.Text = "Load .hex";
            this.btnLoadMotorolaHex.UseVisualStyleBackColor = true;
            this.btnLoadMotorolaHex.Click += new System.EventHandler(this.HandlerLoadMC9S08AC60Hex);
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.AutoScrollMargin = new System.Drawing.Size(5, 5);
            this.panel3.AutoSize = true;
            this.panel3.Controls.Add(this.btnClear);
            this.panel3.Controls.Add(this.tboxOutput);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 104);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(878, 657);
            this.panel3.TabIndex = 4;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.AutoSize = true;
            this.btnClear.BackColor = System.Drawing.Color.Coral;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(806, 619);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(60, 35);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.HandlerCleartText);
            // 
            // tboxOutput
            // 
            this.tboxOutput.BackColor = System.Drawing.Color.White;
            this.tboxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tboxOutput.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tboxOutput.Location = new System.Drawing.Point(0, 0);
            this.tboxOutput.Name = "tboxOutput";
            this.tboxOutput.Size = new System.Drawing.Size(878, 657);
            this.tboxOutput.TabIndex = 4;
            this.tboxOutput.Text = "";
            // 
            // Gui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 761);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Gui";
            this.Text = "flasher-2021-04-05";
            this.Load += new System.EventHandler(this.HandlerOnFormLoad);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private RoundButton btnConnect;
        private RoundComboBox cboxComs;
        private RoundButton btnRefresh;
        private Panel panel1;
        private Panel panel3;
        private RoundButton btnClear;
        private RichTextBox tboxOutput;
        private Label label3;
        private RoundButton btnLoadPicHex;
        private RoundButton btnLoadMotorolaHex;
        private RoundButton btnInitBootloader;
        private RoundButton btnEraseFlash;
        private RoundButton btnProgramFlash;
        private RoundButton btnBridgeToPic;
        private RoundButton btnProgramPic;
        private RoundText tboxSendInput;
        private Label label2;
        private CheckBox cboxShowCommunication;
        private CheckBox cboxProgramConfiguration;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Label lHexNameMC09S08AC60;
        private Label lHexNamePIC18F46K22;
        private RoundButton btnEeprom;
    }
  

}



