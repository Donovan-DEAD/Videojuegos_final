# Scripts/Editor — Herramientas

## `MapChunkCaptureEditor` — `Tools > Madness Dreams > Map Chunk Capture`

Convierte un `Tilemap` de la escena en un `MapChunkSO`.

1. Armá el cuarto en un `Tilemap` (un solo espacio abierto con **anillo de muro de 1 tile**).
2. Agregá **hijos vacíos** al tilemap (o hermanos) nombrados con el prefijo de spawn, p. ej. `EnemySpawn`
   (opcionalmente `EnemySpawn1`, `EnemySpawn2`, …) en las posiciones de aparición. Para spawns de agua,
   usá `EnemySpawn_Water`.
3. En la ventana elegí `Source Tilemap`, `Target MapChunkSO`, `Tileset Catalog`, `Cell size` (por defecto 16)
   y `Tag` → **Capture**. **Preview** renderiza el cuarto a PNG.
4. Qué captura: `size`, `tiles`, **`sizeInCells`**, **`tag`**, **`enemySpawns`** (posición local, peso 1 y
   `terrain` según el nombre) y valida el terreno.

Validaciones (warnings en consola):
- tamaño múltiplo de la celda;
- anillo de muro completo salvo las aperturas 6..9, que deben ser `Floor`;
- conectividad de `Floor`, `Water` y `Floor|Water`;
- spawns dentro del cuarto y sobre el terreno que declaran.

## `TerrainTools` — `Tools > Madness Dreams > Terrain`

- **Build Sea Water**: genera `Water_Sea_0..2` (PNG + `Tile` + capa `Water` del catálogo + perfil).
- **Migrate Enemy Habitats**: asigna Ground/Water/Flying y el comportamiento (escondite/temporizador) a los 8 `EnemyStatsSO`.
- **Regenerate Sea Rooms**: regenera las variantes del banco del mar y sus previews.
- **Render Sea Previews** / **Validate Terrain**: previews y validación global de cuartos.

## `RoomVariantGenerator` / `RoomVariantWindow` / `RoomPreviewRenderer`

`RoomVariantGenerator` construye variantes por tamaño con aperturas 6..9, interior aleatorio, pozos de
agua (variantes B/C) y conectividad triple; `RoomVariantWindow` lo ejecuta y `RoomPreviewRenderer` genera
PNGs para revisión visual.

## `TilesetTools` — `Tools > Madness Dreams > Tileset Builder`

Además de Piso/Muro/Decor/Barrera, el pincel incluye **Agua**; `BuildCatalog` crea los tiles de agua
(`colliderType = None`) y los guarda en la capa `Water` del catálogo.

> Nota técnica: `Tilemap.cellBounds` incluye padding desde el origen; la herramienta calcula los límites
> reales de los tiles pintados.

## `Tools > Madness Dreams > Validate Worlds`

Corre `RoomLayout` para cada `WorldConfigSO` y verifica que exista **exactamente 1 cuarto de Spawn y 1 de Boss**
(reporta cuántos cuartos generó cada mundo).
