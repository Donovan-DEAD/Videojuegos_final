using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Utilidades estáticas para escribir y borrar tiles en un Tilemap (estampado de cuartos y excavado de aberturas).
    /// </summary>
    public static class TilemapStamper
    {
        /// <summary>Copia en bloque los tiles de un MapChunkSO en el Tilemap, sin rotación, desde la celda de origen.</summary>
        public static void Stamp(Tilemap tilemap, MapChunkSO chunk, Vector3Int origin, TilesetCatalogSO catalog = null)
        {
            if (tilemap == null || chunk == null)
            {
                return;
            }

            Vector2Int size = chunk.Size;
            TileBase[] flat = chunk.Tiles;
            if (flat == null || flat.Length == 0 || size.x <= 0 || size.y <= 0)
            {
                return;
            }

            if (catalog != null)
            {
                var resolved = new TileBase[flat.Length];
                for (int i = 0; i < flat.Length; i++)
                {
                    resolved[i] = catalog.ResolveTile(flat[i]);
                }

                flat = resolved;
            }

            BoundsInt area = new BoundsInt(origin.x, origin.y, origin.z, size.x, size.y, 1);
            tilemap.SetTilesBlock(area, flat);
        }

        /// <summary>
        /// Rellena un rectángulo de celdas con un tile (se usa para abrir los pasos: se reemplaza el
        /// muro por piso).
        /// </summary>
        public static void FillCells(Tilemap tilemap, Vector3Int min, Vector3Int size, TileBase tile)
        {
            if (tilemap == null || tile == null)
            {
                return;
            }

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    tilemap.SetTile(new Vector3Int(min.x + x, min.y + y, min.z), tile);
                }
            }
        }

        /// <summary>
        /// Estampa un cuarto dentro de su footprint exacto (tamaño en celdas × tamaño de celda) y rellena
        /// con muro las celdas del footprint que el chunk no cubre, de modo que un chunk más corto que su
        /// footprint no deje una costura abierta.
        /// </summary>
        public static void StampFootprint(
            Tilemap tilemap,
            MapChunkSO chunk,
            Vector3Int origin,
            Vector2Int footprint,
            TileBase wall,
            TilesetCatalogSO catalog = null)
        {
            Stamp(tilemap, chunk, origin, catalog);

            if (tilemap == null || footprint.x <= 0 || footprint.y <= 0)
            {
                return;
            }

            Vector2Int size = chunk != null ? chunk.Size : Vector2Int.zero;
            if (size.x >= footprint.x && size.y >= footprint.y)
            {
                return;
            }

            if (wall == null)
            {
                return;
            }

            for (int y = 0; y < footprint.y; y++)
            {
                for (int x = 0; x < footprint.x; x++)
                {
                    if (x < size.x && y < size.y)
                    {
                        continue;
                    }

                    tilemap.SetTile(new Vector3Int(origin.x + x, origin.y + y, origin.z), wall);
                }
            }
        }

        /// <summary>
        /// Rellena el borde exterior (1 tile de grosor) del área indicada con un tile. Se usa para sellar
        /// el perímetro del mapa completo (cierres de puertas hacia afuera y columnas/filas faltantes).
        /// </summary>
        public static void FillBorder(Tilemap tilemap, BoundsInt bounds, TileBase tile)
        {
            if (tilemap == null || tile == null || bounds.size.x <= 0 || bounds.size.y <= 0)
            {
                return;
            }

            int xMin = bounds.xMin;
            int xMax = bounds.xMax - 1;
            int yMin = bounds.yMin;
            int yMax = bounds.yMax - 1;
            int z = bounds.zMin;

            for (int x = xMin; x <= xMax; x++)
            {
                tilemap.SetTile(new Vector3Int(x, yMin, z), tile);
                tilemap.SetTile(new Vector3Int(x, yMax, z), tile);
            }

            for (int y = yMin + 1; y < yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(xMin, y, z), tile);
                tilemap.SetTile(new Vector3Int(xMax, y, z), tile);
            }
        }
    }
}
