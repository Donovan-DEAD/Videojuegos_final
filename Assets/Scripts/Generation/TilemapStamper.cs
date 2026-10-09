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
        public static void Stamp(Tilemap tilemap, MapChunkSO chunk, Vector3Int origin)
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

            BoundsInt area = new BoundsInt(origin.x, origin.y, origin.z, size.x, size.y, 1);
            tilemap.SetTilesBlock(area, flat);
        }

        /// <summary>Elimina los tiles de una región rectangular (los pone a null).</summary>
        public static void ClearCells(Tilemap tilemap, Vector3Int min, Vector3Int size)
        {
            if (tilemap == null)
            {
                return;
            }

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    tilemap.SetTile(new Vector3Int(min.x + x, min.y + y, min.z), null);
                }
            }
        }
    }
}
