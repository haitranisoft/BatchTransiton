using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatchTransition
{
    public partial class FormSelectMixer : Form
    {
        public MixerUnit SelectedMixer { get; private set; }

        public FormSelectMixer(List<MixerUnit> mixers)
        {
            InitializeComponent();  

            this.Text = "Chọn Mixer";
            this.ClientSize = new Size(386, 340);
            this.MinimumSize = new Size(386, 340);
            this.MaximumSize = new Size(386, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);

            var lblHint = new Label
            {
                Text = "Chọn Mixer đã MixDone:",
                Location = new Point(12, 10),
                Size = new Size(360, 22),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            var lstMixers = new ListBox
            {
                Location = new Point(12, 38),
                Size = new Size(360, 230),
                DataSource = mixers,
                DisplayMember = "Name",
                Font = new Font("Segoe UI", 10f)
            };

            var btnOK = new Button
            {
                Text = "✔  Chọn",
                Location = new Point(12, 282),
                Size = new Size(170, 38),
                BackColor = Color.FromArgb(0, 120, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnOK.Click += (s, e) =>
            {
                SelectedMixer = lstMixers.SelectedItem as MixerUnit;
                if (SelectedMixer == null) { MessageBox.Show("Chọn 1 Mixer!"); return; }
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new Button
            {
                Text = "✖  Hủy",
                Location = new Point(202, 282),
                Size = new Size(170, 38),
                BackColor = Color.FromArgb(160, 60, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] { lblHint, lstMixers, btnOK, btnCancel });
        }
    }
}
