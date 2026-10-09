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
| `Greybox` | Escena de pruebas autocontenida: mapa grande + jefe + enemigos del GDD + HUD. |

Todas están en **Build Settings** (MainMenu = índice 0). Todas tienen Cámara + `EventSystem`.

---

## 2. Estructura de scripts (`Assets/Scripts/`)

```
Boss/       BossAI, BossArenaController, BossManager, BossSpawnPoint,
            CaperucitaBoss, PeterPanBoss, IBossController
Core/       AnimationState, AttackType, Health (+IDamageable), Projectile, RunManager
Data/       BlockBankSO, BossAbilitySO, BossStatsSO, EnemyArchetypeSO,
            EnemyStatsSO, MapChunkSO, WorldConfigSO
Editor/     MapChunkCaptureEditor (Tools > Madness Dreams > Map Chunk Capture)
Enemies/    EnemyAI, EnemyAttack, EnemyFOV, EnemyMovement, SlowEffect
Gameplay/   Portal, WorldLayout, WorldPortalController
Generation/ CellRole, ChunkContent, MapGenerator, Spawner, TilemapStamper
HUD/        HUDController, HealthBarWidget, AttackCooldownWidget, AbilityHotbarWidget,
            BossHealthBarWidget, MinimapWidget, MapOverlayWidget, MapGrid,
            PauseController, DevMenuController, TaleFragmentWidget
Pickups/    AbilityPickup
Player/     PlayerController, PlayerCombat, PlayerAnimationController,
            PlayerAbilities, PlayerStats, PlayerDeathHandler
UI/         GameFlowManager, ScreenFader, LoadingScreen, SettingsData,
            MainMenuController, SettingsController, DefeatController
```

> Los antiguos scripts de depuración `EnemyGreyboxVisual` / `PlayerGreyboxVisual` **se eliminaron**:
> los colores de estado ahora los maneja el **Animator**.

---

## 3. Sistemas implementados

### 3.1 Datos (ScriptableObjects)
`Map Chunk`, `Block Bank`, `Enemy Stats`, `Boss Stats`, `Enemy Archetype`, `World Config`
y **`Boss Ability`** (nuevo, LOTE 2).

### 3.2 Generación procedural
`RunManager` → `MapGenerator` (`Configure`/`Generate`, expone `Config`, `StartChunk`,
`BossOriginChunk`, `ChunkToCell`, `ChunkCenterCell` y el evento `Generated`) → `TilemapStamper`
estampa los chunks del `BlockBankSO` → `ChunkContent` (+ `Spawner`) coloca enemigos del pool.
`ChunkContent` recibe el `config` desde `MapGenerator` (corregido).

### 3.3 Enemigos y jefes
- `EnemyAI` (FSM: `Patrol/Alert/Chase/Attack/Return/Dead`), `EnemyMovement` (con
  **`SpeedMultiplier`** para debuffs), `EnemyAttack` (melee/ranged), `EnemyFOV` (cono + línea de
  visión).
- **`SlowEffect`** (nuevo): multiplicador temporal de velocidad; lo aplica el proyectil de espinas.
- Jefes: `CaperucitaBoss` y `PeterPanBoss` (FSM propia + fases vía `BossStatsSO.phaseHealthThresholds`),
  `BossArenaController` (sella puertas, instancia el jefe, expone **`BossSpawned`** y
  **`BossInstance`**), `BossManager`, `BossSpawnPoint`.

### 3.4 Jugador
- `PlayerController`: movimiento (WASD/flechas), `ControlEnabled` (para el dash) y `SetMoveSpeed`.
- `PlayerCombat`: melee (`Space`) y ranged (`K`, hacia el mouse) + `SetDamageMultiplier`,
  `MeleeCooldownNormalized`/`RangedCooldownNormalized` y `damageMultiplier`.
- `PlayerAnimationController`: expone `CurrentState`.
- **`PlayerAbilities`** (LOTE 2): 4 slots (`1`–`4`), cooldown por slot, `Learn/GetSlot/
  GetCooldownNormalized/IsSlotLocked/SetSlotLocked/UseAbility`. Efectos: WolfClaw (dash + golpe),
  ThornShot (proyectil + slow), PeterDagger (abanico), FairyDust (dash).
- **`PlayerStats`**: nivel + estadísticas **base** (velocidad/daño/vida) + crecimiento por nivel;
  aplica a `PlayerController`/`PlayerCombat`/`Health`.
- `PlayerDeathHandler`: `Health.Died` → `GameFlowManager.NotifyPlayerDied()` → **Derrota**.

### 3.5 Animación (representada por color)
`Animator` con parámetro **int `State`** (0 Idle, 1 Move, 2 MeleeAttack, 3 RangedAttack, 4 Death).
Los clips animan `SpriteRenderer.m_Color`; el estado lógico (FSM del enemigo / jugador) escribe el
entero. Arte final = reemplazar los clips/sprite, sin tocar la lógica.

### 3.6 Flujo de pantallas (UI)
- `GameFlowManager` (singleton persistente): `StartRun`, `Retry`, `GoToMainMenu`, `GoToDefeat`,
  `OpenSettings`/`CloseSettings`, `LoadSceneWithLoading`, `PendingWorldIndex`,
  `NotifyPlayerDied`, `NotifyRunCompleted`.
- **`ScreenFader`** (nuevo): fundido a negro antes/después de cada carga de escena (transiciones
  suaves). Toda la navegación pasa por `TransitionTo`.
- `LoadingScreen`, `MainMenuController`, `DefeatController` (Reintentar/Menú/Salir),
  `SettingsController` + `SettingsData` (PlayerPrefs) y el evento `Closed`.

### 3.7 HUD (prefab `Assets/Prefabs/UI/HUD.prefab`)
- `HUDController` enlaza jugador/`MapGenerator`/`BossArenaController`; cada widget se actualiza solo.
- **Vida** (barra + texto) · **ataques** (sweeps de cooldown `Space`/`K`) · **hotbar** de 4
  habilidades (con cooldown y **bloqueo**: la habilidad desaparece y queda una sombra con “X”).
- **Barra del jefe** (aparece al iniciar el combate, se oculta al morir).
- **Minimapa** (rejilla + marcador del jugador **continuo**) y **mapa expandible** (clic en el
  minimapa o `M`).
- **Pausa** (`Esc` → `Time.timeScale = 0`) con **Ajustes in-game** (mismo `SettingsData`) y
  **Menú principal**. El input del jugador se ignora al pausar.
- **Menú DEV** (botón `DEV`): barras para **estadísticas base** (velocidad/daño/vida), **nivel**
  (−/+) y **bloqueo por habilidad**.

### 3.8 Flujo de mundo (portal → carga → siguiente mundo)
- Al **derrotar al jefe** se **activa un Portal** (`WorldPortalController` + `Portal`).
- Al tocarlo → `RunManager.RequestAdvance()` → `02_Loading` → `03_Gameplay` con el **siguiente
  `WorldConfigSO`** (índice persistido en `GameFlowManager`). Último mundo → `NotifyRunCompleted`.
- **`WorldLayout`**: al generarse el mundo coloca al **jugador en el centro del `StartChunk`** y la
  **arena en el centro del chunk del jefe** (corrige el spawn dentro de paredes).

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
  - `Assets/Prefabs/UI/`: `HUD`.
- **ScriptableObjects**: `Assets/ScriptableObjects/Enemies/` (stats + arquetipos + `BossStats_Caperucita`),
  `.../Abilities/` (4 habilidades), `.../Sample/` (`World_Sample_A/B`, `Bank_Sample`,
  `Chunk_Sample_A/B`, `Chunk_BossArena`).

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
- **Pruebas rápidas:** abrir `Greybox` y dar Play (mapa grande + jefe + enemigos).
- **Menú DEV**: botón `DEV` (abajo-derecha) para nivel, estadísticas base y bloqueo de habilidades.

---

## 8. Pendientes / a reemplazar

- **Arte final** (sprites, animaciones reales, UI): sustituye a las primitivas; la lógica no cambia.
- Montaje "real" de los mundos (chunks pintados, banco, `WorldConfigSO`) — hoy hay **sample**.
- Powerups restantes del GDD (Fragmentos de Cuento como moneda, contenedores de vida) — el contador
  existe como placeholder.
- `PeterPanBoss` aún sin prefab/stats (solo existe `Boss_Caperucita`).
