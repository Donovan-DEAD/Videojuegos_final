# Scripts/Generation — Mapa procedural

Construye el nivel a partir de `WorldConfigSO` + `BlockBankSO`.

## Flujo de `MapGenerator.Generate()`

1. **`RoomLayout.Build`** — reserva **Spawn** (centro) y **Boss** (el más lejano) y luego empaqueta cuartos
   de tamaño fijo por celdas; asigna `tag` por peso y calcula la **adyacencia** (cada celda vecina de
   cuartos distintos = una conexión). Grafo **totalmente conexo**.
2. **`TilemapStamper.Stamp`** — estampa cada arquetipo (sin rotación) en su celda de origen.
3. **Pasos** — por cada conexión se limpia el borde con `TilemapStamper.ClearCells` (un paso por celda
   compartida; 2 en lados de 2 celdas).
4. **`RoomContentSpawner.SpawnAll`** — puebla por tag: barreras → props → enemigos.
5. Dispara **`Generated`** (lo consumen `WorldLayout` y el HUD).

## Generación incremental (`GenerateRoutine`)

`RunManager` usa **`MapGenerator.GenerateRoutine(onProgress)`**: los mismos pasos, repartidos en
varios frames (estampado por lotes y un cuarto de contenido por frame) reportando `Progress` (0..1).
Así la pantalla de carga puede mostrar el avance real sin congelar el juego. `Generate()` sigue
existiendo para el `[ContextMenu]` y las herramientas de editor.

- **Escena activa:** `RunManager.Awake` activa la escena de gameplay antes de generar. La escena se
  monta de forma **aditiva** desde `02_Loading`, y los props/enemigos instanciados deben pertenecer a
  la escena de gameplay (si no, se destruyen al descargar la pantalla de carga).

## Archivos

| Archivo | Rol |
|---|---|
| `MapGenerator` | Orquesta el pipeline; expone `Rooms`, `Connections`, `StartRoom`, `BossRoom`, `StartRoomCenterCell`, `BossRoomCenterCell`, `Progress`, `GetTerrain` y `GenerateRoutine`/`Generate`. Dispara `Generated`. |
| `RoomLayout` | Reservas + empaquetado + adyacencia (estático, determinista por semilla). |
| `RoomPlacement` | Struct: `chunk`, `sizeInCells`, `originCell`, `tag`, `role`. |
| `RoomConnection` / `RoomSide` | Struct + enum: qué cuartos conecta, lado y celda de origen. |
| `RoomContentSpawner` | Barreras (con validación anti-sellado por flood-fill), props y enemigos desde el `ZoneThemeSO`; filtra spawns por `EnemyHabitat` y terreno. Expone `BeginSpawn`/`SpawnRoomAt`/`SpawnAll` y `RoomCount`. |
| `TilemapStamper` | `Stamp` (estampa tiles) y `FillCells` (talla pasos/rellena celdas). |
| `Spawner` | Instancia con jitter y chequeo de obstáculos (`Physics2D.OverlapCircle`). |
| `CellRole` | Enum `Empty / Start / Normal / Boss`. |

## Reglas de datos

- **Terreno data-driven:** `TilesetCatalogSO.GetTerrain(tile)` clasifica `Wall`, `Floor`, `Water` y capas
  extra (`TerrainType` flags); `MapGenerator.GetTerrain(cell/world)` lo expone. `null` = `Void` (fuera del mapa).
- Los pasos se tallan con `TilemapStamper.FillCells` usando un tile de piso; el piso puede estar pintado
  (no solo celdas vacías).
- **Hábitats:** `RoomContentSpawner` elige puntos compatibles con el `EnemyHabitat` del arquetipo
  (`Ground`, `Water`, `Flying`); `EnemySpawnPoint.terrain` puede fijar el tipo (spawns de agua).
- **Agua:** las variantes B/C del `RoomVariantGenerator` agregan pozos; las aperturas y el anillo se fuerzan
  a `Floor`/`Wall`; se validan los grafos piso, agua y piso+agua.
- Un cuarto es un **espacio abierto con anillo de muro de 1 tile** en el borde de su footprint.
- El `tag` del cuarto se resuelve contra `WorldConfigSO.Themes` (lista `tag → ZoneThemeSO`).
- La **dificultad** (densidad + tier de enemigos) sale de `DifficultyProfileSO` con `PlayerStats.Level`.

Ver `Assets/Scripts/Editor/README.md` para la herramienta que captura cuartos.
