# Dungeon Flux

![Unity](https://img.shields.io/badge/Unity-6000.2.10f1-black?style=flat&logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-blue?style=flat&logo=csharp)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

<img width="1920" height="1080" alt="Screenshot (217)" src="https://github.com/user-attachments/assets/1bda1cba-ec03-4640-8bbd-6df65a6f3c2c" />

## About
A 2D Roguelike implementing a **Dynamic Difficulty Adjustment (DDA)** system. The game utilizes **Procedural Content Generation (PCG)** algorithms to restructure dungeon layouts and **Behavior Trees** to adapt enemy AI logic in real-time. By tracking player metrics (kill count, HP, speed), the system dynamically generates personalized challenges tailored to 4 distinct playstyles (Speedrunner, Aggressive, Passive, Explorer).

## Technical Highlights
* **For Speedrunners**
The PCG engine generates **longer corridors with high trap density** to challenge navigation speed and reflex.

<p align="center">
  <img width="1276" height="626" alt="Screenshot 2025-12-22 105436" src="https://github.com/user-attachments/assets/b427b75c-a176-4973-842b-0d0bb55ba3d3" />
</p>

* **For Aggressive Players**
Enemy spawn rates increase, while AI behavior shifts to **evasive tactics**, forcing tactical positioning over button mashing.

<p align="center">
  <img width="1278" height="642" alt="Screenshot 2025-12-22 120715" src="https://github.com/user-attachments/assets/4d8f49d1-5103-448b-836e-b53e7c2b43f1" />
</p>
  
* **For Passive Players**
Spawns more **aggressive ranged enemies**, pressuring defensive players to move and engage in combat.

<p align="center">
  <img width="1595" height="802" alt="Screenshot 2025-12-22 110207" src="https://github.com/user-attachments/assets/03c6e6de-525f-4abe-8baa-71cac58ee1be" />
</p>

* **For Explorers**
Map complexity maximizes with **branching paths and hidden rooms**, rewarding curiosity over combat speed.

<p align="center">
  <img width="1277" height="638" alt="Screenshot 2025-12-22 120443" src="https://github.com/user-attachments/assets/b360067e-b56f-4f49-82fe-b5c00a6294d9" />
</p>

## How to Run:
**Option 1: Play Build**
1.  Download & extract `DungeonFlux_v1.0.zip`.
2.  Open `DungeonFlux.exe`.

**Option 2: Open Source Code**
1.  Clone this repository.
2.  Open project in **Unity 6000.2.10f1**.
3.  Open Scene: `Scenes/MainMenu`.
