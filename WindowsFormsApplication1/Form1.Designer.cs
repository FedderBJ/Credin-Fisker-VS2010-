namespace WindowsFormsApplication1
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.tbCommunication = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnAutoRun = new System.Windows.Forms.Button();
            this.ResetChanel = new System.Windows.Forms.Timer(this.components);
            this.WaitTimer = new System.Windows.Forms.Timer(this.components);
            this.RunTimer = new System.Windows.Forms.Timer(this.components);
            this.btnInit = new System.Windows.Forms.Button();
            this.cbService = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tbCommunication
            // 
            this.tbCommunication.Location = new System.Drawing.Point(9, 40);
            this.tbCommunication.Multiline = true;
            this.tbCommunication.Name = "tbCommunication";
            this.tbCommunication.ReadOnly = true;
            this.tbCommunication.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbCommunication.Size = new System.Drawing.Size(612, 246);
            this.tbCommunication.TabIndex = 8;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(546, 292);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnAutoRun
            // 
            this.btnAutoRun.Location = new System.Drawing.Point(91, 11);
            this.btnAutoRun.Name = "btnAutoRun";
            this.btnAutoRun.Size = new System.Drawing.Size(75, 23);
            this.btnAutoRun.TabIndex = 14;
            this.btnAutoRun.Text = "Auto Run";
            this.btnAutoRun.UseVisualStyleBackColor = true;
            this.btnAutoRun.Click += new System.EventHandler(this.btnAutoRun_Click);
            // 
            // ResetChanel
            // 
            this.ResetChanel.Interval = 2000;
            this.ResetChanel.Tick += new System.EventHandler(this.ResetChanel_Tick);
            // 
            // WaitTimer
            // 
            this.WaitTimer.Interval = 1000;
            this.WaitTimer.Tick += new System.EventHandler(this.WaitTimer_Tick);
            // 
            // RunTimer
            // 
            this.RunTimer.Interval = 500;
            this.RunTimer.Tick += new System.EventHandler(this.RunTimer_Tick);
            // 
            // btnInit
            // 
            this.btnInit.Location = new System.Drawing.Point(10, 11);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(75, 23);
            this.btnInit.TabIndex = 15;
            this.btnInit.Text = "Init Com";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // cbService
            // 
            this.cbService.AutoSize = true;
            this.cbService.Checked = true;
            this.cbService.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbService.Location = new System.Drawing.Point(471, 15);
            this.cbService.Name = "cbService";
            this.cbService.Size = new System.Drawing.Size(150, 17);
            this.cbService.TabIndex = 16;
            this.cbService.Text = "Auto. Check NavService?";
            this.cbService.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 319);
            this.Controls.Add(this.cbService);
            this.Controls.Add(this.btnInit);
            this.Controls.Add(this.btnAutoRun);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tbCommunication);
            this.Name = "Form1";
            this.Text = "Automation Connector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbCommunication;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnAutoRun;
        private System.Windows.Forms.Timer ResetChanel;
        private System.Windows.Forms.Timer WaitTimer;
        private System.Windows.Forms.Timer RunTimer;
        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.CheckBox cbService;
    }
}

