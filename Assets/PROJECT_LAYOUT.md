# Madness Dreams — Project Layout

> Generated from the live Unity Editor via MCP tools.
> Engine **Unity 6000.3.19f1** (Unity 6 LTS) · Render pipeline **URP 2D** · Input System (with legacy fallback) · Cinemachine 3.1.7.
> All art is currently **greybox** (colored primitives); final art drops in without touching code.

## 1. Repository root

```text
Videojuegos_final/
├── Assets/                     # Unity authoring (authoring root)
├── Packages/                   # UPM manifest + lock
├── ProjectSettings/            # Project configuration
├── Library/  Temp/  Logs/      # Generated (Unity)
├── UserSettings/               # Per-user editor state
├── .vscode/                    # Editor integration
├── Assembly-CSharp.csproj      # Runtime assembly
├── Assembly-CSharp-Editor.csproj
├── Videojuegos_final.slnx      # Solution
└── README.md                   # Project overview (ES)
```

## 2. Assets tree

```text
Assets/
├── Animations/                 # AnimatorControllers (color-coded state machine)
│   ├── Player/PlayerAnimator.controller
│   └── Enemy/EnemyAnimator.controller
├── Art/
│   ├── Greybox/                # Placeholder sprites (Square, Circle)
│   ├── Tilemaps/               # New Tile Palette
│   └── Tilesets/
├── Prefabs/
│   ├── Enemies/                # 8 archetypes + Boss_Caperucita
│   ├── Greybox/                # Player + 3 projectiles
│   ├── Pickups/                # 4 ability pickups
│   ├── Props/                  # Barriers + props (+ World_test set)
│   └── UI/                     # HUD.prefab
├── Scenes/                     # 01_..05_ + Greybox (+ SampleScene, URP template)
├── ScriptableObjects/          # Data-driven config (see §5)
├── Scripts/                    # Code (see §4)
├── Settings/                   # URP assets (Renderer2D, UniversalRP, scene template)
├── Tiles/                      # Tile_Wall + Generated/ + World_test/
├── InputSystem_Actions.inputactions
├── DefaultVolumeProfile.asset
└── UniversalRenderPipelineGlobalSettings.asset
```

## 3. Scenes (Build Settings order)

| # | Scene | Role |
|---|-------|------|
| 0 | `01_MainMenu` | Main menu (first in build) |
| 1 | `03_Gameplay` | Playable level (procedural map + HUD) |
| 2 | `02_Loading` | Loading screen |
| 3 | `04_Defeat` | Defeat (Retry / Menu / Quit) |
| 4 | `05_Settings` | Settings |
| 5 | `Greybox` | Test scene (manual map + boss) |

**Startup flow:** `01_MainMenu` → `GameFlowManager.StartRun()` → `02_Loading` → `03_Gameplay` → `RunManager.GenerateWorld(i)` → `MapGenerator.Generate()` → content → boss death spawns a **Portal** → `RunManager.RequestAdvance()` → next world. Player death → `PlayerDeathHandler` → `GameFlowManager.NotifyPlayerDied()` → `04_Defeat`.

## 4. Scripts (`Assets/Scripts/`, namespace `MadnessDreams`)

Layered: **data (SOs) describe → generation builds → entities play → UI presents**.

| Folder | Responsibility | Scripts (68 total) |
|--------|----------------|--------------------|
| `Core/` | Health, projectile, run manager, enums | `Health`, `Projectile`, `RunManager`, `AnimationState`, `AttackType` |
| `Data/` | ScriptableObject config & templates | `WorldConfigSO`, `ZoneThemeSO`, `MapChunkSO`, `PropDefinitionSO`, `RoomTag`, `RoomSizeClass`, `BlockBankSO`, `TilesetCatalogSO`, `EnemyArchetypeSO`, `EnemyStatsSO`, `BossStatsSO`, `BossAbilitySO`, `DifficultyProfileSO` |
| `Generation/` | Procedural rooms + population | `MapGenerator`, `RoomLayout`, `RoomPlacement`, `RoomConnection`, `RoomContentSpawner`, `Spawner`, `TilemapStamper`, `CellRole` |
| `Enemies/` | Minion AI, movement, attack, sight | `EnemyAI`, `EnemyMovement`, `EnemyAttack`, `EnemyFOV`, `SlowEffect` |
| `Boss/` | Bosses, arena, world progression | `BossAI`, `IBossController`, `CaperucitaBoss`, `PeterPanBoss`, `BossArenaController`, `BossManager`, `BossSpawnPoint` |
| `Player/` | Movement, combat, abilities, stats | `PlayerController`, `PlayerCombat`, `PlayerAbilities`, `PlayerStats`, `PlayerAnimationController`, `PlayerDeathHandler` |
| `Pickups/` | Ability pickups | `AbilityPickup` |
| `Gameplay/` | Level layout, advance portal | `WorldLayout`, `Portal`, `WorldPortalController` |
| `HUD/` | Gameplay UI | `HUDController`, `HealthBarWidget`, `BossHealthBarWidget`, `AbilityHotbarWidget`, `AttackCooldownWidget`, `MinimapWidget`, `MapOverlayWidget`, `MapGrid`, `TaleFragmentWidget`, `PauseController`, `DevMenuController` |
| `UI/` | Screen flow & settings | `GameFlowManager`, `MainMenuController`, `LoadingScreen`, `DefeatController`, `SettingsController`, `SettingsData`, `ScreenFader` |
| `Editor/` | Editor tooling (Editor-only assembly) | `MapChunkCaptureEditor`, `TilesetTools` |
| `Greybox/` | (reserved folder) | — |

**Conventions:** no art in code (Animator `State` int: 0 Idle, 1 Move, 2 MeleeAttack, 3 RangedAttack, 4 Death); `System.Action` events (`MapGenerator.Generated`, `Health.HealthChanged/Died`, `BossArenaController.BossSpawned/BossDefeated`, `BossManager.BossDefeated`); `[SerializeField] private` + read-only public properties.

## 5. ScriptableObjects (`Assets/ScriptableObjects/`)

```text
Abilities/       Ability_WolfClaw, Ability_ThornShot, Ability_PeterDagger, Ability_FairyDust
Difficulty/      DifficultyProfile_Default
Enemies/         EnemyArchetype_* ×8 (Wolf, Woodcutter, Crocodile, Mermaid, Firefly, DustFairy, Bramble, CabinBoy)
                 EnemyStats_* ×8 + BossStats_Caperucita
Props/           Prop_1x1/1x2/2x2/1x4 + Barrier_2x2_* (+ World_test set)
Rooms/           Room_1x1, Room_1x2, Room_2x1, Room_2x2, Room_3x3, Room_Start, Room_Boss (+ World_test set)
Themes/          ZoneTheme_Forest (+ world variants)
Tileset/         TilesetCatalog
Sample/          World_Sample_A, Bank_Sample
Greybox/         (test data)
```

## 6. Prefabs

- **Enemies:** `Enemy_` ×8 (Wolf, Woodcutter, Crocodile, Mermaid, Firefly, DustFairy, Bramble, CabinBoy) + `Boss_Caperucita`.
- **Greybox:** `Player`, `Projectile_Player`, `Projectile_Enemy`, `Projectile_Thorn`.
- **Pickups:** `Pickup_WolfClaw`, `Pickup_ThornShot`, `Pickup_PeterDagger`, `Pickup_FairyDust`.
- **Props:** `Prop_1x1/1x2/2x2/1x4` + `Barrier_2x2_*` (+ `World_test/` variants).
- **UI:** `HUD`.

## 7. Packages (direct dependencies)

**Content / gameplay:** 2D Animation, Aseprite Importer, PSD Importer, Sprite, SpriteShape, Tilemap (+ Extras), 2D Tooling, Input System, Cinemachine, Timeline, Visual Scripting, uGUI.

**Rendering:** Universal Render Pipeline (17.3.0), AI Inference (Sentis), AI Assistant (pre).

**Tooling / build:** Rider & VS editors, Unity Version Control, Test Framework, Multiplayer Center, Unity Pipeline, Linux x64 SDK + toolchain.

## 8. Project settings

- **Active build target:** `StandaloneLinux64` (Standalone group) · scripting backend **Mono2x** · API **.NET Standard 2.0**.
- **Product:** `Videojuegos_final` (company `DefaultCompany`, version `1.0`).
- **Quality:** level 0 = *Very Low* (also Low/Medium/High/Very High/Ultra), vSync off, no AA.
- **Graphics:** URP via `Assets/Settings/UniversalRP.asset` (URP 2D renderer `Renderer2D.asset`).
- **Tags:** `Player`, `Respawn`, `Finish`, `GameController`, `MainCamera`.
- **Layers:** `Ground`(5), `Walls`(6), `Player`(7), `Enemies`(8), `PlayerAttack`(9), `EnemyAttack`(10), `Pickups`(11), `Hazards`(12), `BossTrigger`(13).
- **Input Handling = Both** (new Input System with legacy fallback).
- **Cinemachine:** `CinemachineBrain` on Main Camera + `CM Camera` (`CinemachineFollow`).

## 9. Map generation (summary)

1. `RoomLayout` packs fixed cell-sized rooms (1×1, 2×1/1×2, 2×2, 3×3) on a large grid, reserving **Spawn** (center) and **Boss** (farthest) first — fully connected graph.
2. `MapGenerator` stamps archetypes (no rotation) and carves a passage at the midpoint of each shared cell edge (2 passages on 2-cell sides).
3. `RoomContentSpawner` populates each room by `tag` (`ZoneThemeSO`): barriers (validated to avoid sealing a room), decor props, and enemies, with density/tier scaled by player level (`DifficultyProfileSO`).
