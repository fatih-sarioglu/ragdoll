# Ragdoll

A physics-based **active ragdoll** game made with Unity. It started out as a ragdoll *fighting* game, then drifted into something more like ragdoll *soccer*. It's not finished, but it might get another look someday.

More than anything, this was a learning project about active ragdolls: exhausting, fun, and full of lessons. Also, a friendly warning, **this is not a great pick for a beginner project** :)

## What's in it

- **Active ragdoll character** driven by `ConfigurableJoint`s, the character stays upright and moves through physics forces, not animations.
- **Procedural locomotion**, all code-driven (no animation clips):
  - `ProceduralWalk`: phase-based procedural stepping with tunable gait data (`RagdollStepData`)
  - `ProceduralJump`: jumping with coyote time and jump buffering, plus a mid-air leg tuck
  - `ProceduralTurn`: turning the body toward the movement direction
  - `ProceduralArms`: procedural arm motion
- **Grab system**: grab nearby objects with either hand via breakable `FixedJoint`s.
- **Camera-relative movement** with a third-person camera controller.
- **Input** via Unity's new Input System (`PlayerControls.inputactions`).

## Project structure

```
Assets/
├── Scripts/
│   ├── RagdollController.cs        # Central locomotion state machine (Idle / Walking / Airborne)
│   ├── ProceduralWalk.cs           # Procedural leg stepping
│   ├── ProceduralJump.cs           # Jump + grounding logic
│   ├── ProceduralTurn.cs           # Yaw control
│   ├── ProceduralArms.cs           # Arm motion
│   ├── GrabSystem.cs               # Physics-based grabbing
│   ├── PoseDriver.cs               # Drives ragdoll joints toward target poses
│   ├── PlayerInputReader.cs        # Input System wrapper
│   ├── CameraController.cs         # Third-person camera
│   ├── RagdollStepData.cs          # ScriptableObject gait data
│   └── ConfigurableJointExtensions.cs
├── Scenes/                         # Arena scene
├── PhysicsMaterials/
└── Models/
```

## Getting started

1. Open the project with **Unity 6 (6000.0.77f1)** or newer.
2. Open the scene in `Assets/Scenes/` (Arena).
3. Hit Play. Move with WASD, jump, and grab things.

## Not implemented (yet?)

The gameplay layer never got built. If the project gets picked back up, the roadmap would be roughly:

- ⚽ **Goal system**: goals, ball detection, celebrations
- 🏃 **Running / sprinting**: a faster gait on top of the walk cycle
- 🤖 **Opponent AI**: an AI-controlled ragdoll to play against
- 🔢 **Score system**: keeping score, match flow, win conditions
- Kicking the ball properly (procedural kick), sound effects, UI/menus

## Lessons learned

Active ragdolls are a rabbit hole. Balancing joint drives, keeping a physics character upright, and writing procedural gaits from scratch teach you a *lot* about Unity's physics, but expect plenty of trial, error, and flailing limbs along the way.
