# Scripts/Editor — Herramientas

## `MapChunkCaptureEditor` — `Tools > Madness Dreams > Map Chunk Capture`

Convierte un `Tilemap` de la escena en un `MapChunkSO`.

1. Armá el cuarto en un `Tilemap` (un solo espacio abierto con **anillo de muro de 1 tile**).
2. Agregá **hijos vacíos** al tilemap (o hermanos) nombrados con el prefijo de spawn, p. ej. `EnemySpawn`
   (opcionalmente `EnemySpawn1`, `EnemySpawn2`, …) en las posiciones de aparición.
3. En la ventana elegí `Source Tilemap`, `Target MapChunkSO`, `Cell size` (por defecto 16) y `Tag` → **Capture**.

Qué captura: `size`, `tiles`, **`sizeInCells`** (derivado de tamaño/celda), **`tag`** y **`enemySpawns`**
(posiciones locales en tiles, peso 1).

Validaciones (warnings en consola):
- tamaño múltiplo de la celda;
- el borde debe tener **anillo de muro** completo;
- si no encuentra spawn points con el prefijo indicado.

> Nota técnica: `Tilemap.cellBounds` incluye padding desde el origen; la herramienta calcula los límites
> reales de los tiles pintados.

## `Tools > Madness Dreams > Validate Worlds`

Corre `RoomLayout` para cada `WorldConfigSO` y verifica que exista **exactamente 1 cuarto de Spawn y 1 de Boss**
(reporta cuántos cuartos generó cada mundo).
