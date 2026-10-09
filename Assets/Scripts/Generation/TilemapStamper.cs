using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    public static class TilemapStamper
    {
        public static void Stamp(Tilemap tilemap, MapChunkSO chunk, Vector3Int origin, int rotationSteps = 0)
        {
            if (tilemap == null || chunk == null)
            {
                return;
            }

            Vector2Int size = chunk.Size;
            TileBase[] flat = chunk.Tiles;
            if (flat == null || flat.Length == 0)
            {
                return;
            }

            int steps = ((rotationSteps % 4) + 4) % 4;
            Vector2Int rotatedSize = (steps % 2 == 0) ? size : new Vector2Int(size.y, size.x);
            TileBase[] rotated = steps == 0 ? flat : RotateTiles(flat, size, steps);

            BoundsInt area = new BoundsInt(origin.x, origin.y, origin.z, rotatedSize.x, rotatedSize.y, 1);
            tilemap.SetTilesBlock(area, rotated);
        }

        private static TileBase[] RotateTiles(TileBase[] flat, Vector2Int size, int steps)
        {
            int w = size.x;
            int h = size.y;
            int rw = (steps % 2 == 0) ? w : h;
            int rh = (steps % 2 == 0) ? h : w;
            TileBase[] result = new TileBase[rw * rh];

            for (int dy = 0; dy < rh; dy++)
            {
                for (int dx = 0; dx < rw; dx++)
                {
                    int sx, sy;
                    switch (steps)
                    {
                        case 1: sx = dy; sy = h - 1 - dx; break;
                        case 2: sx = w - 1 - dx; sy = h - 1 - dy; break;
                        case 3: sx = w - 1 - dy; sy = dx; break;
                        default: sx = dx; sy = dy; break;
                    }

                    result[dy * rw + dx] = flat[sy * w + sx];
                }
            }

            return result;
        }
    }
}
