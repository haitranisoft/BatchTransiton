using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BatchTransition
{
    public partial class FormAssignTankLines : Form
    {
        public Dictionary<StorageTank, List<ProductionLine>> Assignments
            = new Dictionary<StorageTank, List<ProductionLine>>();

        private readonly List<StorageTank> _tanks;
        private readonly List<ProductionLine> _lines;
        private readonly Func<ProductionLine, bool> _isBusy;

        private readonly Dictionary<StorageTank, CheckedListBox> _clbMap
            = new Dictionary<StorageTank, CheckedListBox>();

        public FormAssignTankLines(
            List<StorageTank> tanks,
            List<ProductionLine> lines,
            Func<ProductionLine, bool> isBusy)
        {
            _tanks = tanks;
            _lines = lines;
            _isBusy = isBusy;

            this.Text = "Assign Tank → Line";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(500, 400);
            this.AutoScroll = true;

            BuildUI();
        }

        private void BuildUI()
        {
            int y = 12;

            var title = new Label
            {
                Text = $"Assign {_tanks.Count} tank vào Production Line:",
                Location = new Point(12, y),
                Size = new Size(460, 24),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 51, 102)
            };
            Controls.Add(title);
            y += 32;

            var legend = new Label
            {
                Text = "✅ Line trống – assign ngay    ⏳ [BUSY] – đưa vào queue, tự động chạy khi line xong",
                Location = new Point(12, y),
                Size = new Size(460, 20),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            Controls.Add(legend);
            y += 28;

            foreach (var tank in _tanks)
            {
                var grp = new GroupBox
                {
                    Text = $"  {tank.Name}  →  chọn Line(s):",
                    Location = new Point(12, y),
                    Size = new Size(460, _lines.Count * 26 + 36),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 80, 140)
                };
                Controls.Add(grp);

                var clb = new CheckedListBox
                {
                    Location = new Point(10, 20),
                    Size = new Size(436, _lines.Count * 26),
                    CheckOnClick = true,
                    Font = new Font("Segoe UI", 9.5f),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.None
                };

                foreach (var line in _lines)
                {
                    bool busy = _isBusy(line);
                    string display = busy
                        ? $"⏳ {line.Name}  [BUSY – sẽ queue]"
                        : $"✅ {line.Name}";
                    clb.Items.Add(new LineItem(line, display));
                }

                grp.Controls.Add(clb);
                _clbMap[tank] = clb;

                y += grp.Height + 10;
            }

            y += 4;
            var btnOK = new Button
            {
                Text = "✔  Xác nhận",
                Location = new Point(12, y),
                Size = new Size(220, 42),
                BackColor = Color.FromArgb(0, 120, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += BtnOK_Click;
            Controls.Add(btnOK);

            var btnCancel = new Button
            {
                Text = "✖  Huỷ",
                Location = new Point(246, y),
                Size = new Size(160, 42),
                BackColor = Color.FromArgb(180, 50, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);

            this.ClientSize = new Size(490, y + 58);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            foreach (var tank in _tanks)
            {
                if (_clbMap[tank].CheckedItems.Count == 0)
                {
                    MessageBox.Show(
                        $"{tank.Name} chưa được assign Line nào!\nVui lòng chọn ít nhất 1 Line.",
                        "Thiếu Line", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            Assignments.Clear();
            foreach (var tank in _tanks)
            {
                var selectedLines = _clbMap[tank].CheckedItems
                    .Cast<LineItem>()
                    .Select(li => li.Line)
                    .ToList();
                Assignments[tank] = selectedLines;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        private class LineItem
        {
            public ProductionLine Line { get; }
            private readonly string _display;
            public LineItem(ProductionLine line, string display) { Line = line; _display = display; }
            public override string ToString() => _display;
        }
    }
}
