# AGENTS.md -- Madness Dreams

Persistent project instructions. These apply to every agent session in this
repository. Project skills in `.commandcode/skills/` add working method on top;
where a skill disagrees with this file, this file wins.

## The game

A 2D top-down dark-fairy-tale roguelite (Binding of Isaac / Cult of the Lamb
structure): corrupted nursery stories, procedural rooms by seed, phased bosses,
persistent progression. Language: **Spanish** for all player-facing text.

- **Engine:** Unity 6.3 LTS (`6000.3.19f1`), URP 2D.
- **UI framework:** uGUI (`com.unity.ugui 2.0.0`), legacy `UnityEngine.UI.Text`.
  Canvas reference resolution **1920x1080**. Do not introduce UI Toolkit.
- **Input:** new Input System with legacy fallback (`Input Handling = Both`).
- **Build target:** StandaloneLinux64.

## UI: non-negotiable standards

Every interface must follow the game's artistic direction, genre, atmosphere and
narrative identity -- a cold, eerie storybook gone wrong. Blue mist, dead trees,
isolated gold light. Not a cheerful casual game and not a business app.

**Required:**

- Professional typography, spacing, composition, contrast and visual hierarchy.
- Game-specific sprites, textures, iconography and nine-sliced frames.
- Reusable components and consistent design tokens.
- Correct behaviour across resolutions and aspect ratios, with keyboard and mouse
  input handled.
- Complete interaction states: hover, pressed, selected, focused, disabled, and
  transitions between them.
- Existing gameplay logic preserved. UI work must not change gameplay scripts.

**Forbidden:**

- Generic SaaS dashboards or mobile-app aesthetics.
- Excessive rounded rectangles; card grids as a default layout.
- Unnecessary gradients, decorative drop shadows, glassmorphism.
- Default Unity UI styling -- especially the built-in sprite (`fileID: 10905`) as
  visible chrome.
- Inventing new colours outside the palette below without a stated reason.

## Design tokens

Palette, taken from live scene and prefab values:

| Role | Hex | Use |
|---|---|---|
| Midnight | `#0D0F1A` | Panel and night base |
| Deep night | `#0F0F17` | Scrims, vignette |
| Ink | `#17171F` | Deep panel interior |
| Slate panel | `#383842` | Menu panels |
| Slate light | `#40404D` | Raised surface |
| Cold cyan | `#66E5FF` | Cold light, highlights |
| Arcane blue | `#4CA6FF` | Primary accent, basic-ability frames |
| Pale gold | `#FFF2A8` | Fireflies, title glow, progress |
| Sickly gold | `#D9CC66` | Special-ability frames |
| Dried blood | `#8C2B2B` | Damage, danger |
| Danger red | `#E65959` | Health loss, warnings |
| Bone | `#F2F2F2` | Body text |

Type scale, installed in `Assets/Art/UI/Fonts/` and already applied across the UI:

- **IM Fell English** -- screen titles, 40px and up.
- **Cardo** -- body, dialogue, buttons, captions (the default for text).
- **Cardo Bold** -- boss name plate.
- **Archivo Narrow SemiBold** -- numerals, key badges, counters, progress.
- **Chronicles of Arkmar** -- the main-menu title **only**. Do not change it and do
  not use it anywhere else.

## Implementation constraints

- UI art lives in `Assets/Art/UI/Generated/`. Reuse it before generating more.
- Sprite import: **Point** filter, no compression, `textureType: Sprite`. Nine-slice
  borders belong in the `.meta` (`spriteBorder`) with `Image.type = Sliced`.
- `Slot1..4` Image components are runtime icon targets (`AbilityHotbarWidget`
  reassigns `.sprite`); only `Frame_0..3` are frames. Hotbar frame colours are
  tinted by code -- keep frames near-white so the art survives the tint.
- `.ttf`, `.png`, `.zip` are Git LFS tracked. Commit `.meta` files alongside assets
  so teammates do not get broken references.
- Unity YAML is fragile under scripted edits. Validate structurally after writing;
  never patch a prefab or scene with a regex and no assertion.
- The main menu title (`Assets/Scenes/01_MainMenu.unity`, "Madness Dreams") uses
  Chronicles of Arkmar at 180px. Leave it alone.

## Before calling UI work finished

Code compiling is not completion. The mandatory sequence:

1. Inspect the existing UI and the scripts that drive it.
2. Gather at least three references from professionally released games, and say
   concretely what makes each effective.
3. Write a visual specification: focal element, reading order, grid, type scale,
   palette mapping, states, scaling.
4. Implement in uGUI with the project's assets and tokens.
5. Enter play mode and capture the Game View (`capture_game_view`,
   `source: "screen"` -- overlay canvases do not appear in camera renders).
6. Look at the actual screenshot.
7. Compare against the specification and the references; name specific defects.
8. Fix and repeat from step 5.

Score the result against the rubric and report it honestly. Minimum **85/100**:

| Criterion | Weight |
|---|---|
| Artistic identity | 25 |
| Composition and visual hierarchy | 20 |
| Typography and readability | 15 |
| Visual polish and asset quality | 15 |
| Interaction feedback | 10 |
| Responsive scaling | 10 |
| Accessibility | 5 |

If it scores under 85, say so and list what remains broken. Do not claim a visual
result you have not seen in a screenshot, and do not report success you cannot
evidence.

## Available tooling

- **Unity MCP** (`unity`, stdio) -- drive the Editor: read/validate prefabs, enter
  play mode, capture screenshots, evaluate C#, check import settings.
- **Context7 MCP** (`context7`, HTTP, project scope) -- current Unity and package
  documentation. Ask for it with `use context7`.
- **Figma MCP** (`figma`, HTTP, project scope) -- requires a one-time OAuth sign-in
  (`cmd mcp auth figma`). Unavailable until that is done.
- **Skills:** `game-ui-design` (this project's method), `game-ui-ux` (design
  critique), `game-ui` (layout/accessibility standards, with a project override at
  the top), `game-ui-layout` (engine-neutral scaling and gamepad focus),
  `using-unity` (33 Unity skills; read its index, load references on demand).

Model: `deepseek/deepseek-v4.1-flash` for implementation (vision-capable). For
sustained heavy visual critique, an explicit vision model may be used.

Keep token consumption reasonable -- load `using-unity` references only when the
task needs them.
