namespace BatchTransition
{
    partial class Form1
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblPlcStatus = new System.Windows.Forms.Label();
            this.lblPendingBatch = new System.Windows.Forms.Label();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnMixDone = new System.Windows.Forms.ToolStripButton();
            this.btnTransition = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnShowJourney = new System.Windows.Forms.ToolStripButton();
            this.sep2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnReset = new System.Windows.Forms.ToolStripButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabMachines = new System.Windows.Forms.TabPage();
            this.pnlLineCards = new System.Windows.Forms.Panel();
            this.tabTanks = new System.Windows.Forms.TabPage();
            this.dgvTanks = new System.Windows.Forms.DataGridView();
            this.tabJourney = new System.Windows.Forms.TabPage();
            this.dgvJourney = new System.Windows.Forms.DataGridView();
            this.pnlLog = new System.Windows.Forms.Panel();
            this.lstLog = new System.Windows.Forms.ListBox();

            this.pnlHeader.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabMachines.SuspendLayout();
            this.tabTanks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTanks)).BeginInit();
            this.tabJourney.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJourney)).BeginInit();
            this.pnlLog.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 48;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblPlcStatus);
            this.pnlHeader.Controls.Add(this.lblPendingBatch);

            this.lblTitle.Text = "Batch Transition SCADA";
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold);
            this.lblTitle.AutoSize = false;
            this.lblTitle.Location = new System.Drawing.Point(10, 10);
            this.lblTitle.Size = new System.Drawing.Size(600, 28);

            this.lblPlcStatus.Text = "● PLC: --";
            this.lblPlcStatus.ForeColor = System.Drawing.Color.Silver;
            this.lblPlcStatus.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblPlcStatus.AutoSize = false;
            this.lblPlcStatus.Location = new System.Drawing.Point(620, 14);
            this.lblPlcStatus.Size = new System.Drawing.Size(220, 22);

            this.lblPendingBatch.Text = "⏳ Pending: -";
            this.lblPendingBatch.ForeColor = System.Drawing.Color.Silver;
            this.lblPendingBatch.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.lblPendingBatch.AutoSize = false;
            this.lblPendingBatch.Location = new System.Drawing.Point(860, 14);
            this.lblPendingBatch.Size = new System.Drawing.Size(460, 22);

            // ── toolStrip ─────────────────────────────────────────────────────
            this.toolStrip.BackColor = System.Drawing.Color.FromArgb(30, 80, 140);
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStrip.Font = new System.Drawing.Font("Segoe UI", 9f);

            this.btnMixDone.Text = "✔  MixDone";
            this.btnMixDone.ForeColor = System.Drawing.Color.White;
            this.btnMixDone.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnMixDone.Click += new System.EventHandler(this.btnMixDone_Click);

            this.btnTransition.Text = "▶  Transition";
            this.btnTransition.ForeColor = System.Drawing.Color.White;
            this.btnTransition.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnTransition.Click += new System.EventHandler(this.btnTransition_Click);

            this.btnStop.Text = "⏹  Stop Sim";
            this.btnStop.ForeColor = System.Drawing.Color.FromArgb(255, 180, 100);
            this.btnStop.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            this.btnShowJourney.Text = "📋  Batch Journey";
            this.btnShowJourney.ForeColor = System.Drawing.Color.White;
            this.btnShowJourney.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnShowJourney.Click += new System.EventHandler(this.btnShowJourney_Click);

            this.btnReset.Text = "🔄  Reset";
            this.btnReset.ForeColor = System.Drawing.Color.FromArgb(255, 120, 120);
            this.btnReset.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.btnReset.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.btnMixDone,
                this.btnTransition,
                this.btnStop,
                this.sep1,
                this.btnShowJourney,
                this.sep2,
                this.btnReset
            });

            // ── pnlLog ────────────────────────────────────────────────────────
            this.pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLog.Height = 110;
            this.pnlLog.Controls.Add(this.lstLog);

            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.Font = new System.Drawing.Font("Courier New", 8.5f);
            this.lstLog.BackColor = System.Drawing.Color.FromArgb(15, 20, 30);
            this.lstLog.ForeColor = System.Drawing.Color.FromArgb(180, 230, 180);
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.ItemHeight = 18;

            // ── tabControl ────────────────────────────────────────────────────
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.tabControl.Padding = new System.Drawing.Point(10, 4);
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[]
            {
                this.tabMachines,
                this.tabTanks,
                this.tabJourney
            });

            // ── tabMachines ───────────────────────────────────────────────────
            this.tabMachines.Text = "🏭  Machine Status";
            this.tabMachines.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);
            this.tabMachines.Controls.Add(this.pnlLineCards);

            this.pnlLineCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLineCards.AutoScroll = true;
            this.pnlLineCards.Padding = new System.Windows.Forms.Padding(8);
            this.pnlLineCards.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);

            // ── tabTanks ──────────────────────────────────────────────────────
            this.tabTanks.Text = "🛢  Storage Tanks";
            this.tabTanks.Padding = new System.Windows.Forms.Padding(3);
            this.tabTanks.Controls.Add(this.dgvTanks);

            this.dgvTanks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTanks.AllowUserToAddRows = false;
            this.dgvTanks.AllowUserToDeleteRows = false;
            this.dgvTanks.ReadOnly = true;
            this.dgvTanks.RowHeadersVisible = false;
            this.dgvTanks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTanks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvTanks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTanks.ColumnHeadersHeight = 28;
            this.dgvTanks.RowTemplate.Height = 24;
            this.dgvTanks.Font = new System.Drawing.Font("Segoe UI", 9f);

            // ── tabJourney ────────────────────────────────────────────────────
            this.tabJourney.Text = "🔄  Batch Journey";
            this.tabJourney.Padding = new System.Windows.Forms.Padding(3);
            this.tabJourney.Controls.Add(this.dgvJourney);

            this.dgvJourney.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJourney.AllowUserToAddRows = false;
            this.dgvJourney.AllowUserToDeleteRows = false;
            this.dgvJourney.ReadOnly = true;
            this.dgvJourney.RowHeadersVisible = false;
            this.dgvJourney.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvJourney.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvJourney.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvJourney.ColumnHeadersHeight = 28;
            this.dgvJourney.RowTemplate.Height = 24;
            this.dgvJourney.Font = new System.Drawing.Font("Segoe UI", 9f);

            // ── Form ──────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 860);
            this.MinimumSize = new System.Drawing.Size(1100, 720);
            this.Text = "Batch Transition SCADA";
            this.BackColor = System.Drawing.Color.FromArgb(240, 243, 248);
            this.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlLog);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.pnlHeader);

            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);

            this.pnlHeader.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTanks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJourney)).EndInit();
            this.tabMachines.ResumeLayout(false);
            this.tabTanks.ResumeLayout(false);
            this.tabJourney.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.pnlLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPlcStatus;
        private System.Windows.Forms.Label lblPendingBatch;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnMixDone;
        private System.Windows.Forms.ToolStripButton btnTransition;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripButton btnShowJourney;
        private System.Windows.Forms.ToolStripSeparator sep2;
        private System.Windows.Forms.ToolStripButton btnReset;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabMachines;
        private System.Windows.Forms.Panel pnlLineCards;
        private System.Windows.Forms.TabPage tabTanks;
        private System.Windows.Forms.DataGridView dgvTanks;
        private System.Windows.Forms.TabPage tabJourney;
        private System.Windows.Forms.DataGridView dgvJourney;
        private System.Windows.Forms.Panel pnlLog;
        private System.Windows.Forms.ListBox lstLog;
    }
}
