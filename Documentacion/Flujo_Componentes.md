# Flujo completo de componentes — Madness Dreams

> Documento de referencia para entender qué hace cada script y cómo se conectan entre sí.
>
> **Actualizado:** además de los sistemas originales (datos, enemigos, jefes, núcleo, jugador), incluye
> el **mapa procedural por cuartos** (secciones 2–3), y además **Flujo de pantallas (UI)**, **HUD**,
> **Habilidades (LOTE 2)**, **Flujo de mundo (portal)** y **Menú DEV** (secciones 10–14). Ver también
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
        └─ RoomLayout reserva Spawn (centro) + Boss (el más lejano) y empaqueta cuartos
           de tamaño fijo por celdas (1x1, 2x1/1x2, 2x2, 3x3), asignando tag por cuarto
             └─ MapGenerator estampa (TilemapStamper) y talla los pasos de cada celda compartida
                  └─ RoomContentSpawner puebla por tag: barreras → props → enemigos
                       └─ el jugador explora y combate
                            └─ BossArenaController spawnea al jefe
                                 └─ al morir se activa el Portal y RunManager pasa al siguiente mundo
```

## 2. Capa de datos (ScriptableObjects)

### BlockBankSO
- **Propósito**: banco de arquetipos de cuarto + el cuarto de Spawn y la arena del jefe.
- **Campos**: `normalChunks` (lista de `MapChunkSO`), `startRoom` (`MapChunkSO`), `bossArena` (`MapChunkSO`).
- **Métodos**: `GetRandomChunk(System.Random)`, `GetChunk(int)`, `FindBySize(sizeInCells, cellSize, results)`, `FindByTag(tag, results)`.
- **Flujo**: `RoomLayout` pide un arquetipo por **clase de tamaño + tag** en cada celda libre.

### MapChunkSO
- **Propósito**: arquetipo de un cuarto (tamaño, tiles, puntos de spawn y regiones de props).
- **Campos**: `sizeInCells` (1×1, 2×1, 1×2, 2×2, 3×3), `tag` (`RoomTag`), `role` (`CellRole`),
  `size` (tiles), `tiles` (aplanado fila a fila), `enemySpawns` (`{localPosition, weight}`),
  `propRegions`.
- **Helpers**: `GetTile(x, y)`, `LocalArea`, `TileIndex`, `ResolveSizeInCells(cellSize)`,
  `SetSize`/`SetTileData`/`SetSizeInCells`/`SetTag`.
- **Flujo**: el editor captura el `Tilemap` (`GetTilesBlock`) y los spawn points desde los hijos;
  `TilemapStamper` lo estampa con `SetTilesBlock`.
- **Convención**: espacio abierto con **anillo de muro de 1 tile**; **piso = celda sin tile**.

### RoomTag · RoomSizeClass / RoomSizes
- `RoomTag`: temática del cuarto (`None`, `Forest`, `Sea`, `Cave`, `Ruins`, …).
- `RoomSizeClass`: `Small` (1×1), `Wide` (2×1/1×2), `Square` (2×2), `Large` (3×3).
- `RoomSizes`: `Candidates(class)` (formas posibles), `Classify(cells)` y `CellCount(cells)`.

### ZoneThemeSO
- **Propósito**: el **banco por tag** — qué enemigos, props y barreras puede usar un cuarto.
- **Campos**: `tag`, `enemyBank`/`propBank`/`barrierBank` (listas ponderadas), y densidades base
  (`enemiesMin/Max`, `propsMin/Max`, `barriersMin/Max`).
- **Flujo**: `RoomContentSpawner` resuelve el tema por el `tag` del cuarto
  (`WorldConfigSO.ThemeFor`), con fallback a `RoomTag.None`.

### PropDefinitionSO
- **Propósito**: un prop de ambientación.
- **Campos**: `prefab`, `kind` (`Decor` sin collider / `Barrier` con collider), `footprint` (tiles),
  `weight`, `avoidPassages`.

### DifficultyProfileSO
- **Propósito**: curva de dificultad del combate.
- **Campos**: `entries` (`{minLevel, maxTier, densityMultiplier, enemiesMin/Max}`), topes duros por
  tamaño de cuarto y `minTierGuaranteed` (piso).
- **Flujo**: `RoomContentSpawner` la consulta con **`PlayerStats.Level`** para filtrar el `tier` de
  enemigos y modular la cantidad.

### EnemyStatsSO
- **Propósito**: plantilla de propiedades comunes de un enemigo.
- **Campos**: `moveSpeed`, `chaseSpeed`, `maxHealth`, `damage`, `attackRange`, `attackCooldown`, `windupTime`, `attackType`, `projectilePrefab`, `projectileSpeed`, `detectionRadius`, `fovRadius`, `fovAngle`, `hearingRadius`.
- **Flujo**: `EnemyAI` inyecta estos stats en `EnemyMovement`, `EnemyAttack`, `EnemyFOV` y `Health`.

### BossStatsSO
- **Hereda de**: `EnemyStatsSO`.
- **Campos extra**: `phaseHealthThresholds`, `phaseAttacks`.
- **Flujo**: `BossAI` usa los umbrales para detectar cambios de fase.

### EnemyArchetypeSO
- **Propósito**: vincula un prefab con sus stats y su nivel de poder.
- **Campos**: `prefab`, `stats`, `tier` (1..N), `weight`.

### WorldConfigSO
- **Propósito**: configuración de un mundo (cuento).
- **Campos**: `seed`, `bank`, `width`, `height` (rejilla, por defecto 14×14), `chunkSize`,
  `bossRegionSize`, `sizeWeights` (peso por clase de tamaño), `openingWidth` (ancho del paso),
  `fillRatio`, `themes` (lista `tag → ZoneThemeSO`), `difficulty` (`DifficultyProfileSO`),
  `bossPrefab`, `boss`.
- **Métodos**: `WeightFor(RoomSizeClass)`, `ThemeFor(RoomTag)`.

### MapChunkCaptureEditor (editor)
- **Propósito**: capturar un cuarto pintado a un `MapChunkSO` y validar los mundos.
- **Flujo**: `Tools > Madness Dreams > Map Chunk Capture` → asignar Tilemap, target, `Cell size` y
  `Tag` → `Capture` → calcula los límites reales de los tiles, aplana (`GetTilesBlock`), deriva
  `sizeInCells`, guarda `tag` y recoge los `enemySpawns` desde los **hijos** nombrados con el prefijo
  (por defecto `EnemySpawn`).
- **Validaciones**: tamaño múltiplo de la celda, anillo de muro de 1 tile y presencia de spawn points.
- **`Tools > Madness Dreams > Validate Worlds`**: corre `RoomLayout` por cada `WorldConfigSO` y
  comprueba que haya **exactamente 1 cuarto de Spawn y 1 de Boss**.

## 3. Generación procedural (mapa por cuartos)

### CellRole
- Enum: `Empty`, `Start`, `Normal`, `Boss`. Marca el rol de cada cuarto (no de cada celda de tile).

### RoomPlacement / RoomConnection
- `RoomPlacement`: struct con `chunk`, `sizeInCells`, `originCell` (en celdas de rejilla), `tag` y `role`.
- `RoomConnection` / `RoomSide` (`Right`, `Up`): qué dos cuartos conecta una celda compartida, y por qué lado.

### RoomLayout (estático)
- **Propósito**: construir la topología del nivel.
- **Flujo**:
  1. **Reserva el cuarto de Spawn** (centro de la rejilla) y el de **Boss** (el más lejano que quepa,
     con degradación de tamaño si no entra). Garantizados en todos los mundos; sin sobrescrituras.
  2. **Empaqueta** el resto: recorre las celdas en orden aleatorio, elige una **clase de tamaño** por
     peso (`WorldConfigSO.WeightFor`) y coloca un arquetipo del banco filtrado por **tamaño + tag**.
     Las celdas sobrantes se rellenan con 1×1 (cobertura total).
  3. **Adyacencia**: cada par de celdas vecinas de cuartos distintos genera una `RoomConnection`.
     El grafo resultante es **totalmente conexo**.

### TilemapStamper
- **Propósito**: estampar un `MapChunkSO` en el Tilemap global y tallar los pasos.
- **Flujo**: `Stamp(tilemap, chunk, origin)` → `SetTilesBlock` (sin rotación).
  `ClearCells(tilemap, min, size)` limpia el borde para abrir el paso.

### MapGenerator
- **Propósito**: orquestar toda la generación.
- **Flujo**:
  1. Valida `config`, `bank` y `targetTilemap`; limpia el tilemap y las listas.
  2. Crea `System.Random(seed)` y llama a `RoomLayout.Build`.
  3. `StampRooms`: estampa cada arquetipo en su celda de origen.
  4. `CarveConnections`: por cada conexión limpia el borde (`openingWidth` centrado) — un paso por celda
     compartida, **2 en lados de 2 celdas**.
  5. `SpawnContent`: delega en `RoomContentSpawner`.
  6. Dispara `Generated` (lo consumen `WorldLayout` y el HUD).
- **API**: `Rooms`, `Connections`, `StartRoom`, `BossRoom`, `StartRoomCenterCell`, `BossRoomCenterCell`,
  `ChunkToCell`, `ChunkCenterCell`, `RoomCenterCell`.

### Spawner
- **Propósito**: instanciar con chequeo de colisión.
- **Flujo**: `TrySpawn(prefab, position, rng)` → genera candidatos con jitter → `Physics2D.OverlapCircle` → instancia si no hay solapamiento (hasta `maxTries`).

### RoomContentSpawner
- **Propósito**: poblar cada cuarto según su `tag`.
- **Flujo**, por cuarto (el cuarto de Spawn no recibe enemigos ni barreras):
  1. Resuelve el `ZoneThemeSO` por `tag` (fallback `RoomTag.None`).
  2. **Barreras** primero: las busca apoyadas al borde y **valida con flood-fill** que no sellen el
     cuarto (si sellan, se descartan). El `footprint` no puede pisar props/aberturas.
  3. **Props** de decoración: esparcidos sobre piso libre, evitando los pasos si `avoidPassages`.
  4. **Enemigos**: en los `enemySpawns` del arquetipo (respetando `weight`), del banco **filtrado por
     tier** permitido; cantidad modulada por la densidad del `DifficultyProfileSO` según
     `PlayerStats.Level`, con piso garantizado y techo por tamaño de cuarto.

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
2. `RoomLayout` empaqueta los cuartos y `MapGenerator` genera los tiles y talla los pasos.
3. `RoomContentSpawner`/`Spawner` pueblan los cuartos (barreras, props y esbirros por tag).
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
