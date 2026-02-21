Unity Inventory System

A modular and cleanly structured inventory system built in Unity.
This project focuses on system design rather than gameplay.

Features

Slot-based inventory

Stackable and non-stackable items

Weight limit system

JSON save and load

ScriptableObject-based item definitions

Unity-independent domain layer

Architecture
Domain → Pure C# inventory logic
Data → ItemData (ScriptableObjects)
Infrastructure → JSON save/load
Presentation → UI and Unity integration

All inventory rules (stacking, weight, slot management) are separated from MonoBehaviour classes.

Technical Highlights

ID-based item resolution

Partial add support (returns remaining amount)

IReadOnlyList slot exposure

Clean separation of concerns

How to Run

Clone the repository

Open with Unity (LTS recommended)

Open the Demo scene

Press Play