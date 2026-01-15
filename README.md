# KindFolx – Code Samples

This repository contains a curated selection of C# scripts extracted from my Unity game project KindFolx. The full project includes several hundred files; the files included here are representative examples
intended to demonstrate system design, gameplay logic, and state management.

---

## EnemyInspectKF.cs
Handles enemy inspection during combat, including:
- Zooming and focus on hover
- Switching active targets on left-click
- Displaying detailed enemy information on right-click

This script demonstrates UI interaction, input handling, and combat-state awareness.

---

## GoldPurse.cs
Tracks the player’s gold and provides methods to safely add and remove currency.

This script demonstrates encapsulated state management and validation logic.

---

## MiscShop.cs
Defines specialty shop items and manages inspection, purchasing, and persistent stock
levels between sessions.

This script demonstrates shop logic, persistence, and transactional validation.

---

## RoomSpawner.cs
Core logic for procedural room generation using a session seed. Rooms begin with four
openings and gradually reduce to a single opening to close out the level. The maximum
number of rooms can be configured.

This script demonstrates deterministic generation, queue-based spawning, and
configurable progression control.
