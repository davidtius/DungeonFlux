# Dungeon Flux

![Unity](https://img.shields.io/badge/Unity-6000.2.10f1-black?style=flat&logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-blue?style=flat&logo=csharp)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

<img width="1506" height="851" alt="Screenshot 2026-01-16 165419" src="https://github.com/user-attachments/assets/5266a6f6-2322-4035-8080-9183806eed5b" />

## About
A 2D Roguelike implementing a **Dynamic Difficulty Adjustment (DDA)** system. The game utilizes **Procedural Content Generation (PCG)** algorithms to restructure dungeon layouts and **Behavior Trees** to adapt enemy AI logic in real-time. By tracking player metrics (kill count, HP, speed), the system dynamically generates personalized challenges tailored to 4 distinct playstyles (Speedrunner, Aggressive, Passive, Explorer).

## Technical Highlights
* **For Speedrunners**
The PCG engine generates **longer corridors with high trap density** to challenge navigation speed and reflex.

* **For Aggressive Players**
Enemy spawn rates increase, while AI behavior shifts to **evasive tactics**, forcing tactical positioning over button mashing.
  
* **For Passive Players**
Spawns more **aggressive ranged enemies**, pressuring defensive players to move and engage in combat.

* **For Explorers**
Map complexity maximizes with **branching paths and hidden rooms**, rewarding curiosity over combat speed.

## How to Run:
**Option 1: Play Build**
1.  Download & extract `DungeonFlux_v1.0.zip`.
2.  Open `DungeonFlux.exe`.

**Option 2: Open Source Code**
1.  Clone this repository.
2.  Open project in **Unity 6000.2.10f1**.
3.  Open Scene: `Scenes/MainMenu`.
