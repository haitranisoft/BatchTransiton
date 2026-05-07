using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BatchTransition
{
    public partial class Form1 : Form
    {
        private List<MixerUnit> mixers = new List<MixerUnit>();
        private List<StorageTank> tanks = new List<StorageTank>();
        private List<ProductionLine> lines = new List<ProductionLine>();

        private Dictionary<StorageTank, List<ProductionLine>> tankLineMap
            = new Dictionary<StorageTank, List<ProductionLine>>();

        private List<(StorageTank tank, List<ProductionLine> lines)> pendingList
            = new List<(StorageTank, List<ProductionLine>)>();

        private List<LineCardControl> lineCards = new List<LineCardControl>();
        private BatchPropagationService propagation = new BatchPropagationService();
        private PlcService plcService = new PlcService();

        private Timer plcTimer = new Timer { Interval = 500 };
        private Timer autoTimer = new Timer { Interval = 800 };

        private Dictionary<string, bool> prevMixDone = new Dictionary<string, bool>();
        private Dictionary<string, bool> prevTransition = new Dictionary<string, bool>();
        private Dictionary<string, bool> prevTransferFiller = new Dictionary<string, bool>();
        private Dictionary<string, bool> prevChangeOver = new Dictionary<string, bool>();
        private Dictionary<string, int> simCounters = new Dictionary<string, int>();

        private static readonly Color C_BATCH = Color.FromArgb(198, 230, 255);
        private static readonly Color C_DONE = Color.FromArgb(210, 245, 210);
        private static readonly Color C_PEND = Color.FromArgb(255, 245, 200);

        public Form1()
        {
            InitializeComponent();
            InitData();
            InitPlc();
            plcTimer.Tick += PlcLoop; plcTimer.Start();
            autoTimer.Tick += AutoRun;
            propagation.OnBatchEvent += msg => AppendLog(msg);
        }
        private void InitData()
        {
            mixers.Add(BuildMixer("Mixer T1-1", "DB30.DBX0.0", "DB30.DBB2", "DB30.DBB22", "DB30.DBB42", "DB30.DBD62"));
            for (int i = 1; i <= 5; i++) { int db = 30 + i; mixers.Add(BuildMixer($"Mixer T2-{i}", $"DB{db}.DBX0.0", $"DB{db}.DBB2", $"DB{db}.DBB22", $"DB{db}.DBB42", $"DB{db}.DBD62")); }
            for (int i = 1; i <= 8; i++) { int db = 40 + i; tanks.Add(new StorageTank { Name = $"ST{i}", PlcTransitionBitAddress = $"DB{db}.DBX0.0", PlcTransferFillerStateAddress = $"DB{db}.DBX0.1" }); }
            string[] dn = { "Checkweigher", "Capper", "Labeller", "Case_Packer", "Case_Erector", "Case_Sealer", "Cobot" };
            for (int i = 1; i <= 5; i++)
            {
                int db = 10 + (i - 1) * 4;
                var f = new Machine { Name = $"Filler L{i}", PlcCounterAddress = $"DB{db}.DBD0", PlcChangeOverAddress = $"DB{db}.DBX4.0", PlcItemCodeAddress = $"DB{db}.DBB8", PlcMachineStatusAddress = $"DB{db}.DBW28" };
                var line = new ProductionLine { Name = $"Line {i}", Filler = f };
                foreach (var d in dn) line.Machines.Add(new Machine { Name = d });
                lines.Add(line); simCounters[line.Name] = 0;
            }
            foreach (var m in mixers) prevMixDone[m.Name] = false;
            foreach (var t in tanks) { prevTransition[t.Name] = false; prevTransferFiller[t.Name] = false; }
            foreach (var l in lines) prevChangeOver[l.Name] = false;
        }

        private MixerUnit BuildMixer(string n, string md, string bc, string po, string ic, string q) => new MixerUnit { Name = n, PlcMixDoneBitAddress = md, PlcBatchCodeAddress = bc, PlcPoAddress = po, PlcItemCodeAddress = ic, PlcTargetQtyAddress = q };

        private void InitPlc()
        {
            bool ok = plcService.Connect("192.168.10.2", "192.168.0.20", 102);
            lblPlcStatus.Text = ok ? "● PLC: Connected" : "● PLC: Offline (Demo Mode)";
            lblPlcStatus.ForeColor = ok ? Color.Lime : Color.OrangeRed;
        }

        private void ResetAll()
        {
            autoTimer.Stop();

            foreach (var line in lines)
            {
                line.Filler.Buffer.Clear();
                line.Filler.CounterOut = 0;
                line.Filler.BatchCounterBase = 0;
                line.Filler.LastItemCode = "";
                foreach (var m in line.Machines) m.Buffer.Clear();
                simCounters[line.Name] = 0;
            }

            foreach (var tank in tanks)
            {
                tank.BatchQueue.Clear();
                tank.SourceMixer = "";
            }

            foreach (var mx in mixers) mx.PendingBatches.Clear();

            tankLineMap.Clear();
            pendingList.Clear();
            propagation.Journeys.Clear();

            lblPendingBatch.Text = "⏳ Pending: -";
            lblPendingBatch.ForeColor = Color.Silver;

            RefreshAll();
            AppendLog("═══════ RESET ═══════ Tất cả đã được reset.");
        }

        private void PlcLoop(object sender, EventArgs e)
        {
            if (!plcService.IsConnected) return;
            var mixBits = plcService.ReadAllMixDoneBits(mixers);
            foreach (var mx in mixers) { bool c = mixBits.ContainsKey(mx.Name) && mixBits[mx.Name]; if (!prevMixDone[mx.Name] && c) OnMixDoneFromPlc(mx); prevMixDone[mx.Name] = c; }
            var tb = plcService.ReadAllTransitionBits(tanks); var tf = plcService.ReadAllTransferFillerStates(tanks);
            foreach (var tank in tanks) { bool c1 = tb.ContainsKey(tank.Name) && tb[tank.Name]; if (!prevTransition[tank.Name] && c1) { var al = GetLinesForTank(tank); if (al.Count > 0 && tank.BatchQueue.Count > 0) { propagation.OnTransitionBatch(tank, al); this.Invoke((Action)(() => { autoTimer.Start(); RefreshAll(); })); } } prevTransition[tank.Name] = c1; bool c2 = tf.ContainsKey(tank.Name) && tf[tank.Name]; if (!prevTransferFiller[tank.Name] && c2) { var al = GetLinesForTank(tank); if (al.Count > 0 && tank.BatchQueue.Count > 0) { propagation.OnTransferFillerState(tank, al); this.Invoke((Action)(() => { autoTimer.Start(); RefreshAll(); })); } } prevTransferFiller[tank.Name] = c2; }
            var ctr = plcService.ReadAllCounterOuts(lines); var co = plcService.ReadAllChangeOvers(lines); var ic = plcService.ReadAllFillerItemCodes(lines);
            foreach (var line in lines) { int raw = ctr.ContainsKey(line.Name) ? ctr[line.Name] : 0; var tank = GetActiveTank(line); propagation.UpdateCounterOut(line.Filler, raw, tank); bool curCO = co.ContainsKey(line.Name) && co[line.Name]; if (!prevChangeOver[line.Name] && curCO) propagation.OnChangeOver(line.Filler, tank, raw); prevChangeOver[line.Name] = curCO; if (ic.ContainsKey(line.Name)) propagation.OnItemCodeChange(line.Filler, ic[line.Name], tank, raw); }
        }

        private void OnMixDoneFromPlc(MixerUnit mx)
        {
            string bc = plcService.ReadMixerString(mx.PlcBatchCodeAddress);
            string it = plcService.ReadMixerString(mx.PlcItemCodeAddress);
            string po = plcService.ReadMixerString(mx.PlcPoAddress);
            int q = plcService.ReadMixerInt(mx.PlcTargetQtyAddress);
            if (string.IsNullOrEmpty(bc)) bc = $"{mx.Name.Replace(" ", "")}_B{DateTime.Now:yyMMdd_HHmmss}";
            if (string.IsNullOrEmpty(it)) it = "ITEM_AUTO"; if (string.IsNullOrEmpty(po)) po = "PO_AUTO"; if (q <= 0) q = 1000;
            propagation.OnMixDone(mx, new Batch { BatchCode = bc, ItemCode = it, ProductionOrder = po, TargetQty = q });
            this.Invoke((Action)(() => { lblPendingBatch.Text = $"⏳ Pending: {bc}"; lblPendingBatch.ForeColor = Color.Orange; PromptAssign(mx); }));
        }

        private bool IsFillerBusy(ProductionLine l) => l.Filler.Buffer.Count > 0;

        private StorageTank GetActiveTank(ProductionLine line)
            => tankLineMap.Where(kv => kv.Value.Contains(line)).Select(kv => kv.Key).FirstOrDefault();

        private List<ProductionLine> GetLinesForTank(StorageTank tank)
            => tankLineMap.ContainsKey(tank) ? tankLineMap[tank] : new List<ProductionLine>();

        private bool IsLineReserved(ProductionLine line)
        {
            if (tankLineMap.Any(kv => kv.Value.Contains(line))) return true;

            if (pendingList.Any(p => p.lines.Contains(line))) return true;
            return false;
        }
        private void PromptAssign(MixerUnit mixer)
        {
            if (mixer.PendingBatches.Count == 0) { MessageBox.Show("Không có batch đang chờ!"); return; }

            var tf = new FormSelectTank(tanks);
            if (tf.ShowDialog() != DialogResult.OK || tf.SelectedTanks.Count == 0) { MessageBox.Show("Chưa chọn Tank!"); return; }

            var af = new FormAssignTankLines(tf.SelectedTanks, lines,
                l => IsFillerBusy(l) || IsLineReserved(l));
            if (af.ShowDialog() != DialogResult.OK) return;

            propagation.AssignBatchToTanks(mixer, tf.SelectedTanks);

            foreach (var kv in af.Assignments)
            {
                var tank = kv.Key;
                var targetLines = kv.Value;

                var canStart = targetLines.Where(l => !IsFillerBusy(l) && !IsLineReserved(l)).ToList();
                var mustWait = targetLines.Where(l => IsFillerBusy(l) || IsLineReserved(l)).ToList();

                if (canStart.Count == targetLines.Count)
                {
                    tankLineMap[tank] = targetLines.ToList();
                    AppendLog($"[Assign] {tank.Name} → {Fmt(targetLines)} (ngay)");
                }
                else
                {
                    pendingList.Add((tank, targetLines.ToList()));
                    AppendLog($"[Queue]  {tank.Name} → {Fmt(targetLines)} (chờ: {Fmt(mustWait)} bận/reserved)");
                }
            }

            lblPendingBatch.Text = "⏳ Pending: -"; lblPendingBatch.ForeColor = Color.Silver;
            RefreshAll();
        }

        private void TryStartPending()
        {
            if (pendingList.Count == 0) return;

            for (int i = 0; i < pendingList.Count; i++)
            {
                var (tank, targetLines) = pendingList[i];

                bool ready = targetLines.All(l =>
                    l.Filler.Buffer.Count == 0
                    && !tankLineMap.Any(kv => kv.Value.Contains(l) && kv.Key != tank));

                if (!ready) continue;

                pendingList.RemoveAt(i); i--;

                foreach (var line in targetLines)
                {
                    var old = tankLineMap.Where(kv => kv.Value.Contains(line)).Select(kv => kv.Key).ToList();
                    foreach (var t in old) tankLineMap[t].Remove(line);
                }
                var empty = tankLineMap.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();
                foreach (var t in empty) tankLineMap.Remove(t);

                tankLineMap[tank] = targetLines.ToList();
                AppendLog($"[AutoStart] {tank.Name} → {Fmt(targetLines)} bắt đầu tự động!");

                if (tank.BatchQueue.Count > 0)
                {
                    propagation.OnTransitionBatch(tank, targetLines);
                    autoTimer.Start();
                }
                else
                    AppendLog($"[Warn] {tank.Name} BatchQueue rỗng!");
            }
        }

        private void btnMixDone_Click(object sender, EventArgs e)
        {
            var dlg = new FormSelectMixer(mixers);
            if (dlg.ShowDialog() != DialogResult.OK || dlg.SelectedMixer == null) return;
            var mx = dlg.SelectedMixer;
            var b = new Batch
            {
                BatchCode = $"{mx.Name.Replace(" ", "")}_B{DateTime.Now:HHmmss}",
                ItemCode = "ITEM-01",
                ProductionOrder = $"PO-{DateTime.Now:HHmmss}",
                TargetQty = 1000
            };
            propagation.OnMixDone(mx, b);
            lblPendingBatch.Text = $"⏳ Pending: {b.BatchCode}"; lblPendingBatch.ForeColor = Color.Orange;
            PromptAssign(mx);
        }

        private void btnTransition_Click(object sender, EventArgs e)
        {
            bool any = false;
            foreach (var kv in tankLineMap.ToList())
            {
                if (kv.Key.BatchQueue.Count == 0 || kv.Value.Count == 0)
                { AppendLog($"[Transition] {kv.Key.Name} – bỏ qua"); continue; }
                propagation.OnTransitionBatch(kv.Key, kv.Value);
                any = true;
            }
            if (any) { autoTimer.Start(); RefreshAll(); }
            else
            {
                AppendLog("[Transition] Không có tank nào sẵn sàng.");
                MessageBox.Show("Không có batch nào đã assign Line!\nHãy MixDone trước.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnStop_Click(object sender, EventArgs e) { autoTimer.Stop(); AppendLog("[Stop] Sim dừng."); }
        private void btnShowJourney_Click(object sender, EventArgs e) => tabControl.SelectedTab = tabJourney;

        private void btnReset_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("Reset toàn bộ?\nTất cả batch, journey, queue sẽ bị xóa.",
                "Xác nhận Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes) ResetAll();
        }

        private void AutoRun(object sender, EventArgs e)
        {
            foreach (var line in lines)
            {
                bool fb = line.Filler.Buffer.Count > 0;
                bool db = line.Machines.Any(m => m.Buffer.Count > 0);
                if (fb)
                {
                    simCounters[line.Name] += 50;
                    var tank = GetActiveTank(line);
                    propagation.UpdateCounterOut(line.Filler, simCounters[line.Name], tank);
                }
                if (fb || db) propagation.MoveLine(line, 20);
            }

            var finished = tankLineMap
                .Where(kv => kv.Key.BatchQueue.Count == 0
                    && kv.Value.All(l => l.Filler.Buffer.Count == 0))
                .Select(kv => kv.Key).ToList();
            foreach (var t in finished)
            {
                AppendLog($"[Cleanup] {t.Name} hoàn thành, giải phóng lines.");
                tankLineMap.Remove(t);
            }

            TryStartPending();

            bool anyActive = lines.Any(l => l.Filler.Buffer.Count > 0 || l.Machines.Any(m => m.Buffer.Count > 0));
            if (!anyActive && pendingList.Count == 0) { autoTimer.Stop(); AppendLog("[AutoRun] Tất cả hoàn thành."); }

            RefreshAll();
        }

        private void RefreshAll()
        {
            if (pnlLineCards.InvokeRequired) { pnlLineCards.Invoke((Action)RefreshAll); return; }
            foreach (var c in lineCards) c.Refresh();
            UpdateTankGrid(); UpdateJourneyGrid();
        }

        private void UpdateTankGrid()
        {
            if (dgvTanks.InvokeRequired) { dgvTanks.Invoke((Action)UpdateTankGrid); return; }
            dgvTanks.SuspendLayout(); dgvTanks.Rows.Clear();
            foreach (var tank in tanks)
            {
                string batch = tank.BatchQueue.Count > 0 ? string.Join("|", tank.BatchQueue.Select(b => b.BatchCode)) : "-";
                string active = tankLineMap.ContainsKey(tank) && tankLineMap[tank].Count > 0 ? Fmt(tankLineMap[tank]) : "-";
                var pl = pendingList.Where(p => p.tank == tank).SelectMany(p => p.lines.Select(l => l.Name)).Distinct().ToList();
                string pend = pl.Count > 0 ? $"⏳ {string.Join(",", pl)}" : "";
                var ri = dgvTanks.Rows.Add(tank.Name, tank.SourceMixer.Length > 0 ? tank.SourceMixer : "-", batch, tank.BatchQueue.Count, active, pend);
                if (tank.BatchQueue.Count > 0) dgvTanks.Rows[ri].DefaultCellStyle.BackColor = C_BATCH;
                if (pl.Count > 0) dgvTanks.Rows[ri].Cells["Queued"].Style.BackColor = C_PEND;
            }
            dgvTanks.ResumeLayout();
        }

        private void UpdateJourneyGrid()
        {
            if (dgvJourney.InvokeRequired) { dgvJourney.Invoke((Action)UpdateJourneyGrid); return; }
            dgvJourney.SuspendLayout(); dgvJourney.Rows.Clear();
            foreach (var j in propagation.Journeys.AsEnumerable().Reverse())
            {
                var ri = dgvJourney.Rows.Add(j.BatchCode, j.ItemCode, j.ProductionOrder, j.MixerName, j.TankName ?? "-",
                    string.Join(",", j.FillerLines), j.MixDoneTime.ToString("HH:mm:ss"),
                    j.TransferTime?.ToString("HH:mm:ss") ?? "-", j.CompleteTime?.ToString("HH:mm:ss") ?? "-", j.Status);
                dgvJourney.Rows[ri].DefaultCellStyle.BackColor = j.Status == "Complete" ? C_DONE : j.Status == "Running" ? C_BATCH : C_PEND;
            }
            dgvJourney.ResumeLayout();
        }

        private void AppendLog(string msg)
        {
            if (lstLog == null) return;
            if (lstLog.InvokeRequired) { lstLog.Invoke((Action)(() => AppendLog(msg))); return; }
            lstLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}]  {msg}");
            if (lstLog.Items.Count > 500) lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int y = 0;
            foreach (var line in lines)
            {
                var card = new LineCardControl(line, propagation)
                {
                    Top = y,
                    Left = 0,
                    Width = pnlLineCards.ClientSize.Width - 16,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                pnlLineCards.Controls.Add(card); lineCards.Add(card);
                y += card.Height + 8;
            }
            pnlLineCards.Resize += (s, ev) => { foreach (var c in lineCards) c.Width = pnlLineCards.ClientSize.Width - 16; };

            dgvTanks.Columns.Add("Tank", "Tank"); dgvTanks.Columns.Add("Mixer", "Mixer");
            dgvTanks.Columns.Add("Batch", "Batch Queue"); dgvTanks.Columns.Add("Count", "Qty");
            dgvTanks.Columns.Add("Lines", "Active Lines"); dgvTanks.Columns.Add("Queued", "Waiting ⏳");
            SetCols(dgvTanks, 60, 90, 200, 45, 120, 160);

            dgvJourney.Columns.Add("Batch", "Batch"); dgvJourney.Columns.Add("Item", "Item");
            dgvJourney.Columns.Add("PO", "PO"); dgvJourney.Columns.Add("Mixer", "Mixer");
            dgvJourney.Columns.Add("Tank", "Tank"); dgvJourney.Columns.Add("Lines", "Lines");
            dgvJourney.Columns.Add("MixT", "MixDone"); dgvJourney.Columns.Add("TrnsT", "Transfer");
            dgvJourney.Columns.Add("DoneT", "Complete"); dgvJourney.Columns.Add("Status", "Status");
            SetCols(dgvJourney, 150, 80, 100, 90, 60, 100, 80, 80, 80, 70);
            UpdateTankGrid();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        { plcTimer.Stop(); autoTimer.Stop(); plcService.Disconnect(); }

        private static void SetCols(DataGridView g, params int[] w)
        { for (int i = 0; i < Math.Min(w.Length, g.Columns.Count); i++) g.Columns[i].Width = w[i]; }

        private static string Fmt(List<ProductionLine> ls) => string.Join("+", ls.Select(l => l.Name));
    }
}
