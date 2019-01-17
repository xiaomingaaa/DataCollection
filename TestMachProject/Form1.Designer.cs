namespace TestMachProject
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.dataList1 = new Comm.DataList();
            this.ipaddr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dumpcount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastdump = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataList1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(416, 425);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "获取记录";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(500, 41);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(381, 305);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // dataList1
            // 
            this.dataList1.BindMode = 0;
            this.dataList1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataList1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ipaddr,
            this.dumpcount,
            this.lastdump});
            this.dataList1.listMode = 0;
            this.dataList1.Location = new System.Drawing.Point(27, 41);
            this.dataList1.Name = "dataList1";
            this.dataList1.RowTemplate.Height = 23;
            this.dataList1.Size = new System.Drawing.Size(358, 316);
            this.dataList1.TabIndex = 2;
            // 
            // ipaddr
            // 
            this.ipaddr.HeaderText = "IP地址";
            this.ipaddr.Name = "ipaddr";
            // 
            // dumpcount
            // 
            this.dumpcount.HeaderText = "心跳数";
            this.dumpcount.Name = "dumpcount";
            // 
            // lastdump
            // 
            this.lastdump.HeaderText = "最后时间";
            this.lastdump.Name = "lastdump";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(592, 424);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "发送黑名单";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1051, 541);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.dataList1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataList1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Timer timer1;
        private Comm.DataList dataList1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ipaddr;
        private System.Windows.Forms.DataGridViewTextBoxColumn dumpcount;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastdump;
        private System.Windows.Forms.Button button2;
    }
}

