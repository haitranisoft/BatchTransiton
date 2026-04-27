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
    public partial class FormSelectLine : Form
    {
        public List<ProductionLine> SelectedLines { get; } = new List<ProductionLine>();

        public FormSelectLine(List<ProductionLine> lines)
        {
            InitializeComponent();
            this.Text = "Chọn Production Line";

            foreach (var l in lines)
                clbLine.Items.Add(l);
        }

        private void FormSelectLine_Load(object sender, EventArgs e) { }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SelectedLines.Clear();
            foreach (var item in clbLine.CheckedItems)
                if (item is ProductionLine l) SelectedLines.Add(l);

            if (SelectedLines.Count == 0)
            { MessageBox.Show("Chọn ít nhất 1 Line!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

}
