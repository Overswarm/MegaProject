# MegaBot — a Mega Man–style game for Unity 6

A classic NES Mega Man–style action platformer built for **Unity 6 LTS (6000.x)**.
All placeholder art, sound effects, scenes and level content are **generated from
code** — the project needs zero binary assets and everything important can be
auto-populated from the **Tools ▸ Mega Man** menu.

## Quick start

1. Open the project in **Unity 6 LTS**. Let it import packages (URP, Input System, uGUI, 2D Sprite).
2. On first open you'll be prompted to generate the game scenes — click **Generate**.
   (Or run **Tools ▸ Mega Man ▸ Setup Project (Generate Scenes)** at any time.)
   Setup also creates and activates the **URP 2D (Universal 2D)** render pipeline
   (`Assets/Settings/`); sprites use unlit materials, so add a Global Light 2D and
   Sprite-Lit materials only if you want 2D lighting later.
3. Open `Assets/Scenes/Title.unity` and press **Play**.

> If you see Input System errors on first launch, run
> **Tools ▸ Mega Man ▸ Fix Input Handler** and restart the editor
> (this sets *Active Input Handling* to "Both").

## Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Move / climb / menus | WASD or Arrows | D-pad / left stick |
| Jump | Space or Z | South button (A) |
| Slide | Down + Jump | Down + A |
| Fire (hold to charge) | X or K | West button (X) |
| Weapon prev / next | Q / E (or [ / ]) | LB / RB |
| Pause (weapon menu) | Enter / Esc / P | Start |

## What's implemented (NES feature checklist)

- **Movement**: instant-accel run, variable-height jump, **slide** (down+jump, with
  low-ceiling handling — there's a mandatory slide corridor in the demo stage), **ladders**
  (climb, jump-off, descend from ladder tops), knockback + invincibility frames.
- **Mega Buster**: pellets (max 3 on screen), MM4-style **charge shot** (mid + full tiers),
  shots deflect off shielded enemies with the classic *ping*.
- **Special weapons** (energy-based, 28-tick bars): **Slice Boomerang** (returning cutter)
  and **Blast Bomb** (lobbed AoE), unlocked by beating bosses; swap via bumpers/Q,E or the pause menu.
- **Health & lives**: 28-tick health bar, lives with REST counter, game over → Continue / Stage Select.
- **Checkpoints**: pass them silently; death respawns you at the last one (scene reload = enemies reset).
- **E-Tanks / W-Tanks**: pick up in stages (max 9), stored on your save, used from the
  pause menu to refill health / weapon energy.
- **Pause menu**: weapon select with energy bars, tank usage, exit stage.
- **Stage select**: classic 3×3 grid (2 playable slots in this build, 6 wired-but-locked),
  cleared stages are marked and replayable.
- **Boss flow**: double shutter doors with auto-walk, camera pan into the boss room,
  boss drop-in + health fill, jump/spread-shot AI, weakness multipliers,
  death-orb explosion, **YOU GOT [WEAPON]** screen.
- **Enemies**: Met-style shielded pop-up shooter, ground walker, swooping flyer — all placed
  via spawners that **respawn off-screen** like the NES games, with classic drop tables
  (health/energy pellets, rare 1-Up).
- **Hazards**: instant-kill spikes and bottomless pits.
- **Presentation**: teleport beam-in + READY text, procedural chiptune-style SFX,
  weapon-based palette tint on the player.

## The Tools menu

Everything can be placed without manual component assembly
(objects appear at the Scene view pivot, snapped to the grid):

- **Setup Project (Generate Scenes)** — builds Title / StageSelect / DemoStage and adds them to Build Settings.
- **Open Demo Stage**, **Fix Input Handler**
- **Create ▸ Stage Essentials** (StageController + camera), **Player**
- **Create ▸ Level ▸** Platform Block, Ground Slab, Ladder, Spikes, Kill Zone, Checkpoint, Boss Door, Boss Door (Final)
- **Create ▸ Enemies ▸** Met / Walker / Flyer spawners, Boss
- **Create ▸ Items ▸** E-Tank, W-Tank, health/energy pickups, 1-Up

To build your own stage: new scene → *Create ▸ Stage Essentials* → lay down blocks,
ladders, spawners, checkpoints (index 0 = spawn) → add two boss doors (mark the second
*Final* and set its Room Bounds + Boss reference) → add the scene to Build Settings and
point a slot at it in `Assets/Scripts/Core/GameConfig.cs` (`Stages.All`).

## Project layout

```
Assets/Scripts/Core/      config & stage table, input, layers, sprites, sfx, GameManager, EntityFactory
Assets/Scripts/Player/    controller, health, visuals, weapons, shooter
Assets/Scripts/Combat/    projectiles (buster/cutter/bomb), effects
Assets/Scripts/Enemies/   Met, Walker, Flyer, spawner, boss
Assets/Scripts/Level/     stage controller, camera, doors, ladders, hazards, pickups, checkpoints
Assets/Scripts/UI/        HUD, pause menu, title, stage select, UI builder
Assets/Editor/            Tools menu, scene builders, demo stage layout, first-run prompt
```

### Tuning & extending

- Physics/game feel numbers (derived from NES frame data) live in `GameConfig`.
- Stage slots, boss names, rewards and weaknesses live in `Stages.All`.
- Placeholder sprites: `SpriteFactory` (swap in real art by assigning sprites and removing
  the `AutoSprite` component); sounds: `Sfx`.

### Known placeholders / next steps

- No music yet (SFX only) • art is generated pixel-map placeholders • 6 of 8 stage slots
  are locked • no Wily/fortress chain, no password system (saving uses PlayerPrefs instead).
