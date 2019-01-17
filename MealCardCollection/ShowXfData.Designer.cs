namespace MealCardCollection
{
    partial class ShowXfData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowXfData));
            this.listView1 = new System.Windows.Forms.ListView();
            this.space = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cardno = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.xfmoney = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.xftime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.xfupload = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.space,
            this.cardno,
            this.xfmoney,
            this.xftime,
            this.xfupload});
            this.listView1.Location = new System.Drawing.Point(-4, 1);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(383, 528);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // space
            // 
            this.space.Text = "编号";
            this.space.Width = 50;
            // 
            // cardno
            // 
            this.cardno.Text = "卡号";
            this.cardno.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // xfmoney
            // 
            this.xfmoney.Text = "消费金额";
            this.xfmoney.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // xftime
            // 
            this.xftime.Text = "消费时间";
            this.xftime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.xftime.Width = 120;
            // 
            // xfupload
            // 
            this.xfupload.Text = "是否上传";
            this.xfupload.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ShowXfData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 528);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ShowXfData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "消费数据上传概览";
            this.Load += new System.EventHandler(this.ShowXfData_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader space;
        private System.Windows.Forms.ColumnHeader cardno;
        private System.Windows.Forms.ColumnHeader xfmoney;
        private System.Windows.Forms.ColumnHeader xftime;
        private System.Windows.Forms.ColumnHeader xfupload;
    }
}