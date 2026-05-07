using System;
using System.Collections.Generic;
using IoTClient.Clients.PLC;
using IoTClient.Common.Enums;

namespace BatchTransition
{
    public class PlcService : IDisposable
    {
        private SiemensClient _mixerPlc;   
        private SiemensClient _packPlc;    

        public bool   IsConnected { get; private set; }
        public string LastError   { get; private set; } = "";

        public bool Connect(string mixerIp, string packIp, int port = 102)
        {
            try
            {
                _mixerPlc = new SiemensClient(SiemensVersion.S7_1500, mixerIp, port);
                _packPlc  = new SiemensClient(SiemensVersion.S7_1200, packIp,  port);

                var r1 = _mixerPlc.Open();
                var r2 = _packPlc.Open();

                IsConnected = r1.IsSucceed && r2.IsSucceed;
                if (!IsConnected) LastError = $"MixerPLC: {r1.Err} | PackPLC: {r2.Err}";
                return IsConnected;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                IsConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            try { _mixerPlc?.Close(); } catch { }
            try { _packPlc?.Close();  } catch { }
            IsConnected = false;
        }
        public bool ReadMixDone(string address)
        {
            if (_mixerPlc == null) return false;
            var r = _mixerPlc.ReadBoolean(address);
            if (!r.IsSucceed) { LastError = r.Err; return false; }
            return r.Value;
        }
        public string ReadMixerString(string address, ushort len = 20)
        {
            if (_mixerPlc == null) return "";
            var r = _mixerPlc.ReadString(address, len);
            if (!r.IsSucceed || r.Value == null) return "";
            return System.Text.Encoding.ASCII.GetString(r.Value).Trim('\0', ' ');
        }
        public int ReadMixerInt(string address)
        {
            if (_mixerPlc == null) return 0;
            var r = _mixerPlc.ReadInt32(address);
            return r.IsSucceed ? r.Value : 0;
        }
        public bool ReadTransitionBit(string address)
        {
            if (_packPlc == null) return false;
            var r = _packPlc.ReadBoolean(address);
            if (!r.IsSucceed) { LastError = r.Err; return false; }
            return r.Value;
        }
        public bool ReadTransferFillerState(string address)
        {
            if (_packPlc == null) return false;
            var r = _packPlc.ReadBoolean(address);
            if (!r.IsSucceed) { LastError = r.Err; return false; }
            return r.Value;
        }
        public int ReadCounterOut(string address)
        {
            if (_packPlc == null) return 0;
            var r = _packPlc.ReadInt32(address);
            return r.IsSucceed ? r.Value : 0;
        }

        public bool ReadChangeOver(string address)
        {
            if (_packPlc == null) return false;
            var r = _packPlc.ReadBoolean(address);
            if (!r.IsSucceed) { LastError = r.Err; return false; }
            return r.Value;
        }
        public string ReadFillerProductCode(string address, ushort len = 20)
        {
            if (_packPlc == null) return "";
            var r = _packPlc.ReadString(address, len);
            if (!r.IsSucceed || r.Value == null) return "";
            return System.Text.Encoding.ASCII.GetString(r.Value).Trim('\0', ' ');
        }

        public Dictionary<string, bool> ReadAllMixDoneBits(IEnumerable<MixerUnit> mixers)
        {
            var d = new Dictionary<string, bool>();
            foreach (var m in mixers) d[m.Name] = ReadMixDone(m.PlcMixDoneBitAddress);
            return d;
        }

        public Dictionary<string, bool> ReadAllTransitionBits(IEnumerable<StorageTank> tanks)
        {
            var d = new Dictionary<string, bool>();
            foreach (var t in tanks) d[t.Name] = ReadTransitionBit(t.PlcTransitionBitAddress);
            return d;
        }

        public Dictionary<string, bool> ReadAllTransferFillerStates(IEnumerable<StorageTank> tanks)
        {
            var d = new Dictionary<string, bool>();
            foreach (var t in tanks)
            {
                d[t.Name] = !string.IsNullOrEmpty(t.PlcTransferFillerStateAddress)
                    ? ReadTransferFillerState(t.PlcTransferFillerStateAddress)
                    : false;
            }
            return d;
        }

        public Dictionary<string, int> ReadAllCounterOuts(IEnumerable<ProductionLine> lines)
        {
            var d = new Dictionary<string, int>();
            foreach (var l in lines)
                if (!string.IsNullOrEmpty(l.Filler.PlcCounterAddress))
                    d[l.Name] = ReadCounterOut(l.Filler.PlcCounterAddress);
            return d;
        }

        public Dictionary<string, bool> ReadAllChangeOvers(IEnumerable<ProductionLine> lines)
        {
            var d = new Dictionary<string, bool>();
            foreach (var l in lines)
                if (!string.IsNullOrEmpty(l.Filler.PlcChangeOverAddress))
                    d[l.Name] = ReadChangeOver(l.Filler.PlcChangeOverAddress);
            return d;
        }

        public Dictionary<string, string> ReadAllFillerItemCodes(IEnumerable<ProductionLine> lines)
        {
            var d = new Dictionary<string, string>();
            foreach (var l in lines)
                if (!string.IsNullOrEmpty(l.Filler.PlcItemCodeAddress))
                    d[l.Name] = ReadFillerProductCode(l.Filler.PlcItemCodeAddress);
            return d;
        }

        public void Dispose() => Disconnect();
    }
}
