# Madness Dreams

Roguelite 2D top-down (estilo *Binding of Isaac* / *Cult of the Lamb*): mundos de cuentos
distorsionados, generación procedural por semilla, jefes por fases y progresión persistente.

> **Documentación técnica detallada:** ver **[`Estado_Implementacion_Unity.md`](./Estado_Implementacion_Unity.md)**
> (qué existe hoy en Unity, escena por escena y sistema por sistema).
> El *Manual de montaje* y el *Flujo de componentes* siguen siendo válidos como guía conceptual; este
> README resume el estado real del proyecto.

---

## Estructura de scripts (`Assets/Scripts/`)

```
Boss/       Jefes y arena (BossAI, CaperucitaBoss, PeterPanBoss, BossArenaController, BossManager)
Core/       Vida, proyectil, RunManager y enums (Health, Projectile, AnimationState, AttackType)
Data/       ScriptableObjects (WorldConfig, BlockBank, MapChunk, RoomTag, RoomSizeClass,
            ZoneTheme, PropDefinition, DifficultyProfile, EnemyStats, BossStats,
            EnemyArchetype, BossAbility)
Editor/     Captura de cuartos + validador de mundos (Tools > Madness Dreams)
Enemies/    IA y combate de esbirros (EnemyAI, EnemyMovement, EnemyAttack, EnemyFOV, SlowEffect)
Gameplay/   Flujo de nivel (WorldLayout, WorldPortalController, Portal)
Generation/ Mapa procedural (MapGenerator, RoomLayout, RoomContentSpawner, TilemapStamper,
            Spawner, RoomPlacement, RoomConnection, CellRole)
HUD/        Interfaz de gameplay (HUDController y widgets, Pausa, Mapa, Menú DEV)
Pickups/    Recogibles (AbilityPickup)
Player/     Jugador (Controller, Combat, Abilities, Stats, Animation, DeathHandler)
UI/         Flujo de pantallas (GameFlowManager, ScreenFader, LoadingScreen, Settings, Menús)
```

> Cada carpeta tiene su propio `README.md` con el detalle de sus archivos.

## Escenas (`Assets/Scenes/`)

| Escena | Rol |
|---|---|
| `01_MainMenu` | Menú principal (primera del build) |
| `02_Loading` | Pantalla de carga |
| `03_Gameplay` | Nivel jugable |
| `04_Defeat` | Derrota (Reintentar / Menú / Salir) |
| `05_Settings` | Ajustes |
| `Greybox` | Escena de pruebas (mapa manual + jefe) |

## Cómo ejecutar

- **Juego completo:** abrir `01_MainMenu` → **Play** → Jugar.
- **Pruebas:** abrir `Greybox` → **Play**.

## Controles

| Tecla | Acción |
|---|---|
| `WASD` / flechas | Mover |
| `Space` | Melee |
| `K` | Ranged (hacia el mouse) |
| `1`–`4` | Habilidades de jefe |
| `Esc` | Pausa (Reanudar / Ajustes / Menú) |
| `M` / clic en el minimapa | Mapa grande |

---

## Generación procedural (cuartos por celdas)

1. **`RoomLayout`** empaqueta cuartos de **tamaño fijo por celdas** (1×1, 2×1 / 1×2, 2×2, 3×3) sobre una
   rejilla grande. Primero **reserva** el cuarto de **Spawn** (centro) y el de **Boss** (el más lejano);
   el resto se rellena por peso de clase de tamaño. Asigna un **`tag`** temático por cuarto y calcula la
   **adyacencia** celda a celda → el grafo es **totalmente conexo**.
2. **`MapGenerator`** estampa cada arquetipo (sin rotación) y **talla un paso** en el punto medio de cada
   celda compartida (2 pasos en lados de 2 celdas).
3. **`RoomContentSpawner`** puebla cada cuarto según su `tag` (resuelto en `WorldConfigSO.themes` →
   `ZoneThemeSO`): **barreras** (validadas para no sellar el cuarto), **props** de ambientación y
   **enemigos**, con densidad y **tier escalados por el nivel del jugador** (`DifficultyProfileSO`).

Convención: **piso = celda sin tile**; sólo los muros llevan `TileBase`.

## Notas de proyecto

- **Animación por color (greybox):** el `Animator` usa un parámetro int **`State`**
  (0 Idle, 1 Move, 2 MeleeAttack, 3 RangedAttack, 4 Death) y los clips animan
  `SpriteRenderer.m_Color`. El arte final solo reemplaza los clips/sprite.
- **Habilidades (LOTE 2):** implementadas y **desbloqueadas desde el inicio** para testear
  (`PlayerAbilities.startingAbilities`); además, el jefe **suelta un pickup** al morir.
- **Menú DEV:** botón `DEV` en el HUD para nivel, estadísticas base y bloqueo de habilidades.
- **Layers**: `Ground, Walls, Player, Enemies, PlayerAttack, EnemyAttack, Pickups, Hazards,
  BossTrigger` (índices 6–14) + tag `Player`.
- **Input Handling = Both**; el jugador usa el nuevo Input System con *fallback* legacy.
- **Cinemachine 3.1.7** para el seguimiento de cámara.

## Crear ScriptableObjects

Desde `Create > Madness Dreams > ...`: `Map Chunk`, `Block Bank`, `World Config`, `Zone Theme`,
`Prop Definition`, `Difficulty Profile`, `Enemy Archetype`, `Enemy Stats`, `Boss Stats`, `Boss Ability`.

## Capturar un cuarto a `MapChunkSO`

1. Pintá el cuarto en un `Tilemap` (un espacio abierto con **anillo de muro de 1 tile**).
2. Agregá hijos vacíos nombrados `EnemySpawn` (y `EnemySpawn1`, `EnemySpawn2`, …) en las posiciones de
   aparición.
3. `Tools > Madness Dreams > Map Chunk Capture` → elegí `Source Tilemap`, `Target MapChunkSO`,
   `Cell size` y `Tag` → **Capture**.

> Captura `size`, `tiles`, **`sizeInCells`** (derivado), **`tag`** y **`enemySpawns`** desde los hijos.
> Valida tamaño múltiplo de la celda, anillo de muro y presencia de spawn points.
>
> `Tools > Madness Dreams > Validate Worlds` verifica que cada `WorldConfigSO` tenga **exactamente 1
> cuarto de Spawn y 1 de Boss**.
