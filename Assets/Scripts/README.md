# Assets/Scripts — Arquitectura

Código de Madness Dreams. Namespace único: **`MadnessDreams`**. Organizado por **capas**:
los datos (SOs) describen, la generación construye, las entidades juegan, la UI presenta.

## Capas

| Carpeta | Responsabilidad | README |
|---|---|---|
| `Data/` | ScriptableObjects: configuración y plantillas | [Data/README.md](./Data/README.md) |
| `Generation/` | Mapa procedural por cuartos y poblamiento | [Generation/README.md](./Generation/README.md) |
| `Enemies/` | IA, movimiento, ataque y visión de esbirros | [Enemies/README.md](./Enemies/README.md) |
| `Boss/` | Jefes, arena y avance de mundo | [Boss/README.md](./Boss/README.md) |
| `Player/` | Movimiento, combate, habilidades y stats | [Player/README.md](./Player/README.md) |
| `Pickups/` | Recogibles (habilidades) | [Pickups/README.md](./Pickups/README.md) |
| `Gameplay/` | Layout del nivel, portal de avance | [Gameplay/README.md](./Gameplay/README.md) |
| `HUD/` | Interfaz de gameplay | [HUD/README.md](./HUD/README.md) |
| `UI/` | Flujo de pantallas y ajustes | [UI/README.md](./UI/README.md) |
| `Core/` | Vida, proyectil, run manager y enums | [Core/README.md](./Core/README.md) |
| `Editor/` | Herramientas de editor | [Editor/README.md](./Editor/README.md) |

## Flujo de arranque

`01_MainMenu` → `GameFlowManager.StartRun()` → `02_Loading` → `03_Gameplay`
→ `RunManager.GenerateWorld(indice)` → `MapGenerator.Generate()` → contenido →
al morir el jefe aparece un **Portal** → `RunManager.RequestAdvance()` → siguiente mundo.
Al morir el jugador: `PlayerDeathHandler` → `GameFlowManager.NotifyPlayerDied()` → `04_Defeat`.

## Generación del mapa (resumen)

1. **`RoomLayout`** empaqueta cuartos de **tamaño fijo por celdas** (1×1, 2×1/1×2, 2×2, 3×3) sobre una
   rejilla grande, **reservando primero** el cuarto de **Spawn** (centro) y el de **Boss** (el más lejano).
   El grafo resultante es **totalmente conexo**.
2. **`MapGenerator`** estampa los arquetipos (sin rotación) y **talla un paso** en el punto medio de cada
   celda compartida (2 pasos en lados de 2 celdas).
3. **`RoomContentSpawner`** puebla cada cuarto según su **`tag`** (`ZoneThemeSO`): **barreras** (validadas
   para no sellar el cuarto), **props** de ambientación y **enemigos**, con densidad y tier escalados por
   el **nivel del jugador** (`DifficultyProfileSO`).
4. **Terreno y hábitats:** `TilesetCatalogSO.GetTerrain` clasifica `Floor/Wall/Water`; `MapGenerator.GetTerrain`
   lo expone. `RoomContentSpawner` filtra por `EnemyHabitat` y `EnemyMovement` restringe el paso; los
   voladores atraviesan `Walls` con `Collider2D.excludeLayers`.

## Convenciones

- **Sin arte en el código:** los cambios de estado se representan por **color** mediante un `Animator`
  con parámetro int `State` (0 Idle, 1 Move, 2 MeleeAttack, 3 RangedAttack, 4 Death).
- **Eventos** (`System.Action`) para desacoplar: `MapGenerator.Generated`, `Health.HealthChanged`/`Died`,
  `BossArenaController.BossSpawned`/`BossDefeated`, `BossManager.BossDefeated`.
- **ScriptableObjects** para todo dato ajustable (nada hardcodeado que deba balancearse).
- **Terreno data-driven:** `TerrainType` (flags) + `TilesetCatalogSO.terrainLayers`; agregar un tipo
  (lava/pasto) no toca el tilemap ni el pipeline.
- **Comportamiento por autómata:** `EnemyAI` agrega `Hide`; `EnemyStatsSO` configura escondite
  (`hidesPeriodically`, `hideTerrain`, intervalos) y `maxTimeOutOfWater` (cocodrilo); los voladores
  se esconden en muros.
- **Configuración por Inspector** (`[SerializeField] private`) con propiedades públicas de solo lectura.
