using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Renderiza previews PNG de cuartos (tiles reales + grilla + spawns) para revisarlos con visión
    /// antes de cablearlos al juego.
    /// </summary>
    public static class RoomPreviewRenderer
    {
        private static readonly Dictionary<TileBase, Texture2D> tileCache = new Dictionary<TileBase, Texture2D>();

        /// <summary>Renderiza todos los MapChunkSO de una carpeta a PNGs individuales.</summary>
        public static int RenderFolder(string roomsFolder, string outputFolder, int scale = 8)
        {
            if (string.IsNullOrEmpty(roomsFolder) || !AssetDatabase.IsValidFolder(roomsFolder))
            {
                Debug.LogWarning("RoomPreviewRenderer: carpeta inválida: " + roomsFolder);
                return 0;
            }

            EnsureFolder(outputFolder);
            int count = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:MapChunkSO", new[] { roomsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(outputFolder)) continue;

                var room = AssetDatabase.LoadAssetAtPath<MapChunkSO>(path);
                if (room == null) continue;

                RenderRoom(room, outputFolder + "/" + room.name + ".png", scale);
                count++;
            }

            AssetDatabase.Refresh();
            return count;
        }

        /// <summary>Renderiza un cuarto a PNG: tiles, grilla de celdas, borde y spawns.</summary>
        public static void RenderRoom(MapChunkSO room, string outputPath, int scale = 8)
        {
            if (room == null || room.Tiles == null || room.Size.x <= 0 || room.Size.y <= 0)
            {
                return;
            }

            scale = Mathf.Clamp(scale, 1, 32);
            const int sourceCell = 16;
            int border = 8;
            int width = border * 2 + room.Size.x * scale;
            int height = border * 2 + room.Size.y * scale;

            Color background = new Color(0.06f, 0.06f, 0.08f, 1f);
            Texture2D big = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = background;
            big.SetPixels(pixels);

            for (int y = 0; y < room.Size.y; y++)
            {
                for (int x = 0; x < room.Size.x; x++)
                {
                    int index = y * room.Size.x + x;
                    TileBase tile = index < room.Tiles.Length ? room.Tiles[index] : null;
                    Texture2D copy = GetTileTexture(tile);
                    if (copy == null) continue;

                    int px = border + x * scale;
                    int py = border + y * scale;
                    Color[] src = copy.GetPixels();
                    Color[] block = new Color[scale * scale];

                    for (int by = 0; by < scale; by++)
                    {
                        int sy = Mathf.Clamp(by, 0, sourceCell - 1);
                        for (int bx = 0; bx < scale; bx++)
                        {
                            int sx = Mathf.Clamp(bx, 0, sourceCell - 1);
                            block[by * scale + bx] = src[sy * sourceCell + sx];
                        }
                    }

                    big.SetPixels(px, py, scale, scale, block);
                }
            }

            Color gridColor = new Color(1f, 1f, 1f, 0.07f);
            for (int x = 0; x <= room.Size.x; x += sourceCell)
            {
                DrawVerticalLine(big, border + x * scale, border, border + room.Size.y * scale, gridColor);
            }
            for (int y = 0; y <= room.Size.y; y += sourceCell)
            {
                DrawHorizontalLine(big, border + y * scale, border, border + room.Size.x * scale, gridColor);
            }

            Color borderColor = new Color(1f, 0.2f, 0.6f, 1f);
            DrawVerticalLine(big, border, border, border + room.Size.y * scale, borderColor);
            DrawVerticalLine(big, border + room.Size.x * scale, border, border + room.Size.y * scale, borderColor);
            DrawHorizontalLine(big, border, border, border + room.Size.x * scale, borderColor);
            DrawHorizontalLine(big, border + room.Size.y * scale, border, border + room.Size.x * scale, borderColor);

            Color floorSpawnColor = new Color(0.2f, 1f, 0.35f, 1f);
            Color waterSpawnColor = new Color(0.2f, 0.65f, 1f, 1f);
            EnemySpawnPoint[] spawns = room.EnemySpawns;
            if (spawns != null)
            {
                foreach (var spawn in spawns)
                {
                    int sx = border + spawn.localPosition.x * scale + scale / 2;
                    int sy = border + spawn.localPosition.y * scale + scale / 2;
                    Color spawnColor = spawn.terrain == TerrainType.Water ? waterSpawnColor : floorSpawnColor;
                    DrawBlock(big, sx - 2, sy - 2, 5, 5, spawnColor);
                }
            }

            big.Apply();
            EnsureFolder(Path.GetDirectoryName(outputPath).Replace('\\', '/'));
            File.WriteAllBytes(outputPath, big.EncodeToPNG());
            Object.DestroyImmediate(big);

            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        private static Texture2D GetTileTexture(TileBase tileBase)
        {
            if (tileBase == null) return null;

            Texture2D cached;
            if (tileCache.TryGetValue(tileBase, out cached)) return cached;

            var tile = tileBase as Tile;
            if (tile == null || tile.sprite == null) return null;

            Texture2D result = ReadSprite(tile.sprite);
            if (result == null) return null;

            if (result.width != 16 || result.height != 16)
            {
                Texture2D resized = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                Color[] src = result.GetPixels();
                Color[] dst = new Color[256];
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int sx = Mathf.Clamp(x * result.width / 16, 0, result.width - 1);
                        int sy = Mathf.Clamp(y * result.height / 16, 0, result.height - 1);
                        dst[y * 16 + x] = src[sy * result.width + sx];
                    }
                }
                resized.SetPixels(dst);
                resized.Apply();
                Object.DestroyImmediate(result);
                result = resized;
            }

            tileCache[tileBase] = result;
            return result;
        }

        private static Texture2D ReadSprite(Sprite sprite)
        {
            Texture2D tex = sprite.texture;
            Rect tr = sprite.textureRect;
            int w = Mathf.Max(1, Mathf.RoundToInt(tr.width));
            int h = Mathf.Max(1, Mathf.RoundToInt(tr.height));

            RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(tex, rt, new Vector2(w / tex.width, h / tex.height), new Vector2(tr.x / tex.width, tr.y / tex.height));
            RenderTexture.active = rt;

            Texture2D copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            copy.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return copy;
        }

        private static void DrawBlock(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    int tx = x + px;
                    int ty = y + py;
                    if (tx < 0 || ty < 0 || tx >= tex.width || ty >= tex.height) continue;
                    tex.SetPixel(tx, ty, color);
                }
            }
        }

        private static void DrawVerticalLine(Texture2D tex, int x, int y0, int y1, Color color)
        {
            for (int y = y0; y <= y1; y++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height) tex.SetPixel(x, y, color);
            }
        }

        private static void DrawHorizontalLine(Texture2D tex, int y, int x0, int x1, Color color)
        {
            for (int x = x0; x <= x1; x++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height) tex.SetPixel(x, y, color);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
