# Scripts/Data — ScriptableObjects

Configuración y plantillas del juego. Todo lo ajustable vive acá (nada hardcodeado que deba balancearse).

| Asset | Para qué |
|---|---|
| `WorldConfigSO` | Config de un mundo: semilla, rejilla, banco de cuartos, pesos de tamaño, ancho de paso, temas por tag, perfil de dificultad y jefe. |
| `BlockBankSO` | Banco de arquetipos de cuarto (`normalChunks`), el cuarto de **Spawn** (`startRoom`) y la **arena del jefe** (`bossArena`). |
| `MapChunkSO` | Un cuarto: `sizeInCells`, `tag`, `role`, `tiles`, `enemySpawns` (con peso y `terrain`) y `propRegions`. |
| `TilesetCatalogSO` | Tiles de piso/muro, capas de terreno adicionales (`Water`, …) y sprites de props; `GetTerrain(tile)` clasifica la celda. |
| `TilePainterProfileSO` | Roles visuales de piso/muro/agua para pintar cuartos sin elegir tiles al azar. |
| `RoomTag` | Enum de temática del cuarto (`Forest`, `Sea`, `Cave`, `Ruins`, …). |
| `RoomSizeClass` / `RoomSizes` | Clases de tamaño (1×1, 2×1/1×2, 2×2, 3×3) y sus tamaños candidatos. |
| `ZoneThemeSO` | **Banco por tag**: enemigos, props y barreras con pesos + densidades base. |
| `PropDefinitionSO` | Un prop de ambientación: prefab, `kind` (`Decor`/`Barrier`), footprint, peso y `allowWater`. |
| `DifficultyProfileSO` | Curva de dificultad: por nivel del jugador devuelve `maxTier`, multiplicador de densidad y rango de enemigos; más topes por tamaño de cuarto. |
| `EnemyArchetypeSO` | Vincula un prefab de enemigo con sus `EnemyStatsSO`, su `tier` y su peso. |
| `EnemyStatsSO` | Plantilla de propiedades de un enemigo: movimiento, vida, ataque, detección, `habitat`, `passThroughWalls`, `visionThroughWalls` y comportamiento (`hidesPeriodically`, `hideTerrain`, `hideIntervalMin/Max`, `hideDuration`, `maxTimeOutOfWater`). |
| `BossStatsSO` | Como `EnemyStatsSO` + umbrales de fases. |
| `BossAbilitySO` | Una habilidad del jugador: tipo, tiempos, daño, proyectil, slow, dash e ícono. |

**Menú:** `Create > Madness Dreams > …`

Ver también: [../Generation/README.md](../Generation/README.md) para cómo se consumen estos datos.
