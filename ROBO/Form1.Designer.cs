namespace ROBO
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.Server = new System.Windows.Forms.TextBox();
            this.Company = new System.Windows.Forms.TextBox();
            this.DbUser = new System.Windows.Forms.TextBox();
            this.DbPass = new System.Windows.Forms.TextBox();
            this.UserSap = new System.Windows.Forms.TextBox();
            this.PassSAP = new System.Windows.Forms.TextBox();
            this.License = new System.Windows.Forms.TextBox();
            this.DbServerType = new System.Windows.Forms.ComboBox();
            this.btnMarvi = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(5, 277);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "OP";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(91, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(623, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Conexão Jason:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(86, 277);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(123, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Testar Conexão SAP";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Server";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "CompanyDB";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 92);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "DbUserName";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "DbPassword";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 143);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "UserSAP";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 169);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "PasswordSAP";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 195);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "DbServerType";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 221);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "LicenseServer";
            // 
            // Server
            // 
            this.Server.Location = new System.Drawing.Point(91, 36);
            this.Server.Name = "Server";
            this.Server.Size = new System.Drawing.Size(189, 20);
            this.Server.TabIndex = 12;
            this.Server.Text = "Mheydimaq1";
            // 
            // Company
            // 
            this.Company.Location = new System.Drawing.Point(91, 62);
            this.Company.Name = "Company";
            this.Company.Size = new System.Drawing.Size(189, 20);
            this.Company.TabIndex = 13;
            this.Company.Text = "SBODemoBR";
            // 
            // DbUser
            // 
            this.DbUser.Location = new System.Drawing.Point(91, 85);
            this.DbUser.Name = "DbUser";
            this.DbUser.Size = new System.Drawing.Size(189, 20);
            this.DbUser.TabIndex = 14;
            this.DbUser.Text = "sa";
            // 
            // DbPass
            // 
            this.DbPass.Location = new System.Drawing.Point(91, 110);
            this.DbPass.Name = "DbPass";
            this.DbPass.Size = new System.Drawing.Size(189, 20);
            this.DbPass.TabIndex = 15;
            this.DbPass.Text = "ramo01";
            // 
            // UserSap
            // 
            this.UserSap.Location = new System.Drawing.Point(91, 136);
            this.UserSap.Name = "UserSap";
            this.UserSap.Size = new System.Drawing.Size(189, 20);
            this.UserSap.TabIndex = 16;
            this.UserSap.Text = "manager";
            // 
            // PassSAP
            // 
            this.PassSAP.Location = new System.Drawing.Point(91, 162);
            this.PassSAP.Name = "PassSAP";
            this.PassSAP.Size = new System.Drawing.Size(189, 20);
            this.PassSAP.TabIndex = 17;
            this.PassSAP.Text = "ramo01";
            // 
            // License
            // 
            this.License.Location = new System.Drawing.Point(91, 214);
            this.License.Name = "License";
            this.License.Size = new System.Drawing.Size(189, 20);
            this.License.TabIndex = 19;
            this.License.Text = "Mheydimaq1:30000";
            // 
            // DbServerType
            // 
            this.DbServerType.FormattingEnabled = true;
            this.DbServerType.Items.AddRange(new object[] {
            "dst_MSSQL2008",
            "dst_MSSQL2012",
            "dst_MSSQL2014",
            "dst_HANADB"});
            this.DbServerType.Location = new System.Drawing.Point(91, 188);
            this.DbServerType.Name = "DbServerType";
            this.DbServerType.Size = new System.Drawing.Size(189, 21);
            this.DbServerType.TabIndex = 20;
            this.DbServerType.Text = "dst_MSSQL2014";
            // 
            // btnMarvi
            // 
            this.btnMarvi.Location = new System.Drawing.Point(215, 277);
            this.btnMarvi.Name = "btnMarvi";
            this.btnMarvi.Size = new System.Drawing.Size(100, 23);
            this.btnMarvi.TabIndex = 21;
            this.btnMarvi.Text = "MarviCampos";
            this.btnMarvi.UseVisualStyleBackColor = true;
            this.btnMarvi.Click += new System.EventHandler(this.button4_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(482, 60);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(262, 148);
            this.richTextBox1.TabIndex = 32;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Insert",
            "Update"});
            this.comboBox1.Location = new System.Drawing.Point(482, 36);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(181, 21);
            this.comboBox1.TabIndex = 34;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(7, 304);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(308, 23);
            this.progressBar1.TabIndex = 35;
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Location = new System.Drawing.Point(12, 330);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(34, 13);
            this.lblMsg.TabIndex = 36;
            this.lblMsg.Text = "MSG:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 351);
            this.Controls.Add(this.lblMsg);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnMarvi);
            this.Controls.Add(this.DbServerType);
            this.Controls.Add(this.License);
            this.Controls.Add(this.PassSAP);
            this.Controls.Add(this.UserSap);
            this.Controls.Add(this.DbPass);
            this.Controls.Add(this.DbUser);
            this.Controls.Add(this.Company);
            this.Controls.Add(this.Server);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Robo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox Server;
        private System.Windows.Forms.TextBox Company;
        private System.Windows.Forms.TextBox DbUser;
        private System.Windows.Forms.TextBox DbPass;
        private System.Windows.Forms.TextBox UserSap;
        private System.Windows.Forms.TextBox PassSAP;
        private System.Windows.Forms.TextBox License;
        private System.Windows.Forms.ComboBox DbServerType;
        private System.Windows.Forms.Button btnMarvi;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblMsg;
    }
}

