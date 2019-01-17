namespace MealCardCollection
{
    partial class LocalConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalConfig));
            this.label1 = new System.Windows.Forms.Label();
            this.serverTbx = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dbnameTbx = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.userTbx = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pwdTbx = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.machTbx = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "本地服务器：";
            // 
            // serverTbx
            // 
            this.serverTbx.Location = new System.Drawing.Point(96, 40);
            this.serverTbx.Name = "serverTbx";
            this.serverTbx.Size = new System.Drawing.Size(123, 21);
            this.serverTbx.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "用户名：";
            // 
            // dbnameTbx
            // 
            this.dbnameTbx.Location = new System.Drawing.Point(96, 84);
            this.dbnameTbx.Name = "dbnameTbx";
            this.dbnameTbx.Size = new System.Drawing.Size(123, 21);
            this.dbnameTbx.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 178);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "密码：";
            // 
            // userTbx
            // 
            this.userTbx.Location = new System.Drawing.Point(96, 126);
            this.userTbx.Name = "userTbx";
            this.userTbx.Size = new System.Drawing.Size(123, 21);
            this.userTbx.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "数据库名称：";
            // 
            // pwdTbx
            // 
            this.pwdTbx.Location = new System.Drawing.Point(95, 169);
            this.pwdTbx.Name = "pwdTbx";
            this.pwdTbx.Size = new System.Drawing.Size(123, 21);
            this.pwdTbx.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 221);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "消费机端口：";
            // 
            // machTbx
            // 
            this.machTbx.Location = new System.Drawing.Point(95, 218);
            this.machTbx.Name = "machTbx";
            this.machTbx.ReadOnly = true;
            this.machTbx.Size = new System.Drawing.Size(123, 21);
            this.machTbx.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.button1.Location = new System.Drawing.Point(50, 275);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(118, 48);
            this.button1.TabIndex = 10;
            this.button1.Text = "保存本地配置";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LocalConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(231, 349);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.machTbx);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pwdTbx);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.userTbx);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dbnameTbx);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.serverTbx);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "LocalConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "本地配置设置";
            this.Load += new System.EventHandler(this.LocalConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverTbx;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox dbnameTbx;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox userTbx;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox pwdTbx;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox machTbx;
        private System.Windows.Forms.Button button1;
    }
}