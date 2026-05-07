namespace BatchTransition
{
    partial class FormSelectLine
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
            this.clbLine = new System.Windows.Forms.CheckedListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // lblHint
            this.lblHint.Text = "Chọn một hoặc nhiều Production Line:";
            this.lblHint.Location = new System.Drawing.Point(12, 12);
            this.lblHint.Size = new System.Drawing.Size(360, 22);
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);

            // clbLine
            this.clbLine.Location = new System.Drawing.Point(12, 40);
            this.clbLine.Size = new System.Drawing.Size(360, 175);
            this.clbLine.CheckOnClick = true;
            this.clbLine.Font = new System.Drawing.Font("Segoe UI", 10f);
            this.clbLine.TabIndex = 0;

            // btnOK
            this.btnOK.Text = "✔  Chọn";
            this.btnOK.Location = new System.Drawing.Point(12, 230);
            this.btnOK.Size = new System.Drawing.Size(170, 40);
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(0, 120, 60);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.TabIndex = 1;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.Text = "✖  Hủy";
            this.btnCancel.Location = new System.Drawing.Point(202, 230);
            this.btnCancel.Size = new System.Drawing.Size(170, 40);
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(180, 50, 40);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10f);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // Form  – KHÔNG đặt MaximumSize
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 285);
            this.MinimumSize = new System.Drawing.Size(386, 325);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chọn Production Line";
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.clbLine);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Load += new System.EventHandler(this.FormSelectLine_Load);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.CheckedListBox clbLine;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblHint;
    }


}