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

## Archivos

| Archivo | Rol |
|---|---|
| `MapGenerator` | Orquesta el pipeline; expone `Rooms`, `Connections`, `StartRoom`, `BossRoom`, `StartRoomCenterCell`, `BossRoomCenterCell`, `Generated`. |
| `RoomLayout` | Reservas + empaquetado + adyacencia (estático, determinista por semilla). |
| `RoomPlacement` | Struct: `chunk`, `sizeInCells`, `originCell`, `tag`, `role`. |
| `RoomConnection` / `RoomSide` | Struct + enum: qué cuartos conecta, lado y celda de origen. |
| `RoomContentSpawner` | Barreras (con validación anti-sellado por flood-fill), props y enemigos desde el `ZoneThemeSO`. |
| `TilemapStamper` | `Stamp` (estampa tiles) y `ClearCells` (talla pasos). |
| `Spawner` | Instancia con jitter y chequeo de obstáculos (`Physics2D.OverlapCircle`). |
| `CellRole` | Enum `Empty / Start / Normal / Boss`. |

## Reglas de datos

- **Piso = celda sin tile**; sólo los muros llevan `TileBase`. Así `caminable == sin tile` y tallar un paso
  es limpiar tiles.
- Un cuarto es un **espacio abierto con anillo de muro de 1 tile** en el borde de su footprint.
- El `tag` del cuarto se resuelve contra `WorldConfigSO.Themes` (lista `tag → ZoneThemeSO`).
- La **dificultad** (densidad + tier de enemigos) sale de `DifficultyProfileSO` con `PlayerStats.Level`.

Ver `Assets/Scripts/Editor/README.md` para la herramienta que captura cuartos.
