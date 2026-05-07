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
    public partial class FormSelectTank : Form
    {
        public List<StorageTank> SelectedTanks { get; } = new List<StorageTank>();

        public FormSelectTank(List<StorageTank> tanks)
        {
            InitializeComponent();
            this.Text = "Chọn Storage Tank";

            foreach (var t in tanks)
                clbTank.Items.Add(t);
        }

        private void FormSelectTank_Load(object sender, EventArgs e) { }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SelectedTanks.Clear();
            foreach (var item in clbTank.CheckedItems)
                if (item is StorageTank t) SelectedTanks.Add(t);

            if (SelectedTanks.Count == 0)
            { MessageBox.Show("Chọn ít nhất 1 Tank!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

}
