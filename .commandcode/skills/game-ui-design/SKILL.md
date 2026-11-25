---
name: game-ui-design
description: "Design, implement, and visually verify game UI for this Unity project (Madness Dreams). Use for any HUD, menu, screen, overlay, widget, or UI art task. Enforces the project's dark-fairy-tale art direction, a reference-driven workflow with real screenshots, and a weighted quality rubric. Not for gameplay logic or non-UI work."
argument-hint: "[screen or widget to design or fix]"
metadata:
  project: Madness Dreams
  engine: Unity 6.3 LTS (6000.3.19f1)
  framework: uGUI (com.unity.ugui 2.0.0)
---

# Game UI design -- Madness Dreams

This skill is the authority for UI work in this repository. `AGENTS.md` at the repo
root holds the standards in brief; this file holds the working method. Read
`AGENTS.md` first if you have not.

## Art direction (do not re-litigate)

A cold, eerie, dark-fairy-tale pixel-art roguelite. Nursery stories gone wrong:
Hansel & Gretel, Little Red Riding Hood, Peter Pan. The warmth is gone -- blue mist,
dead trees, isolated points of gold light.

Palette, extracted from live scene and prefab values. Use these, do not invent new
hues without a stated reason:

| Role | Hex |
|---|---|
| Midnight / panels | `#0D0F1A` |
| Deep night / scrims | `#0F0F17` |
| Ink | `#17171F` |
| Slate panel | `#383842` |
| Slate light | `#40404D` |
| Cold cyan | `#66E5FF` |
| Arcane blue (primary accent) | `#4CA6FF` |
| Pale gold | `#FFF2A8` |
| Sickly gold | `#D9CC66` |
| Dried blood | `#8C2B2B` |
| Danger red | `#E65959` |
| Bone (body text) | `#F2F2F2` |

Typography already installed and in use (`Assets/Art/UI/Fonts/`):

- **IM Fell English** -- screen titles (PAUSA, DERROTA, AJUSTES, Comerciante). 40px+.
- **Cardo** -- body, dialogue, buttons, captions. The workhorse.
- **Cardo Bold** -- boss name plate only.
- **Archivo Narrow SemiBold** -- numerals: health, counters, progress, key badges.
- **Chronicles of Arkmar** -- the main-menu game title ONLY. Do not change it, and
  do not reuse it elsewhere.

Forbidden, without exception: generic SaaS dashboards, mobile-app aesthetics,
rounded-rectangle card grids, decorative gradients, drop shadows used as decoration,
default Unity UI sprites (the built-in `10905` rounded rect), and purple-blue
"modern dark mode" that is not the palette above.

## The workflow -- mandatory, in order

Never consider a UI task complete because the code compiles or the YAML parses.

1. **Inspect before proposing.** Read the actual prefab/scene and the scripts that
   drive it: `Assets/Prefabs/UI/HUD.prefab`, the `0X_*.unity` scenes, and the widget
   scripts under `Assets/Scripts/HUD/` and `Assets/Scripts/UI/`. Note which Images
   are tinted or overwritten at runtime -- `AbilityHotbarWidget` rewrites
   `slotIcons`, `HealthBarWidget` drives `fill`, and several widgets set `.color`.
   Art placed on those components is discarded or multiplied. Check first.
2. **Gather at least three references from professionally released games.** Name
   them explicitly (e.g. Hades, Dead Cells, Hollow Knight, Blasphemous, Darkest
   Dungeon, Cult of the Lamb, Slay the Spire, Ori). For each, state what makes it
   work: composition, hierarchy, typography, colour use, interaction feedback.
   Do not proceed with zero references, and do not cite references you cannot
   describe concretely.
3. **Write a visual specification before code.** Short and concrete: focal element,
   reading order, grid and spacing, type scale, palette mapping, states, and how the
   screen scales. Deliver this as text in the session so it can be checked later.
4. **Implement with the existing framework.** uGUI is the stack here -- do not
   introduce UI Toolkit or a new package. Reuse `Assets/Art/UI/Generated/` sprites
   and set nine-slice borders when a frame must stretch. Keep gameplay logic
   untouched; UI work must not alter `Assets/Scripts/{Core,Player,Enemies,Boss,...}`.
5. **Render it in Unity and capture the Game View.** Use the Unity MCP tools:
   enter play mode on the right scene, then `capture_game_view` with
   `source: "screen"` (overlay canvas is invisible to a camera render).
6. **Look at the screenshot.** Open the captured PNG and actually assess it. Do not
   claim a visual result you have not seen.
7. **Compare against the specification and the references.** Name specific
   mismatches -- not "looks good".
8. **Fix defects, then repeat from step 5.** Iterate until the rubric passes.
9. **Verify** scaling at other aspect ratios, interaction states, accessibility,
   and that gameplay still behaves.

## Quality rubric -- score honestly

Weighted, out of 100. **85 is the minimum to call it done.**

| Criterion | Weight |
|---|---|
| Artistic identity | 25 |
| Composition and visual hierarchy | 20 |
| Typography and readability | 15 |
| Visual polish and asset quality | 15 |
| Interaction feedback | 10 |
| Responsive scaling | 10 |
| Accessibility | 5 |

Report the score with per-criterion justification. If it lands under 85, say so and
list unresolved defects. Never inflate a score to close a task, and never claim a
screenshot shows something you did not observe.

## Unity-specific traps in this project

- `Slot1..4` Images are **runtime icon targets**; only `Frame_0..3` are frames.
- Hotbar frame colours are **tinted by code** (`basicFrameColor`,
  `specialFrameColor`). Near-white tints preserve ornate art; saturated tints
  destroy it.
- Sprites must import with **Point filter**, **no compression**, `textureType:
  Sprite`. Bilinear filtering ruins pixel art.
- Nine-slice borders live in each PNG's `.meta` (`spriteBorder`) and require
  `Image.type = Sliced`. A zero border means the art stretches and smears.
- `.ttf`, `.png` and `.zip` are **Git LFS tracked**; commit the `.meta` files or
  teammates get broken references.
- Unity YAML is fragile when scripted: always round-trip and validate, never write
  a prefab by regex without a structural assertion.
- Text is legacy `UnityEngine.UI.Text`, not TextMeshPro. It rasterizes without SDF,
  so high-contrast faces go soft at large sizes. Do not swap the title font.

## Delegation

Implementation on `deepseek/deepseek-v4.1-flash` (vision-capable). For heavy visual
critique of a complex screenshot, prefer an explicit vision model. Keep token use
reasonable: load `using-unity` references on demand, not wholesale.
