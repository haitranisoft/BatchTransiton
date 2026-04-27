using System;
using System.Collections.Generic;
using System.Linq;

namespace BatchTransition
{
    public class Batch
    {
        public string BatchCode { get; set; }
        public string ItemCode { get; set; }
        public string ProductionOrder { get; set; }
        public int TargetQty { get; set; } = 1000;
    }

    public class WipLot
    {
        public string BatchCode { get; set; }
        public string ItemCode { get; set; }
        public string ProductionOrder { get; set; }
        public string SourceMixer { get; set; }
        public string SourceTank { get; set; }
        public int Quantity { get; set; }
        public int OriginalQty { get; set; }
    }

    public class BatchJourney
    {
        public string BatchCode { get; set; }
        public string ItemCode { get; set; }
        public string ProductionOrder { get; set; }
        public string MixerName { get; set; }
        public string TankName { get; set; }
        public List<string> FillerLines { get; set; } = new List<string>();
        public DateTime MixDoneTime { get; set; }
        public DateTime? TransferTime { get; set; }
        public DateTime? CompleteTime { get; set; }
        public int CompletedLineCount { get; set; } = 0;
        public string Status
        {
            get
            {
                if (CompleteTime.HasValue) return "Complete";
                if (TransferTime.HasValue) return "Running";
                return "Pending";
            }
        }
    }

    public class Machine
    {
        public string Name { get; set; }
        public string PlcCounterAddress { get; set; }
        public string PlcChangeOverAddress { get; set; }
        public string PlcItemCodeAddress { get; set; }
        public string PlcMachineStatusAddress { get; set; }
        public Queue<WipLot> Buffer { get; set; } = new Queue<WipLot>();
        public WipLot CurrentLot => Buffer.Count > 0 ? Buffer.Peek() : null;
        public int BatchCounterBase { get; set; } = 0;
        public int CounterOut { get; set; } = 0;
        public string LastItemCode { get; set; } = "";
    }

    public class ProductionLine
    {
        public string Name { get; set; }
        public Machine Filler { get; set; }
        public List<Machine> Machines { get; set; } = new List<Machine>();
        public override string ToString() => Name;
    }

    public class StorageTank
    {
        public string Name { get; set; }
        public string PlcTransitionBitAddress { get; set; }
        public string PlcTransferFillerStateAddress { get; set; }
        public Queue<Batch> BatchQueue { get; set; } = new Queue<Batch>();
        public Batch CurrentBatch => BatchQueue.Count > 0 ? BatchQueue.Peek() : null;
        public string SourceMixer { get; set; } = "";
        public override string ToString() => Name;
    }

    public class MixerUnit
    {
        public string Name { get; set; }
        public string PlcMixDoneBitAddress { get; set; }
        public string PlcBatchCodeAddress { get; set; }
        public string PlcItemCodeAddress { get; set; }
        public string PlcPoAddress { get; set; }
        public string PlcTargetQtyAddress { get; set; }
        public Queue<Batch> PendingBatches { get; set; } = new Queue<Batch>();
        public override string ToString() => Name;
    }
}
