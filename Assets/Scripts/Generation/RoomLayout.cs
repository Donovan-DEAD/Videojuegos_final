using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Calcula la disposición de los cuartos: reserva primero el cuarto Start (centro) y el Boss (más lejano),
    /// luego empaqueta cuartos de los tamaños fijos y calcula las adyacencias.
    /// </summary>
    public static class RoomLayout
    {
        /// <summary>Genera la lista de cuartos y sus conexiones a partir de la configuración y un RNG.</summary>
        public static void Build(WorldConfigSO config, System.Random rng, List<RoomPlacement> rooms, List<RoomConnection> connections)
        {
            rooms.Clear();
            connections.Clear();

            int width = Mathf.Max(1, config.Width);
            int height = Mathf.Max(1, config.Height);
            int cellSize = Mathf.Max(1, config.ChunkSize);
            BlockBankSO bank = config.Bank;

            int[,] index = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    index[x, y] = -1;
                }
            }

            Vector2Int startOrigin = PlaceStart(bank, cellSize, width, height, index, rooms);
            PlaceBoss(bank, cellSize, width, height, index, rooms, startOrigin);
            Pack(config, bank, cellSize, width, height, index, rooms, rng);
            BuildConnections(width, height, index, connections);
        }

        private static Vector2Int PlaceStart(BlockBankSO bank, int cellSize, int width, int height, int[,] index, List<RoomPlacement> rooms)
        {
            MapChunkSO archetype = bank != null ? bank.StartRoom : null;
            if (archetype == null && bank != null && bank.NormalChunkCount > 0)
            {
                archetype = bank.NormalChunks[0];
            }

            Vector2Int cells = ClampToGrid(archetype != null ? archetype.ResolveSizeInCells(cellSize) : Vector2Int.one, width, height);
            Vector2Int origin = new Vector2Int((width - cells.x) / 2, (height - cells.y) / 2);

            AddRoom(rooms, index, width, height, archetype, cells, origin, RoomTag.None, CellRole.Start);
            return origin;
        }

        private static void PlaceBoss(BlockBankSO bank, int cellSize, int width, int height, int[,] index, List<RoomPlacement> rooms, Vector2Int startOrigin)
        {
            MapChunkSO arena = bank != null ? bank.BossArena : null;
            if (arena != null)
            {
                Vector2Int cells = ClampToGrid(arena.ResolveSizeInCells(cellSize), width, height);
                if (TryFindFarthestOrigin(index, width, height, cells, startOrigin, out Vector2Int origin))
                {
                    AddRoom(rooms, index, width, height, arena, cells, origin, RoomTag.None, CellRole.Boss);
                    return;
                }
            }

            MapChunkSO fallback = null;
            Vector2Int fallbackCells = Vector2Int.one;
            Vector2Int fallbackOrigin = Vector2Int.zero;
            int bestArea = 0;

            if (bank != null && bank.NormalChunks != null)
            {
                for (int i = 0; i < bank.NormalChunks.Count; i++)
                {
                    MapChunkSO chunk = bank.NormalChunks[i];
                    if (chunk == null)
                    {
                        continue;
                    }

                    Vector2Int cells = ClampToGrid(chunk.ResolveSizeInCells(cellSize), width, height);
                    int area = cells.x * cells.y;
                    if (area <= bestArea)
                    {
                        continue;
                    }

                    if (TryFindFarthestOrigin(index, width, height, cells, startOrigin, out Vector2Int origin))
                    {
                        fallback = chunk;
                        fallbackCells = cells;
                        fallbackOrigin = origin;
                        bestArea = area;
                    }
                }
            }

            if (fallback != null)
            {
                AddRoom(rooms, index, width, height, fallback, fallbackCells, fallbackOrigin, RoomTag.None, CellRole.Boss);
            }
            else
            {
                Debug.LogWarning("RoomLayout: no se pudo reservar un cuarto de Boss (rejilla muy chica).");
            }
        }

        private static void Pack(WorldConfigSO config, BlockBankSO bank, int cellSize, int width, int height, int[,] index, List<RoomPlacement> rooms, System.Random rng)
        {
            List<RoomSizeClass> available = AvailableClasses(bank, cellSize);
            if (available.Count == 0)
            {
                available.Add(RoomSizeClass.Small);
            }

            List<Vector2Int> order = new List<Vector2Int>(width * height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    order.Add(new Vector2Int(x, y));
                }
            }

            Shuffle(order, rng);

            for (int i = 0; i < order.Count; i++)
            {
                Vector2Int cell = order[i];
                if (index[cell.x, cell.y] >= 0)
                {
                    continue;
                }

                RoomSizeClass sizeClass = WeightedClass(config, available, rng);
                List<Vector2Int> candidates = new List<Vector2Int>(RoomSizes.Candidates(sizeClass));
                Shuffle(candidates, rng);

                bool placed = false;
                for (int c = 0; c < candidates.Count; c++)
                {
                    Vector2Int cells = ClampToGrid(candidates[c], width, height);
                    if (!Fits(index, width, height, cell, cells))
                    {
                        continue;
                    }

                    RoomTag tag = PickTag(config, rng);
                    MapChunkSO archetype = PickArchetype(bank, tag, cells, cellSize, rng);
                    if (archetype == null)
                    {
                        continue;
                    }

                    AddRoom(rooms, index, width, height, archetype, cells, cell, tag, CellRole.Normal);
                    placed = true;
                    break;
                }

                if (placed)
                {
                    continue;
                }

                Vector2Int single = Vector2Int.one;
                if (Fits(index, width, height, cell, single))
                {
                    RoomTag tag = PickTag(config, rng);
                    MapChunkSO archetype = PickArchetype(bank, tag, single, cellSize, rng);
                    AddRoom(rooms, index, width, height, archetype, single, cell, tag, CellRole.Normal);
                }
            }
        }

        private static bool AddRoom(List<RoomPlacement> rooms, int[,] index, int width, int height, MapChunkSO chunk, Vector2Int cells, Vector2Int origin, RoomTag tag, CellRole role)
        {
            if (!Fits(index, width, height, origin, cells))
            {
                return false;
            }

            int id = rooms.Count;
            rooms.Add(new RoomPlacement
            {
                chunk = chunk,
                sizeInCells = cells,
                originCell = origin,
                tag = tag,
                role = role
            });

            for (int y = 0; y < cells.y; y++)
            {
                for (int x = 0; x < cells.x; x++)
                {
                    index[origin.x + x, origin.y + y] = id;
                }
            }

            return true;
        }

        private static void BuildConnections(int width, int height, int[,] index, List<RoomConnection> connections)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int a = index[x, y];
                    if (a < 0)
                    {
                        continue;
                    }

                    if (x + 1 < width)
                    {
                        int b = index[x + 1, y];
                        if (b >= 0 && b != a)
                        {
                            connections.Add(new RoomConnection { roomA = a, roomB = b, side = RoomSide.Right, cell = new Vector2Int(x, y) });
                        }
                    }

                    if (y + 1 < height)
                    {
                        int b = index[x, y + 1];
                        if (b >= 0 && b != a)
                        {
                            connections.Add(new RoomConnection { roomA = a, roomB = b, side = RoomSide.Up, cell = new Vector2Int(x, y) });
                        }
                    }
                }
            }
        }

        private static List<RoomSizeClass> AvailableClasses(BlockBankSO bank, int cellSize)
        {
            List<RoomSizeClass> result = new List<RoomSizeClass>();
            if (bank == null || bank.NormalChunks == null)
            {
                return result;
            }

            for (int c = 0; c < 4; c++)
            {
                RoomSizeClass sizeClass = (RoomSizeClass)c;
                Vector2Int[] candidates = RoomSizes.Candidates(sizeClass);

                bool found = false;
                for (int i = 0; i < candidates.Length && !found; i++)
                {
                    for (int j = 0; j < bank.NormalChunks.Count; j++)
                    {
                        MapChunkSO chunk = bank.NormalChunks[j];
                        if (chunk != null && chunk.ResolveSizeInCells(cellSize) == candidates[i])
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                {
                    result.Add(sizeClass);
                }
            }

            return result;
        }

        private static RoomSizeClass WeightedClass(WorldConfigSO config, List<RoomSizeClass> available, System.Random rng)
        {
            int total = 0;
            for (int i = 0; i < available.Count; i++)
            {
                total += Mathf.Max(1, config.WeightFor(available[i]));
            }

            if (total <= 0)
            {
                return available[0];
            }

            int pick = rng.Next(total);
            for (int i = 0; i < available.Count; i++)
            {
                pick -= Mathf.Max(1, config.WeightFor(available[i]));
                if (pick < 0)
                {
                    return available[i];
                }
            }

            return available[available.Count - 1];
        }

        private static RoomTag PickTag(WorldConfigSO config, System.Random rng)
        {
            if (config.Themes == null || config.Themes.Count == 0)
            {
                return RoomTag.None;
            }

            return config.Themes[rng.Next(config.Themes.Count)].tag;
        }

        private static MapChunkSO PickArchetype(BlockBankSO bank, RoomTag tag, Vector2Int cells, int cellSize, System.Random rng)
        {
            if (bank == null || bank.NormalChunks == null || bank.NormalChunks.Count == 0)
            {
                return null;
            }

            List<MapChunkSO> bySizeAndTag = new List<MapChunkSO>();
            List<MapChunkSO> bySize = new List<MapChunkSO>();

            for (int i = 0; i < bank.NormalChunks.Count; i++)
            {
                MapChunkSO chunk = bank.NormalChunks[i];
                if (chunk == null || chunk.ResolveSizeInCells(cellSize) != cells)
                {
                    continue;
                }

                bySize.Add(chunk);
                if (chunk.Tag == tag)
                {
                    bySizeAndTag.Add(chunk);
                }
            }

            if (bySizeAndTag.Count > 0)
            {
                return bySizeAndTag[rng.Next(bySizeAndTag.Count)];
            }

            if (bySize.Count > 0)
            {
                return bySize[rng.Next(bySize.Count)];
            }

            return null;
        }

        private static bool Fits(int[,] index, int width, int height, Vector2Int origin, Vector2Int cells)
        {
            if (origin.x < 0 || origin.y < 0 || origin.x + cells.x > width || origin.y + cells.y > height)
            {
                return false;
            }

            for (int y = 0; y < cells.y; y++)
            {
                for (int x = 0; x < cells.x; x++)
                {
                    if (index[origin.x + x, origin.y + y] >= 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool TryFindFarthestOrigin(int[,] index, int width, int height, Vector2Int cells, Vector2Int from, out Vector2Int result)
        {
            result = Vector2Int.zero;
            int bestDistance = -1;
            bool found = false;

            for (int y = 0; y <= height - cells.y; y++)
            {
                for (int x = 0; x <= width - cells.x; x++)
                {
                    Vector2Int origin = new Vector2Int(x, y);
                    if (!Fits(index, width, height, origin, cells))
                    {
                        continue;
                    }

                    int distance = Mathf.Abs(origin.x - from.x) + Mathf.Abs(origin.y - from.y);
                    if (distance > bestDistance)
                    {
                        bestDistance = distance;
                        result = origin;
                        found = true;
                    }
                }
            }

            return found;
        }

        private static Vector2Int ClampToGrid(Vector2Int cells, int width, int height)
        {
            return new Vector2Int(Mathf.Clamp(cells.x, 1, width), Mathf.Clamp(cells.y, 1, height));
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
