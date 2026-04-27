namespace BatchTransition
{
    partial class FormSelectTank
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.clbTank = new System.Windows.Forms.CheckedListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // lblHint
            this.lblHint.Text = "Chọn một hoặc nhiều Storage Tank:";
            this.lblHint.Location = new System.Drawing.Point(12, 12);
            this.lblHint.Size = new System.Drawing.Size(360, 22);
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);

            // clbTank
            this.clbTank.Location = new System.Drawing.Point(12, 40);
            this.clbTank.Size = new System.Drawing.Size(360, 230);
            this.clbTank.CheckOnClick = true;
            this.clbTank.Font = new System.Drawing.Font("Segoe UI", 10f);
            this.clbTank.TabIndex = 0;

            // btnOK
            this.btnOK.Text = "✔  Chọn";
            this.btnOK.Location = new System.Drawing.Point(12, 285);
            this.btnOK.Size = new System.Drawing.Size(170, 40);
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(0, 120, 60);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.TabIndex = 1;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // button1 (Cancel)
            this.button1.Text = "✖  Hủy";
            this.button1.Location = new System.Drawing.Point(202, 285);
            this.button1.Size = new System.Drawing.Size(170, 40);
            this.button1.BackColor = System.Drawing.Color.FromArgb(180, 50, 40);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 10f);
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.TabIndex = 2;
            this.button1.Click += new System.EventHandler(this.button1_Click);

            // Form  – KHÔNG đặt MaximumSize để tránh cắt nút
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 340);
            this.MinimumSize = new System.Drawing.Size(386, 380);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chọn Storage Tank";
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.clbTank);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.button1);
            this.Load += new System.EventHandler(this.FormSelectTank_Load);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.CheckedListBox clbTank;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblHint;
    }


}