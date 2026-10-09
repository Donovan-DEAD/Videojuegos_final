# Madness Dreams — Estado de implementación en Unity

> Documento de referencia: **qué existe hoy** en el proyecto Unity, tras toda la implementación
> hecha. Sustituye a las descripciones "pendientes/planeadas" del *Manual de montaje* y del
> *Flujo de componentes* cuando haya diferencias.
>
> Proyecto: `Assets/` dentro del proyecto Unity (URP 2D, **Unity 6.0.3 LTS / 6000.3.x**).
> El arte es **greybox** (primitivas); la lógica está completa y es reemplazable por arte final.

---

## 1. Escenas

| Escena | Rol |
|---|---|
| `01_MainMenu` | Menú principal (Jugar / Ajustes / Salir + elegir protagonista). Primera del build. |
| `02_Loading` | Pantalla de carga asíncrona con barra y % (usa `GameFlowManager.NextScene`). |
| `03_Gameplay` | Nivel jugable: generación procedural, jugador, HUD, jefes, portal. |
| `04_Defeat` | Pantalla de derrota (Reintentar / Menú / Salir). |
| `05_Settings` | Ajustes (escena): volúmenes, pantalla completa. |
| `Greybox` | Escena de pruebas autocontenida: mapa hecho a mano + jefe + HUD. |

Todas están en **Build Settings** (MainMenu = índice 0). Todas tienen Cámara + `EventSystem`.

---

## 2. Estructura de scripts (`Assets/Scripts/`)

```
Boss/       BossAI, BossArenaController, BossManager, BossSpawnPoint,
            CaperucitaBoss, PeterPanBoss, IBossController
Core/       AnimationState, AttackType, Health (+IDamageable), Projectile, RunManager
Data/       BlockBankSO, BossAbilitySO, BossStatsSO, DifficultyProfileSO, EnemyArchetypeSO,
            EnemyStatsSO, MapChunkSO, PropDefinitionSO, RoomSizeClass, RoomTag, WorldConfigSO,
            ZoneThemeSO
Editor/     MapChunkCaptureEditor (captura de cuartos + Validate Worlds)
Enemies/    EnemyAI, EnemyAttack, EnemyFOV, EnemyMovement, SlowEffect
Gameplay/   Portal, WorldLayout, WorldPortalController
Generation/ CellRole, MapGenerator, RoomConnection, RoomContentSpawner, RoomLayout,
            RoomPlacement, Spawner, TilemapStamper
HUD/        HUDController, HealthBarWidget, AttackCooldownWidget, AbilityHotbarWidget,
            BossHealthBarWidget, MinimapWidget, MapOverlayWidget, MapGrid,
            PauseController, DevMenuController, TaleFragmentWidget
Pickups/    AbilityPickup
Player/     PlayerController, PlayerCombat, PlayerAnimationController,
            PlayerAbilities, PlayerStats, PlayerDeathHandler
UI/         GameFlowManager, ScreenFader, LoadingScreen, SettingsData,
            MainMenuController, SettingsController, DefeatController
```

> Cada carpeta tiene su `README.md` y **todos los scripts tienen documentación XML**.
> Los antiguos `EnemyGreyboxVisual` / `PlayerGreyboxVisual` y `ChunkContent` **se eliminaron**.

---

## 3. Sistemas implementados

### 3.1 Datos (ScriptableObjects)
- Núcleo: `Map Chunk`, `Block Bank`, `World Config`, `Enemy Stats`, `Boss Stats`,
  `Enemy Archetype`, `Boss Ability`.
- **Mapa procedural (nuevos):** `RoomTag` (temática), `RoomSizeClass`/`RoomSizes` (clases de tamaño),
  `Zone Theme` (banco por tag: enemigos + props + barreras), `Prop Definition` (decor/barrera) y
  `Difficulty Profile` (curva de densidad y tier de enemigos por nivel).

### 3.2 Generación procedural (cuartos de tamaño fijo por celdas)
`RunManager` → `MapGenerator.Generate()`:
1. **`RoomLayout`** reserva el cuarto de **Spawn** (centro) y el de **Boss** (el más lejano, 3×3) y luego
   empaqueta cuartos de tamaño fijo por celdas (1×1, 2×1/1×2, 2×2, 3×3) con pesos; asigna `tag` por peso
   (resolviendo `WorldConfigSO.Themes`) y calcula la adyacencia. El grafo es **totalmente conexo** y la
   rejilla es grande (14×14 por defecto en los mundos de sample).
2. **`TilemapStamper.Stamp`** estampa cada arquetipo **sin rotación**; después se **talla un paso** en el
   punto medio de cada celda compartida (`CarveConnections`, 2 pasos en lados de 2 celdas).
3. **`RoomContentSpawner.SpawnAll`** puebla cada cuarto por tag: **barreras** → **props** → **enemigos**.
4. Dispara **`Generated`** (lo consumen `WorldLayout` y el HUD).

`MapGenerator` expone `Rooms`, `Connections`, `StartRoom`, `BossRoom`, `StartRoomCenterCell`,
`BossRoomCenterCell`, `ChunkToCell` y el evento `Generated`.

`RoomContentSpawner`:
- resuelve el `ZoneThemeSO` por `tag` (con fallback a `RoomTag.None`);
- las **barreras** se validan con flood-fill para **no sellar** el cuarto (si sellan, se descartan);
- los **props** respetan `footprint` y evitan los pasos (`avoidPassages`);
- los **enemigos** salen del banco filtrado por **tier permitido** y la cantidad se modula por la
  densidad del `DifficultyProfileSO` según **`PlayerStats.Level`**, con **piso garantizado** (siempre
  puede salir el tier más bajo) y **techo duro por tamaño de cuarto**.

Convención de datos: **piso = celda sin tile**; sólo los muros llevan `TileBase`. Un cuarto es un
**espacio abierto con anillo de muro de 1 tile** en el borde de su footprint.

### 3.3 Enemigos y jefes
- `EnemyAI` (FSM: `Patrol/Alert/Chase/Attack/Return/Dead`), `EnemyMovement` (con
  **`SpeedMultiplier`** para debuffs), `EnemyAttack` (melee/ranged), `EnemyFOV` (cono + línea de
  visión).
- **Al morir** un enemigo: se reproduce el estado `Death` (color oscuro), se desactivan los colliders y
  se destruye tras `deathLingerTime` (2 s).
- **`SlowEffect`**: multiplicador temporal de velocidad; lo aplica el proyectil de espinas.
- Jefes: `CaperucitaBoss` y `PeterPanBoss` (FSM propia + fases vía `BossStatsSO.phaseHealthThresholds`),
  `BossArenaController` (sella salidas, instancia el jefe, expone **`BossSpawned`** y
  **`BossInstance`**), `BossManager`, `BossSpawnPoint`.

### 3.4 Jugador
- `PlayerController`: movimiento (WASD/flechas), `ControlEnabled` (para el dash) y `SetMoveSpeed`.
- `PlayerCombat`: melee (`Space`) y ranged (`K`, hacia el mouse) + `SetDamageMultiplier`,
  `MeleeCooldownNormalized`/`RangedCooldownNormalized`.
- `PlayerAnimationController`: expone `CurrentState`.
- **`PlayerAbilities`**: 4 slots (`1`–`4`), cooldown por slot, `Learn/GetSlot/
  GetCooldownNormalized/IsSlotLocked/SetSlotLocked/UseAbility`. Efectos: WolfClaw (dash + golpe),
  ThornShot (proyectil + slow), PeterDagger (abanico), FairyDust (dash).
- **`PlayerStats`**: nivel + estadísticas **base** (velocidad/daño/vida) + crecimiento por nivel;
  aplica a `PlayerController`/`PlayerCombat`/`Health`. **Su nivel alimenta la dificultad** del mapa.
- `PlayerDeathHandler`: `Health.Died` → `GameFlowManager.NotifyPlayerDied()` → **Derrota**.

### 3.5 Animación (representada por color)
`Animator` con parámetro **int `State`** (0 Idle, 1 Move, 2 MeleeAttack, 3 RangedAttack, 4 Death).
Los clips animan `SpriteRenderer.m_Color`; el estado lógico (FSM del enemigo / jugador) escribe el
entero. Arte final = reemplazar los clips/sprite, sin tocar la lógica.

### 3.6 Flujo de pantallas (UI)
- `GameFlowManager` (singleton persistente): `StartRun`, `Retry`, `GoToMainMenu`, `GoToDefeat`,
  `OpenSettings`/`CloseSettings`, `LoadSceneWithLoading`, `PendingWorldIndex`,
  `NotifyPlayerDied`, `NotifyRunCompleted`.
- **`ScreenFader`**: fundido a negro antes/después de cada carga de escena. Toda la navegación pasa
  por `TransitionTo`.
- `LoadingScreen`, `MainMenuController`, `DefeatController` (Reintentar/Menú/Salir),
  `SettingsController` + `SettingsData` (PlayerPrefs) y el evento `Closed`.

### 3.7 HUD (prefab `Assets/Prefabs/UI/HUD.prefab`)
- `HUDController` enlaza jugador/`MapGenerator`/`BossArenaController`; cada widget se actualiza solo.
- **Vida** (barra + texto) · **ataques** (sweeps de cooldown `Space`/`K`) · **hotbar** de 4
  habilidades (con cooldown y **bloqueo**: la habilidad desaparece y queda una sombra con “X”).
- **Barra del jefe** (aparece al iniciar el combate, se oculta al morir).
- **Minimapa y mapa**: `MapGrid.BuildRooms` dibuja **un rectángulo por cuarto** (soporta cuartos de
  varios tamaños) + marcador del jugador **continuo**. `MapGrid.Build` (grilla por celda) queda sólo
  como *fallback* cuando no hay `MapGenerator` (p. ej. `Greybox`).
- **Pausa** (`Esc` → `Time.timeScale = 0`) con **Ajustes in-game** (mismo `SettingsData`) y
  **Menú principal**. El input del jugador se ignora al pausar.
- **Menú DEV** (botón `DEV`): barras para **estadísticas base** (velocidad/daño/vida), **nivel**
  (−/+) y **bloqueo por habilidad**.

### 3.8 Flujo de mundo (portal → carga → siguiente mundo)
- Al **derrotar al jefe** se **activa un Portal** (`WorldPortalController` + `Portal`).
- Al tocarlo → `RunManager.RequestAdvance()` → `02_Loading` → `03_Gameplay` con el **siguiente
  `WorldConfigSO`** (índice persistido en `GameFlowManager`). Último mundo → `NotifyRunCompleted`.
- **`WorldLayout`**: al generarse el mundo coloca al **jugador en el centro del cuarto de Spawn**
  (`StartRoomCenterCell`) y la **arena en el centro del cuarto del jefe** (`BossRoomCenterCell`).
- Si no hay `BossManager` (p. ej. `Greybox`), `WorldPortalController` engancha directamente el `Health`
  del jefe colocado en la escena.

---

## 4. Assets creados (greybox)

- **Arte**: `Assets/Art/Greybox/Square.png` (cuadrado blanco+borde), `Circle.png` (jugador).
- **Tile**: `Assets/Tiles/Tile_Wall.asset`.
- **Animaciones**: `Assets/Animations/Player/PlayerAnimator.controller` y
  `Assets/Animations/Enemy/EnemyAnimator.controller` + clips `Idle/Move/MeleeAttack/RangedAttack/Death`.
- **Prefabs**:
  - `Assets/Prefabs/Greybox/`: `Player`, `Projectile_Player`, `Projectile_Thorn`.
  - `Assets/Prefabs/Enemies/`: enemigos del GDD `Enemy_Wolf/Bramble/Woodcutter/Firefly/CabinBoy/
    Mermaid/Crocodile/DustFairy` + `Boss_Caperucita` + `Projectile_Enemy`.
  - `Assets/Prefabs/Pickups/`: `Pickup_WolfClaw/ThornShot/PeterDagger/FairyDust`.
  - `Assets/Prefabs/Props/`: `Prop_Tree`, `Prop_Bush`, `Prop_Rock` (decor) + `Barrier_Rock`,
    `Barrier_Arch`, `Barrier_Mountain` (barreras con collider, capa `Walls`).
  - `Assets/Prefabs/UI/`: `HUD`.
- **ScriptableObjects**:
  - `.../Sample/`: `World_Sample_A/B` (14×14, temas y dificultad cableados), `Bank_Sample`,
    `Chunk_Sample_A/B`, `Chunk_BossArena`.
  - `.../Rooms/`: arquetipos `Room_Forest_*` / `Room_Sea_*` (1×1, 2×1, 1×2, 2×2, 3×3) y
    `Room_Boss_3x3`.
  - `.../Themes/`: `ZoneTheme_Forest`, `ZoneTheme_Sea`. `.../Difficulty/`: `DifficultyProfile_Default`.
  - `.../Props/`: definiciones de los 6 props/barreras.
  - `.../Enemies/`: stats + arquetipos (con `tier`/`weight`) + `BossStats_Caperucita`.
  - `.../Abilities/`: 4 habilidades.

---

## 5. Configuración del proyecto

- **Layers** (6–14): `Ground, Walls, Player, Enemies, PlayerAttack, EnemyAttack, Pickups, Hazards,
  BossTrigger`. Tag `Player`.
- **Matriz de colisión 2D**: los proyectiles solo chocan con su objetivo; `Enemies × EnemyAttack` y
  `PlayerAttack × Player` y `Walls × (PlayerAttack/EnemyAttack)` desactivados.
- **Input Handling = Both** (legacy + Input System). El jugador usa el **nuevo Input System**
  (`WASD`+flechas, `Space`, `K`, mouse) con *fallback* legacy.
- **Cinemachine 3.1.7**: `CinemachineBrain` en la cámara + `CM Camera` (`CinemachineFollow`) que
  sigue al jugador.

---

## 6. Controles

| Tecla | Acción |
|---|---|
| `WASD` / flechas | Mover |
| `Space` | Ataque melee |
| `K` | Ataque a distancia (hacia el mouse) |
| `1`–`4` | Habilidades (WolfClaw / ThornShot / PeterDagger / FairyDust) |
| `Esc` | Pausa (Reanudar / Ajustes / Menú) |
| `M` o clic en el minimapa | Ver/ocultar el mapa grande |

---

## 7. Cómo probar

- **Nivel completo:** abrir `01_MainMenu` → **Jugar** → `02_Loading` → `03_Gameplay`.
- **Pruebas rápidas:** abrir `Greybox` y dar Play (mapa manual + jefe).
- **Mapa procedural:** en `03_Gameplay`, `RunManager` genera el mundo; ver `Estado_Implementacion` §3.2.
- **Validar mundos:** `Tools > Madness Dreams > Validate Worlds` (1 Spawn y 1 Boss por mundo).
- **Menú DEV**: botón `DEV` (abajo-derecha) para nivel, estadísticas base y bloqueo de habilidades.

---

## 8. Pendientes / a reemplazar

- **Arte final** (sprites, animaciones reales, UI, props): sustituye a las primitivas; la lógica no cambia.
- **Arquetipos de cuarto definitivos**: los actuales son anillos de muro greybox; falta pintar cuartos
  reales por tamaño × tag (el pipeline ya los consume).
- **Balance** de `DifficultyProfileSO` y densidades de `ZoneThemeSO` (ajustar jugando).
- Powerups restantes del GDD (Fragmentos de Cuento como moneda, contenedores de vida) — el contador
  existe como placeholder.
- `PeterPanBoss` aún sin prefab/stats (solo existe `Boss_Caperucita`).
- Cámara: el tamaño ortográfico actual (16 tiles) muestra una celda; revisar zoom para cuartos 2×2/3×3.
