using System;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.LinkLabel;

namespace BatchTransition
{
    public class BatchPropagationService
    {
        public List<BatchJourney> Journeys { get; } = new List<BatchJourney>();
        public event Action<string> OnBatchEvent;
        /*
        Trigger khi Mixer hoàn thành batch (MixDone bit ON)
        Logic : Mixer tạo batch (B01, B02, B03…)
                Batch được đưa vào PendingBatches
                Journey được tạo để tracking
         */
        public void OnMixDone(MixerUnit mixer, Batch batch)
        {
            if (batch == null) return;
            mixer.PendingBatches.Enqueue(batch);

            Journeys.Add(new BatchJourney
            {
                BatchCode = batch.BatchCode,
                ItemCode = batch.ItemCode,
                ProductionOrder = batch.ProductionOrder,
                MixerName = mixer.Name,
                MixDoneTime = DateTime.Now
            });

            Raise($"[MixDone] {mixer.Name} → Batch={batch.BatchCode} | Item={batch.ItemCode}" +
                  $" | PO={batch.ProductionOrder} | Qty={batch.TargetQty}");
        }
        /*
        Gán batch từ Mixer xuống Storage Tank (buffer trung gian)
        Logic : Lấy batch từ PendingQueue
                Copy batch sang tất cả tank được chọn
                Set nguồn mixer
        */
        public void AssignBatchToTanks(MixerUnit mixer, List<StorageTank> tanks)
        {
            if (mixer.PendingBatches.Count == 0)
            { Raise($"[Warn] {mixer.Name}: không có batch đang chờ!"); return; }

            var b = mixer.PendingBatches.Dequeue();
            foreach (var tank in tanks)
            {
                tank.BatchQueue.Enqueue(new Batch
                {
                    BatchCode = b.BatchCode,
                    ItemCode = b.ItemCode,
                    ProductionOrder = b.ProductionOrder,
                    TargetQty = b.TargetQty
                });
                tank.SourceMixer = mixer.Name;

                var j = FindJourney(b.BatchCode);
                if (j != null && string.IsNullOrEmpty(j.TankName)) j.TankName = tank.Name;

                Raise($"[Assign] {mixer.Name}→{tank.Name} Batch={b.BatchCode} Queue={tank.BatchQueue.Count}");
            }
        }
        public void OnTransferFillerState(StorageTank tank, List<ProductionLine> lines)
            => PushToFiller(tank, lines, "TransferFillerState");
        public void OnTransitionBatch(StorageTank tank, List<ProductionLine> lines)
            => PushToFiller(tank, lines, "TransitionBatch");


        /* Đẩy batch từ Tank xuống tất cả Filler lines
        Logic:  Lấy batch hiện tại trong Tank
                Gửi xuống tất cả line
                Reset counter base
                Update Journey(traceability)
        */
        private void PushToFiller(StorageTank tank, List<ProductionLine> lines, string trigger)
        {
            if (tank.BatchQueue.Count == 0)
            { Raise($"[Warn] {tank.Name} {trigger} raised – BatchQueue rỗng!"); return; }

            var b = tank.BatchQueue.Peek(); 

            foreach (var line in lines)
            {
                EnqueueToMachine(line.Filler, b, tank.Name, tank.SourceMixer);

                line.Filler.BatchCounterBase += line.Filler.CounterOut;
                line.Filler.CounterOut = 0;

                var j = FindJourney(b.BatchCode);
                if (j != null)
                {
                    if (string.IsNullOrEmpty(j.TankName)) j.TankName = tank.Name;
                    if (!j.FillerLines.Contains(line.Name)) j.FillerLines.Add(line.Name);
                    if (j.TransferTime == null) j.TransferTime = DateTime.Now;
                }

                Raise($"[{trigger}] {tank.Name}→{line.Name}/Filler Batch={b.BatchCode} Target={b.TargetQty}");
            }
        }

        // Tính thực tế từ PLC counter
        public bool UpdateCounterOut(Machine filler, int rawPlcCounter, StorageTank tank)
        {
            int delta = Math.Max(0, rawPlcCounter - filler.BatchCounterBase);
            filler.CounterOut = delta;

            if (filler.Buffer.Count == 0) return false;
            var lot = filler.Buffer.Peek();
            if (lot.OriginalQty <= 0) return false;

            if (delta >= lot.OriginalQty)
            {
                AdvanceBatch(filler, lot, tank, rawPlcCounter,
                    $"Counter {delta}/{lot.OriginalQty}");
                return true;
            }
            return false;
        }

        // Force chuyển batch khi có ChangeOver bit
        public void OnChangeOver(Machine filler, StorageTank tank, int rawPlcCounter)
        {
            if (filler.Buffer.Count == 0)
            { Raise($"[ChangeOver] {filler.Name}: buffer rỗng – bỏ qua"); return; }

            var lot = filler.Buffer.Peek();
            Raise($"[ChangeOver] {filler.Name} force advance Batch={lot.BatchCode}" +
                  $" (counter={filler.CounterOut})");
            AdvanceBatch(filler, lot, tank, rawPlcCounter, "ChangeOver bit");
        }

        // Khi PLC đổi ItemCode → kiểm tra batch transition
        public bool OnItemCodeChange(Machine filler, string newItemCode,
            StorageTank tank, int rawPlcCounter)
        {
            if (string.IsNullOrEmpty(newItemCode) || newItemCode == filler.LastItemCode)
                return false;

            string old = filler.LastItemCode;
            filler.LastItemCode = newItemCode;

            if (filler.Buffer.Count == 0) return false;
            var lot = filler.Buffer.Peek();

            if (lot.ItemCode != newItemCode)
            {
                Raise($"[ItemChange] {filler.Name}: {old}→{newItemCode} | force advance Batch={lot.BatchCode}");
                AdvanceBatch(filler, lot, tank, rawPlcCounter, $"ItemCode {old}→{newItemCode}");
                return true;
            }
            return false;
        }

        // Mô phỏng flow sản phẩm qua các máy downstream
        public void MoveLine(ProductionLine line, int qty)
        {
            Machine last = line.Machines[line.Machines.Count - 1];

            DrainMachine(last, qty);

            for (int i = line.Machines.Count - 1; i >= 0; i--)
            {
                Machine from = i == 0 ? line.Filler : line.Machines[i - 1];
                Machine to = line.Machines[i];
                TransferLot(from, to, qty);
            }
        }

        // Giảm sản lượng ở machine cuối line
        private void DrainMachine(Machine machine, int qty)
        {
            while (qty > 0 && machine.Buffer.Count > 0)
            {
                var lot = machine.Buffer.Peek();
                int drain = Math.Min(qty, lot.Quantity);
                lot.Quantity -= drain;
                qty -= drain;
                if (lot.Quantity <= 0)
                    machine.Buffer.Dequeue();
            }
        }

        // Lấy thông tin hiển thị SCADA (Batch/Item/PO)
        public (string batch, string item, string po, string mixer, string tank)
            GetDisplayInfo(Machine m)
        {
            var lot = m.CurrentLot;
            if (lot == null) return ("-", "-", "-", "-", "-");
            return (lot.BatchCode, lot.ItemCode, lot.ProductionOrder,
                    lot.SourceMixer, lot.SourceTank);
        }

        // Debug trạng thái buffer từng máy
        public string GetBufferState(Machine m)
        {
            if (m.Buffer.Count == 0) return "(empty)";
            return string.Join(" → ", m.Buffer.Select(x =>
                $"[{x.BatchCode}/{x.ItemCode} {x.SourceMixer}→{x.SourceTank}: {x.Quantity}]"));
        }

        /* Chuyển sang batch tiếp theo khi hoàn thành 
        Logic : Remove batch khỏi Filler
                Reset counter
                Mark Journey Complete
                Remove batch khỏi Tank nếu match
                Log event
         */
        private void AdvanceBatch(Machine filler, WipLot lot,
            StorageTank tank, int rawPlcCounter, string reason)
        {
            filler.Buffer.Dequeue();
            filler.BatchCounterBase = rawPlcCounter;
            filler.CounterOut = 0;

            var j = FindJourney(lot.BatchCode);
            if (j != null && j.CompleteTime == null) j.CompleteTime = DateTime.Now;

            if (tank?.BatchQueue.Count > 0
                && tank.BatchQueue.Peek().BatchCode == lot.BatchCode)
            {
                tank.BatchQueue.Dequeue();
                Raise($"[TankDequeue] {tank.Name} dequeue Batch={lot.BatchCode}");
            }

            Raise($"[BatchComplete] {filler.Name} Batch={lot.BatchCode} ({reason}) " +
                  "→ chuyển sang Batch tiếp theo");
        }

        // Đưa batch vào buffer của machine
        private void EnqueueToMachine(Machine m, Batch b, string tankName, string mixerName)
        {
            if (m.Buffer.Count > 0 && m.Buffer.Last().BatchCode == b.BatchCode)
            {
                m.Buffer.Last().Quantity += b.TargetQty;
                m.Buffer.Last().OriginalQty = b.TargetQty; 
                Raise($"[Guard] {m.Name}: Batch={b.BatchCode} đã có – merge qty");
                return;
            }
            m.Buffer.Enqueue(new WipLot
            {
                BatchCode = b.BatchCode,
                ItemCode = b.ItemCode,
                ProductionOrder = b.ProductionOrder,
                SourceMixer = mixerName ?? "",
                SourceTank = tankName ?? "",
                Quantity = b.TargetQty,
                OriginalQty = b.TargetQty  
            });
        }

        // Di chuyển batch giữa các machine downstream
        private void TransferLot(Machine from, Machine to, int qty)
        {
            while (qty > 0 && from.Buffer.Count > 0)
            {
                var lot = from.Buffer.Peek();
                int move = Math.Min(qty, lot.Quantity);
                lot.Quantity -= move;
                if (lot.Quantity == 0) from.Buffer.Dequeue();

                if (to.Buffer.Count > 0 && to.Buffer.Last().BatchCode == lot.BatchCode)
                    to.Buffer.Last().Quantity += move;
                else
                    to.Buffer.Enqueue(new WipLot
                    {
                        BatchCode = lot.BatchCode,
                        ItemCode = lot.ItemCode,
                        ProductionOrder = lot.ProductionOrder,
                        SourceMixer = lot.SourceMixer,
                        SourceTank = lot.SourceTank,
                        Quantity = move,
                        OriginalQty = lot.OriginalQty 
                    });
                qty -= move;
            }
        }

        // Tìm tracking batch trong lịch sử
        private BatchJourney FindJourney(string code)
            => Journeys.LastOrDefault(j => j.BatchCode == code);

        private void Raise(string msg) => OnBatchEvent?.Invoke(msg);
    }

}
