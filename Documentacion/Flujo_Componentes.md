# Flujo completo de componentes — Madness Dreams

> Documento de referencia para entender qué hace cada script y cómo se conectan entre sí.
>
> **Actualizado:** además de los sistemas originales (datos, generación, enemigos, jefes, núcleo,
> jugador), ahora incluye **Flujo de pantallas (UI)**, **HUD**, **Habilidades (LOTE 2)**,
> **Flujo de mundo (portal)** y **Menú DEV** (secciones 10–14). Ver también
> [`Estado_Implementacion_Unity.md`](./Estado_Implementacion_Unity.md).

## 1. Visión general

El proyecto se organiza en capas:

1. **Datos (ScriptableObjects)** — configuración editable sin lógica de frame.
2. **Generación procedural** — construye el mapa con semilla.
3. **Enemigos** — FSM, movimiento, ataque y visión.
4. **Jefes** — FSM con fases + arena.
5. **Núcleo** — vida, proyectiles, flujo de mundos y enums.
6. **Jugador** — movimiento, combate y animación.

### Flujo general

```
RunManager
   └─ carga WorldConfigSO
        └─ MapGenerator estampa chunks (TilemapStamper)
             └─ ChunkContent / Spawner colocan enemigos
                  └─ el jugador explora y combate
                       └─ BossArenaController spawnea al jefe
                            └─ al morir, RunManager pasa al siguiente mundo
```

## 2. Capa de datos (ScriptableObjects)

### BlockBankSO
- **Propósito**: banco de chunks disponibles por nivel + la arena del jefe.
- **Campos**: `normalChunks` (lista de `MapChunkSO`), `bossArena` (`MapChunkSO`).
- **Métodos**: `GetRandomChunk(System.Random)`, `GetChunk(int)`.
- **Flujo**: `MapGenerator` pide un chunk aleatorio por cada celda del grid.

### MapChunkSO
- **Propósito**: plantilla de un bloque/room (tiles, puertas y puntos de spawn).
- **Campos**: `size`, `tiles` (aplanado fila a fila), `doorPositions`, `enemySpawns`, `role`.
- **Helpers**: `GetTile(x, y)`, `LocalArea`, `SetSize`, `SetTileData`.
- **Flujo**: el editor captura un `Tilemap` pintado con `GetTilesBlock` y guarda los datos; `TilemapStamper` lo estampa con `SetTilesBlock`.

### EnemyStatsSO
- **Propósito**: plantilla de propiedades comunes de un enemigo.
- **Campos**: `moveSpeed`, `chaseSpeed`, `maxHealth`, `damage`, `attackRange`, `attackCooldown`, `windupTime`, `attackType`, `projectilePrefab`, `projectileSpeed`, `detectionRadius`, `fovRadius`, `fovAngle`, `hearingRadius`.
- **Flujo**: `EnemyAI` inyecta estos stats en `EnemyMovement`, `EnemyAttack`, `EnemyFOV` y `Health`.

### BossStatsSO
- **Hereda de**: `EnemyStatsSO`.
- **Campos extra**: `phaseHealthThresholds`, `phaseAttacks`.
- **Flujo**: `BossAI` usa los umbrales para detectar cambios de fase.

### EnemyArchetypeSO
- **Propósito**: vincula un prefab con sus stats.
- **Campos**: `prefab`, `stats`.

### WorldConfigSO
- **Propósito**: configuración de un mundo (cuento).
- **Campos**: `seed`, `bank`, `width`, `height`, `chunkSize`, `bossRegionSize`, `enemyPool`, `enemiesPerChunk`, `bossPrefab`, `boss`.

### MapChunkCaptureEditor (editor)
- **Propósito**: capturar un room pintado a un `MapChunkSO`.
- **Flujo**: `Tools > Madness Dreams > Map Chunk Capture` → asignar Tilemap y target → `Capture` → `GetTilesBlock` + aplanado → guardar en el SO.

## 3. Generación procedural

### CellRole
- Enum: `Empty`, `Start`, `Normal`, `Boss`. Marca el rol de cada celda del grid.

### TilemapStamper
- **Propósito**: estampar un `MapChunkSO` en un Tilemap global.
- **Flujo**: `Stamp(tilemap, chunk, origin, rotationSteps)` → rota el layout si es necesario → `SetTilesBlock`.

### MapGenerator
- **Propósito**: orquestar toda la generación.
- **Flujo**:
  1. Valida `config`, `bank` y `targetTilemap`; limpia el tilemap.
  2. Crea `System.Random(seed)`.
  3. `BuildRoles`: `Start` al centro, `Boss` en la celda más lejana (región `bossRegionSize`), resto `Normal`.
  4. `StampNormalChunks`: por celda `Normal`/`Start`, pide un chunk al banco, rota y estampa.
  5. `StampBossArena`: estampa la arena en la región del jefe.
  6. `SpawnEnemies`: delega en `ChunkContent`.

### Spawner
- **Propósito**: instanciar con chequeo de colisión.
- **Flujo**: `TrySpawn(prefab, position, rng)` → genera candidatos con jitter → `Physics2D.OverlapCircle` → instancia si no hay solapamiento (hasta `maxTries`).

### ChunkContent
- **Propósito**: poblar un chunk con enemigos.
- **Flujo**: lee `enemySpawns` del chunk, elige enemigos del `enemyPool`, convierte celda→mundo con `GetCellCenterWorld` y usa el `Spawner`.

## 4. Enemigos

### EnemyAI (FSM central)
- **Estados**: `Patrol`, `Alert`, `Chase`, `Attack`, `Return`, `Dead`.
- **Por frame**: `attack.Tick`, `UpdatePerception` (FOV con intervalo), switch de estado, `UpdateAnimation`.

Transiciones:

| De → A | Condición |
|---|---|
| `Patrol` → `Alert` | Jugador visto u oído |
| `Alert` → `Chase` | Terminó el tiempo de alerta |
| `Chase` → `Attack` | Distancia ≤ `attackRange` |
| `Attack` → `Chase` | Jugador salió de `attackRange` |
| `Chase` → `Return` | Jugador fuera de `detectionRadius` por un tiempo |
| `Return` → `Patrol` | Llegó al punto de origen |
| Cualquiera → `Dead` | Vida ≤ 0 |

### EnemyMovement
- **Flujo**: `MoveDirection`, `MoveTowards` y `Stop` setean la velocidad del `Rigidbody2D`.

### EnemyAttack
- **Flujo**: según `stats.attackType`:
  - `Melee`: corrutina con `windupTime` → aplica daño en rango.
  - `Ranged`: corrutina con `windupTime` → instancia `stats.projectilePrefab` y lo lanza.

### EnemyFOV
- **Flujo**: `CanHearPlayer` (radio) y `CanSeePlayer` (radio + cono + línea de visión con raycast contra `obstacleMask`).

### Projectile
- **Flujo**: `Launch` setea dirección y velocidad → vuela con `Rigidbody2D` → `OnTriggerEnter2D` daña si el collider está en `targetMask` → se autodestruye por `lifetime`.

## 5. Jefes

### BossAI
- **Hereda de**: `EnemyAI`.
- **Flujo**: calcula la fase según la vida restante (`phaseHealthThresholds`) y, en fase 2+, invoca esbirros con el `Spawner`.

### BossSpawnPoint
- **Propósito**: marcador de posición del jefe (con gizmo).

### BossArenaController
- **Flujo**: `OnTriggerEnter2D` (capa `BossTrigger`) → bloquea puertas (`exitBlockers`) → spawnea el jefe → al morir, desbloquea y dispara `BossDefeated`.

### BossManager
- **Flujo**: configura la arena desde `WorldConfigSO` (`Setup`) y escucha `BossDefeated` para avisar a `RunManager`.

## 6. Núcleo

### Health / IDamageable
- **Flujo**: `TakeDamage` reduce la vida, dispara `HealthChanged` y, al llegar a 0, `Died`.

### RunManager
- **Flujo**: en `Start` suscribe `BossDefeated` y genera el mundo 0; al derrotar al jefe, avanza al siguiente mundo.

### AnimationState
- Enum: `Idle`, `Move`, `MeleeAttack`, `RangedAttack`, `Death`.

### AttackType
- Enum: `Melee`, `Ranged`.

## 7. Jugador

### PlayerController
- **Flujo**: lee el input en `Update` y setea la velocidad en `FixedUpdate`; expone `IsMoving`.

### PlayerCombat
- **Flujo**:
  - Melee (`Space`): `OverlapCircleAll` contra `enemyMask` y daño a cada `IDamageable`.
  - Ranged (`K`): instancia `projectilePrefab` hacia el ratón y lo lanza.
  - Expone `IsMeleeAttacking` / `IsRangedAttacking`.

### PlayerAnimationController
- **Flujo**: cada frame determina `AnimationState` (muerte > ataque > movimiento > idle) y setea el parámetro `State` del Animator.

## 8. Flujo end-to-end

1. `RunManager` carga el mundo 0.
2. `MapGenerator` genera tiles y reserva la región del jefe.
3. `ChunkContent`/`Spawner` spawnean esbirros.
4. El jugador explora; los enemigos detectan, persiguen y atacan.
5. Al entrar en la arena, `BossArenaController` sella las puertas y spawnea al jefe.
6. `BossAI` pelea cambiando de fase.
7. El jefe muere → `BossManager`/`RunManager` avanzan al siguiente mundo con nueva semilla.

## 9. Resumen de estados de animación

Parámetro int `State` del Animator:

| Valor | Estado |
|---|---|
| 0 | Idle |
| 1 | Move |
| 2 | MeleeAttack |
| 3 | RangedAttack |
| 4 | Death |

Mapeo enemigo (FSM → animación):

| Estado FSM | Animación |
|---|---|
| `Patrol` (quieto) | Idle |
| `Patrol` (moviendo) | Move |
| `Alert` | Idle |
| `Chase` | Move |
| `Return` | Move |
| `Attack` (melee) | MeleeAttack |
| `Attack` (ranged) | RangedAttack |
| `Dead` | Death |

Mapeo jugador:

| Condición | Animación |
|---|---|
| Vida ≤ 0 | Death |
| `IsMeleeAttacking` | MeleeAttack |
| `IsRangedAttacking` | RangedAttack |
| `IsMoving` | Move |
| resto | Idle |

---

## 10. Flujo de pantallas (UI)

- **`GameFlowManager`** (singleton persistente, `DontDestroyOnLoad`): mantiene el estado de la run
  (`SelectedCharacter`, `RunSeed`, `PendingWorldIndex`, `Settings`) y navega entre escenas:
  `StartRun`, `Retry`, `GoToMainMenu`, `GoToDefeat`, `OpenSettings`/`CloseSettings`,
  `LoadSceneWithLoading`, `NotifyPlayerDied`, `NotifyRunCompleted`.
- **`ScreenFader`** (singleton persistente): fundido a negro. Toda navegación pasa por
  `GameFlowManager.TransitionTo` → `FadeOut` → `LoadScene` → `FadeIn` (evento `sceneLoaded`).
- **`LoadingScreen`** (`02_Loading`): carga asíncrona del `NextScene` con barra y %.
- **`MainMenuController`**: Jugar / Ajustes / Salir + toggle de protagonista.
- **`DefeatController`** (`04_Defeat`): Reintentar / Menú principal / Salir.
- **`SettingsData`** + **`SettingsController`**: ajustes (volúmenes/pantalla completa) con
  `PlayerPrefs`. `SettingsController` expone el evento `Closed` (para reutilizarlo como panel
  in-game).

**Flujo:** `MainMenu → (Jugar) → Loading → Gameplay → (muerte) → Defeat → Menú/Reintentar`.
`Esc` en el gameplay abre la pausa (con Ajustes in-game) sin salir de la escena.

## 11. HUD

`HUDController` enlaza al `Player`, al `MapGenerator` y al `BossArenaController`; cada widget se
auto-gestiona:

- **`HealthBarWidget`** ← evento `Health.HealthChanged`.
- **`AttackCooldownWidget`** ← `PlayerCombat.MeleeCooldownNormalized`/`RangedCooldownNormalized`.
- **`AbilityHotbarWidget`** ← lee `PlayerAbilities` (ícono, cooldown y **bloqueo**: el ícono
  desaparece y queda una sombra con “X”).
- **`BossHealthBarWidget`** ← evento `BossArenaController.BossSpawned` (y se oculta con
  `BossDefeated`).
- **`MinimapWidget`** y **`MapOverlayWidget`** ← `MapGenerator` (+ `MapGrid` comparte la rejilla y
  el mapeo mundo→minimapa; el marcador es **continuo**). Clic en el minimapa (o `M`) abre el mapa.
- **`TaleFragmentWidget`** (placeholder) y **`PauseController`** (pausa + Ajustes in-game) y
  **`DevMenuController`** (sección 14).

## 12. Habilidades (LOTE 2)

- **`BossAbilitySO`** (Data): tipo, tiempos, daño/rango, proyectil (prefab/velocidad/cantidad/abanico),
  slow y dash.
- **`PlayerAbilities`** (jugador): 4 slots (`1`–`4`), cooldown por slot; `Learn` (pickups),
  `UseAbility`, `IsSlotLocked`/`SetSlotLocked`. Efectos: **WolfClaw** (dash + golpe),
  **ThornShot** (proyectil + slow), **PeterDagger** (abanico), **FairyDust** (dash).
- **`AbilityPickup`** (Pickups): al recogerlo llama `PlayerAbilities.Learn` (solo se destruye si se
  aprendió). El jefe lo suelta con su `lootPrefab`.
- **`SlowEffect`** (Enemies): aplica `EnemyMovement.SpeedMultiplier`; lo dispara **`Projectile`**
  cuando se lanza con `slowMultiplier`/`slowDuration`.

## 13. Flujo de mundo (portal → carga → siguiente mundo)

- `RunManager.GenerateWorld(index)` configura `MapGenerator` + `BossManager` y genera.
- **`WorldLayout`**: al evento `MapGenerator.Generated` coloca al **jugador en el centro del
  `StartChunk`** y la **arena en el centro del chunk del jefe**.
- Al **derrotar al jefe** → **`WorldPortalController`** activa el **`Portal`** (en la arena).
- El jugador toca el `Portal` → **`RunManager.RequestAdvance()`** →
  `GameFlowManager.SetPendingWorldIndex(+1)` + `LoadSceneWithLoading(Gameplay)` → `02_Loading` →
  `03_Gameplay` con el **siguiente `WorldConfigSO`**. Si era el último → `NotifyRunCompleted`.

## 14. Menú DEV

- **`DevMenuController`** (HUD, botón `DEV`): barras para las **estadísticas base** (velocidad, daño
  ×, vida), **nivel** (−/+) y **bloqueo por habilidad**.
- Está respaldado por **`PlayerStats`** (`SetLevel`, `SetBaseMoveSpeed`, `SetBaseDamageMultiplier`,
  `SetBaseHealth`; aplica a `PlayerController.SetMoveSpeed`, `PlayerCombat.SetDamageMultiplier` y
  `Health.SetMaxHealth`) y por `PlayerAbilities.SetSlotLocked`.
