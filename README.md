<div align="center">
<h2> FPS Gameplay Prototype </h2>

</div>

## 💡 Overview

A gameplay-focused FPS prototype built in Unity, has fast-paced movement and combat systems combined with RPG-style progression.

The idea behind combat loop was to force the player to get closer to the enemy. The player can get health and ammo from the enemies only after they enter a "stagger" state and the player uses melee to finish them off. This discourages the player from playing safely, instead, pushing them forward.

It includes a custom character controller, dynamic gravity, combat mechanics, and player progression.

Built using C# with an emphasis on modular and extensible architecture.

<div align="center">

## Video Showcase 👇

[![Video](https://img.youtube.com/vi/M6CxlSu4iV4/0.jpg)](https://www.youtube.com/watch?v=M6CxlSu4iV4)

</div>

## 🚀 Features
### Movement System
- Custom character controller
- Movement abilities: dash, double jumps, grappling hook
- Dynamic gravity system allowing non-linear traversal

### Combat System
- Hybrid combat: ranged (hitscan & projectile) and melee
- Modular, data-driven gun system using ScriptableObjects
- Health and damage system

### RPG & Progression
- Player stat upgrades affecting gameplay
- Weapon upgrade system, alternative fire modes

### AI System
- State-based enemy behavior
- Encounter system for enemy spawning

### Other Systems
- Sound Manager
- Level interaction mechanics (doors, jump pads)
