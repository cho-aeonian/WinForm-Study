namespace MiniHexClientWinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtHost = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.grpSend = new System.Windows.Forms.GroupBox();
            this.btnBuildSend = new System.Windows.Forms.Button();
            this.txtPayloadHex = new System.Windows.Forms.TextBox();
            this.txtCmdHex = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.grpQuick = new System.Windows.Forms.GroupBox();
            this.btnSetStatus2 = new System.Windows.Forms.Button();
            this.btnSetStatus1 = new System.Windows.Forms.Button();
            this.btnGetNumber = new System.Windows.Forms.Button();
            this.btnGetStatus = new System.Windows.Forms.Button();
            this.btnHello = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSendRaw = new System.Windows.Forms.Button();
            this.txtRawHex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.grpSend.SuspendLayout();
            this.grpQuick.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(64, 14);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(150, 23);
            this.txtHost.TabIndex = 0;
            this.txtHost.Text = "127.0.0.1";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(262, 14);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(72, 23);
            this.txtPort.TabIndex = 1;
            this.txtPort.Text = "9001";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(350, 13);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 25);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(431, 13);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(85, 25);
            this.btnDisconnect.TabIndex = 3;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(14, 54);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(502, 310);
            this.txtLog.TabIndex = 4;
            // 
            // grpSend
            // 
            this.grpSend.Controls.Add(this.btnBuildSend);
            this.grpSend.Controls.Add(this.txtPayloadHex);
            this.grpSend.Controls.Add(this.txtCmdHex);
            this.grpSend.Controls.Add(this.label3);
            this.grpSend.Controls.Add(this.label2);
            this.grpSend.Controls.Add(this.label1);
            this.grpSend.Enabled = false;
            this.grpSend.Location = new System.Drawing.Point(530, 54);
            this.grpSend.Name = "grpSend";
            this.grpSend.Size = new System.Drawing.Size(356, 132);
            this.grpSend.TabIndex = 5;
            this.grpSend.TabStop = false;
            this.grpSend.Text = "Build+Send";
            // 
            // btnBuildSend
            // 
            this.btnBuildSend.Location = new System.Drawing.Point(252, 67);
            this.btnBuildSend.Name = "btnBuildSend";
            this.btnBuildSend.Size = new System.Drawing.Size(87, 25);
            this.btnBuildSend.TabIndex = 5;
            this.btnBuildSend.Text = "Build+Send";
            this.btnBuildSend.UseVisualStyleBackColor = true;
            this.btnBuildSend.Click += new System.EventHandler(this.btnBuildSend_Click);
            // 
            // txtPayloadHex
            // 
            this.txtPayloadHex.Location = new System.Drawing.Point(79, 67);
            this.txtPayloadHex.Name = "txtPayloadHex";
            this.txtPayloadHex.PlaceholderText = "e.g. 02";
            this.txtPayloadHex.Size = new System.Drawing.Size(160, 23);
            this.txtPayloadHex.TabIndex = 4;
            // 
            // txtCmdHex
            // 
            this.txtCmdHex.Location = new System.Drawing.Point(16, 67);
            this.txtCmdHex.Name = "txtCmdHex";
            this.txtCmdHex.PlaceholderText = "e.g. 01";
            this.txtCmdHex.Size = new System.Drawing.Size(51, 23);
            this.txtCmdHex.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "CMD (hex)";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(79, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "PAYLOAD (hex)";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(320, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Build: [AA CMD LEN PAY CHK 55] (XOR)";
            // 
            // grpQuick
            // 
            this.grpQuick.Controls.Add(this.btnSetStatus2);
            this.grpQuick.Controls.Add(this.btnSetStatus1);
            this.grpQuick.Controls.Add(this.btnGetNumber);
            this.grpQuick.Controls.Add(this.btnGetStatus);
            this.grpQuick.Controls.Add(this.btnHello);
            this.grpQuick.Enabled = false;
            this.grpQuick.Location = new System.Drawing.Point(530, 200);
            this.grpQuick.Name = "grpQuick";
            this.grpQuick.Size = new System.Drawing.Size(356, 164);
            this.grpQuick.TabIndex = 6;
            this.grpQuick.TabStop = false;
            this.grpQuick.Text = "Quick";
            // 
            // btnSetStatus2
            // 
            this.btnSetStatus2.Location = new System.Drawing.Point(144, 106);
            this.btnSetStatus2.Name = "btnSetStatus2";
            this.btnSetStatus2.Size = new System.Drawing.Size(95, 25);
            this.btnSetStatus2.TabIndex = 4;
            this.btnSetStatus2.Text = "SET_STATUS=2";
            this.btnSetStatus2.UseVisualStyleBackColor = true;
            this.btnSetStatus2.Click += new System.EventHandler(this.btnSetStatus2_Click);
            // 
            // btnSetStatus1
            // 
            this.btnSetStatus1.Location = new System.Drawing.Point(16, 106);
            this.btnSetStatus1.Name = "btnSetStatus1";
            this.btnSetStatus1.Size = new System.Drawing.Size(95, 25);
            this.btnSetStatus1.TabIndex = 3;
            this.btnSetStatus1.Text = "SET_STATUS=1";
            this.btnSetStatus1.UseVisualStyleBackColor = true;
            this.btnSetStatus1.Click += new System.EventHandler(this.btnSetStatus1_Click);
            // 
            // btnGetNumber
            // 
            this.btnGetNumber.Location = new System.Drawing.Point(130, 59);
            this.btnGetNumber.Name = "btnGetNumber";
            this.btnGetNumber.Size = new System.Drawing.Size(95, 25);
            this.btnGetNumber.TabIndex = 2;
            this.btnGetNumber.Text = "GET_NUMBER";
            this.btnGetNumber.UseVisualStyleBackColor = true;
            this.btnGetNumber.Click += new System.EventHandler(this.btnGetNumber_Click);
            // 
            // btnGetStatus
            // 
            this.btnGetStatus.Location = new System.Drawing.Point(16, 59);
            this.btnGetStatus.Name = "btnGetStatus";
            this.btnGetStatus.Size = new System.Drawing.Size(95, 25);
            this.btnGetStatus.TabIndex = 1;
            this.btnGetStatus.Text = "GET_STATUS";
            this.btnGetStatus.UseVisualStyleBackColor = true;
            this.btnGetStatus.Click += new System.EventHandler(this.btnGetStatus_Click);
            // 
            // btnHello
            // 
            this.btnHello.Location = new System.Drawing.Point(16, 22);
            this.btnHello.Name = "btnHello";
            this.btnHello.Size = new System.Drawing.Size(75, 25);
            this.btnHello.TabIndex = 0;
            this.btnHello.Text = "HELLO";
            this.btnHello.UseVisualStyleBackColor = true;
            this.btnHello.Click += new System.EventHandler(this.btnHello_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(14, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 15);
            this.label5.TabIndex = 9;
            this.label5.Text = "Host / Port";
            // 
            // btnSendRaw
            // 
            this.btnSendRaw.Location = new System.Drawing.Point(835, 370);
            this.btnSendRaw.Name = "btnSendRaw";
            this.btnSendRaw.Size = new System.Drawing.Size(51, 25);
            this.btnSendRaw.TabIndex = 11;
            this.btnSendRaw.Text = "Send";
            this.btnSendRaw.UseVisualStyleBackColor = true;
            this.btnSendRaw.Click += new System.EventHandler(this.btnSendRaw_Click);
            // 
            // txtRawHex
            // 
            this.txtRawHex.Location = new System.Drawing.Point(530, 341);
            this.txtRawHex.Name = "txtRawHex";
            this.txtRawHex.PlaceholderText = "AA 01 00 01 55";
            this.txtRawHex.Size = new System.Drawing.Size(356, 23);
            this.txtRawHex.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(530, 321);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(257, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Raw frame (AA .. 55)";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(441, 370);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 25);
            this.btnClear.TabIndex = 6;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(902, 406);
            this.Controls.Add(this.btnSendRaw);
            this.Controls.Add(this.txtRawHex);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.grpQuick);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.grpSend);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtHost);
            this.Name = "MainForm";
            this.Text = "MiniHex Client (WinForms) - Request/Response";
            this.grpSend.ResumeLayout(false);
            this.grpSend.PerformLayout();
            this.grpQuick.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.GroupBox grpSend;
        private System.Windows.Forms.Button btnBuildSend;
        private System.Windows.Forms.TextBox txtPayloadHex;
        private System.Windows.Forms.TextBox txtCmdHex;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpQuick;
        private System.Windows.Forms.Button btnHello;
        private System.Windows.Forms.Button btnGetStatus;
        private System.Windows.Forms.Button btnGetNumber;
        private System.Windows.Forms.Button btnSetStatus1;
        private System.Windows.Forms.Button btnSetStatus2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSendRaw;
        private System.Windows.Forms.TextBox txtRawHex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnClear;
    }
}