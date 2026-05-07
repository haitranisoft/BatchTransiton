# 🏭 Batch Transition System

## 📌 Introduction
Batch Transition System is a WinForms application designed to simulate and manage batch production flow in an industrial environment.

The system handles the transition of batches from Mixer → Storage Tank → Production Lines (Filler → downstream machines), with integration to PLC signals and real-time tracking.

---

## 🎯 Main Features

- Detect MixDone signal from Mixer
- Create and manage Batch
- Assign Batch to Storage Tanks
- Assign Tanks to Production Lines
- Automatic Batch Transition to Filler
- Real-time production tracking (CounterOut)
- Handle ChangeOver, ItemCode change, Batch completion
- Visual monitoring: Line status, Machine flow, Batch progress
- Track full Batch Journey

---

## 🧱 System Architecture

Mixer → Storage Tank → Filler → Machines → Complete

---

## ⚙️ Core Logic

BatchPropagationService handles:
- MixDone → create batch
- Assign batch → Tank
- Tank → Filler transition
- Counter tracking
- Batch completion

---

## 🔌 PLC Integration

PlcService connects to:
- Siemens S7-1500
- Siemens S7-1200

Supports reading:
- MixDone
- Transition
- CounterOut
- ChangeOver
- ItemCode

If PLC is not connected → Simulation Mode

---

## 🖥 UI

- Main screen shows all Production Lines
- Displays:
  - Batch info
  - Progress
  - Machine flow
- Includes logs and tables

---

## 🔄 Simulation

- Auto simulation when PLC offline
- Simulates production flow

---

## 📊 Batch Journey

Tracks:
- Batch
- Mixer
- Tank
- Lines
- Status (Pending / Running / Complete)

---

## 🧑‍💻 My Contributions

- Built batch flow logic
- Developed PLC communication
- Designed UI
- Created simulation system

---

## 🚀 How to Run

1. Open in Visual Studio
2. Build
3. Run

Optional PLC:
Update IP in code

---

## 📂 Structure

BatchTransition/
- Models
- Services
- UI
- Main Form
