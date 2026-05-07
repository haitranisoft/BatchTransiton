using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchTransition
{
    public class BatchPropagationService
    {
        public List<BatchJourney> Journeys { get; } = new List<BatchJourney>();
        public event Action<string> OnBatchEvent;
        private DateTime _lastMixDoneTime;

        public void OnMixDone(MixerUnit mixer, Batch batch)
        {
            if (batch == null) return;
            mixer.PendingBatches.Enqueue(batch);
            _lastMixDoneTime = DateTime.Now;
            Raise($"[MixDone] {mixer.Name} → Batch={batch.BatchCode}");
        }

        public void AssignBatchToTanks(MixerUnit mixer, List<StorageTank> tanks)
        {
            if (mixer.PendingBatches.Count == 0) { Raise("[Warn] No pending batch!"); return; }
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

                Journeys.Add(new BatchJourney
                {
                    BatchCode = b.BatchCode,
                    ItemCode = b.ItemCode,
                    ProductionOrder = b.ProductionOrder,
                    MixerName = mixer.Name,
                    TankName = tank.Name,
                    MixDoneTime = _lastMixDoneTime
                });
                Raise($"[Assign] {mixer.Name}→{tank.Name} Batch={b.BatchCode}");
            }
        }

        public void OnTransferFillerState(StorageTank tank, List<ProductionLine> lines)
            => PushToFiller(tank, lines, "TransferFillerState");

        public void OnTransitionBatch(StorageTank tank, List<ProductionLine> lines)
            => PushToFiller(tank, lines, "Transition");

        private void PushToFiller(StorageTank tank, List<ProductionLine> lines, string trigger)
        {
            if (tank.BatchQueue.Count == 0)
            { Raise($"[Warn] {tank.Name} {trigger} – BatchQueue rỗng!"); return; }

            var b = tank.BatchQueue.Peek();
            var j = FindJourney(b.BatchCode, tank.Name);

            foreach (var line in lines)
            {
                EnqueueToMachine(line.Filler, b, tank.Name, tank.SourceMixer);
                line.Filler.BatchCounterBase += line.Filler.CounterOut;
                line.Filler.CounterOut = 0;

                if (j != null)
                {
                    if (!j.FillerLines.Contains(line.Name)) j.FillerLines.Add(line.Name);
                    if (j.TransferTime == null) j.TransferTime = DateTime.Now;
                }
                Raise($"[{trigger}] {tank.Name}→{line.Name}/Filler Batch={b.BatchCode}");
            }
        }

        public bool UpdateCounterOut(Machine filler, int rawPlcCounter, StorageTank tank)
        {
            int delta = Math.Max(0, rawPlcCounter - filler.BatchCounterBase);
            filler.CounterOut = delta;
            if (filler.Buffer.Count == 0) return false;
            var lot = filler.Buffer.Peek();
            if (lot.OriginalQty <= 0) return false;
            if (delta >= lot.OriginalQty)
            {
                AdvanceBatch(filler, lot, tank, rawPlcCounter, $"Counter {delta}/{lot.OriginalQty}");
                return true;
            }
            return false;
        }

        public void OnChangeOver(Machine filler, StorageTank tank, int rawPlcCounter)
        {
            if (filler.Buffer.Count == 0) return;
            var lot = filler.Buffer.Peek();
            AdvanceBatch(filler, lot, tank, rawPlcCounter, "ChangeOver");
        }

        public bool OnItemCodeChange(Machine filler, string newCode, StorageTank tank, int raw)
        {
            if (string.IsNullOrEmpty(newCode) || newCode == filler.LastItemCode) return false;
            filler.LastItemCode = newCode;
            if (filler.Buffer.Count == 0) return false;
            var lot = filler.Buffer.Peek();
            if (lot.ItemCode != newCode)
            {
                AdvanceBatch(filler, lot, tank, raw, "ItemCode change");
                return true;
            }
            return false;
        }

        private void AdvanceBatch(Machine filler, WipLot lot,
            StorageTank tank, int rawPlcCounter, string reason)
        {
            filler.Buffer.Dequeue();
            filler.BatchCounterBase = rawPlcCounter;
            filler.CounterOut = 0;

            var j = FindJourney(lot.BatchCode, lot.SourceTank);
            if (j != null)
            {
                j.CompletedLineCount++;
                Raise($"[Advance] {filler.Name} Batch={lot.BatchCode} ({reason})" +
                      $" [{j.CompletedLineCount}/{j.FillerLines.Count} lines done]");

                if (j.CompletedLineCount >= j.FillerLines.Count && j.CompleteTime == null)
                {
                    j.CompleteTime = DateTime.Now;
                    Raise($"[Complete] {lot.SourceTank}→ Batch={lot.BatchCode} hoàn thành tất cả lines");

                    if (tank?.BatchQueue.Count > 0
                        && tank.BatchQueue.Peek().BatchCode == lot.BatchCode)
                    {
                        tank.BatchQueue.Dequeue();
                        Raise($"[TankDequeue] {tank.Name} dequeue Batch={lot.BatchCode}");
                    }
                }
            }
            else
            {
                Raise($"[Advance] {filler.Name} Batch={lot.BatchCode} ({reason})");
            }
        }

        public void MoveLine(ProductionLine line, int qty)
        {
            Machine last = line.Machines[line.Machines.Count - 1];
            DrainMachine(last, qty);
            for (int i = line.Machines.Count - 1; i >= 0; i--)
            {
                Machine from = i == 0 ? line.Filler : line.Machines[i - 1];
                TransferLot(from, line.Machines[i], qty);
            }
        }

        private void DrainMachine(Machine m, int qty)
        {
            while (qty > 0 && m.Buffer.Count > 0)
            {
                var lot = m.Buffer.Peek();
                int d = Math.Min(qty, lot.Quantity);
                lot.Quantity -= d; qty -= d;
                if (lot.Quantity <= 0) m.Buffer.Dequeue();
            }
        }

        public (string batch, string item, string po, string mixer, string tank)
            GetDisplayInfo(Machine m)
        {
            var lot = m.CurrentLot;
            if (lot == null) return ("-", "-", "-", "-", "-");
            return (lot.BatchCode, lot.ItemCode, lot.ProductionOrder,
                    lot.SourceMixer, lot.SourceTank);
        }

        private void EnqueueToMachine(Machine m, Batch b, string tankName, string mixerName)
        {
            if (m.Buffer.Count > 0 && m.Buffer.Last().BatchCode == b.BatchCode
                && m.Buffer.Last().SourceTank == tankName)
            {
                m.Buffer.Last().Quantity += b.TargetQty;
                m.Buffer.Last().OriginalQty = b.TargetQty;
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

        private void TransferLot(Machine from, Machine to, int qty)
        {
            while (qty > 0 && from.Buffer.Count > 0)
            {
                var lot = from.Buffer.Peek();
                int mv = Math.Min(qty, lot.Quantity);
                lot.Quantity -= mv;
                if (lot.Quantity == 0) from.Buffer.Dequeue();
                if (to.Buffer.Count > 0 && to.Buffer.Last().BatchCode == lot.BatchCode)
                    to.Buffer.Last().Quantity += mv;
                else
                    to.Buffer.Enqueue(new WipLot
                    {
                        BatchCode = lot.BatchCode,
                        ItemCode = lot.ItemCode,
                        ProductionOrder = lot.ProductionOrder,
                        SourceMixer = lot.SourceMixer,
                        SourceTank = lot.SourceTank,
                        Quantity = mv,
                        OriginalQty = lot.OriginalQty
                    });
                qty -= mv;
            }
        }

        private BatchJourney FindJourney(string batchCode, string tankName)
            => Journeys.LastOrDefault(j => j.BatchCode == batchCode && j.TankName == tankName);

        private void Raise(string msg) => OnBatchEvent?.Invoke(msg);
    }
}
