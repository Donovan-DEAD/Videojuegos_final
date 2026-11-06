using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Construye un mundo de cuartos conectados: delega la disposición en RoomLayout, estampa los tiles
    /// (sin rotación), excava una abertura por cada borde compartido y cede el contenido a RoomContentSpawner.
    /// Expone Rooms, Connections, StartRoom, BossRoom, StartRoomCenterCell, BossRoomCenterCell y el evento Generated.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WorldConfigSO config;
        [SerializeField] private Tilemap targetTilemap;
        [SerializeField] private int randomSeed = -1;

        [Header("Optional: content spawning")]
        [SerializeField] private RoomContentSpawner contentSpawner;

        [Header("Boundary")]
        [Tooltip("Sella con muro el perímetro del mapa para que el jugador no pueda salir por las puertas que miran hacia afuera.")]
        [SerializeField] private bool sealPerimeter = true;

        public WorldConfigSO Config => config;
        public Tilemap TargetTilemap => targetTilemap;

        /// <summary>Progreso de la última generación (0..1). Lo consume la pantalla de carga.</summary>
        public float Progress { get; private set; }

        /// <summary>Catálogo de tiles del mundo (vive en el <see cref="WorldConfigSO"/>): distingue piso de muro y aporta el tile para abrir los pasos.</summary>
        public TilesetCatalogSO Tileset => config != null ? config.Tileset : null;

        /// <summary>True si la celda contiene un tile de muro del catálogo (es decir, no es transitable).</summary>
        public bool IsWall(Vector3Int cell)
        {
            TilesetCatalogSO catalog = Tileset;
            if (targetTilemap == null || catalog == null)
            {
                return false;
            }

            return catalog.IsWall(targetTilemap.GetTile(cell));
        }

        /// <summary>Clasifica el terreno de una celda (Void si no hay tile).</summary>
        public TerrainType GetTerrain(Vector3Int cell)
        {
            TilesetCatalogSO catalog = Tileset;
            if (targetTilemap == null || catalog == null)
            {
                return TerrainType.Void;
            }

            return catalog.GetTerrain(targetTilemap.GetTile(cell));
        }

        /// <summary>Clasifica el terreno en una posición de mundo.</summary>
        public TerrainType GetTerrain(Vector3 world)
        {
            if (targetTilemap == null)
            {
                return TerrainType.Void;
            }

            return GetTerrain(targetTilemap.WorldToCell(world));
        }

        /// <summary>Cuartos colocados en la última generación.</summary>
        public List<RoomPlacement> Rooms { get; } = new List<RoomPlacement>();
        /// <summary>Conexiones (aberturas) entre cuartos adyacentes.</summary>
        public List<RoomConnection> Connections { get; } = new List<RoomConnection>();

        /// <summary>Celda de origen del cuarto inicial tras generar.</summary>
        public Vector2Int StartChunk { get; private set; }
        /// <summary>Celda de origen del cuarto de jefe tras generar.</summary>
        public Vector2Int BossOriginChunk { get; private set; }

        /// <summary>Esquina del cuarto inicial en coordenadas de celda de tilemap.</summary>
        public Vector3Int StartCell => ChunkToCell(StartChunk);
        /// <summary>Esquina del cuarto de jefe en coordenadas de celda de tilemap.</summary>
        public Vector3Int BossOriginCell => ChunkToCell(BossOriginChunk);

        /// <summary>Primer cuarto con rol Start.</summary>
        public RoomPlacement StartRoom => FindRoom(CellRole.Start);
        /// <summary>Primer cuarto con rol Boss.</summary>
        public RoomPlacement BossRoom => FindRoom(CellRole.Boss);

        /// <summary>Centro del cuarto inicial en celdas de tilemap.</summary>
        public Vector3Int StartRoomCenterCell => RoomCenterCell(StartRoom);
        /// <summary>Centro del cuarto de jefe en celdas de tilemap.</summary>
        public Vector3Int BossRoomCenterCell => RoomCenterCell(BossRoom);

        /// <summary>Se dispara al finalizar una generación completa.</summary>
        public event System.Action Generated;

        /// <summary>Permite inyectar la configuración del mundo en tiempo de ejecución.</summary>
        public void Configure(WorldConfigSO worldConfig)
        {
            config = worldConfig;
        }

        /// <summary>
        /// Fija la semilla del próximo <see cref="Generate"/> (una por run). Con -1 se usa la del
        /// <see cref="WorldConfigSO"/>, de modo que el mapa se repite.
        /// </summary>
        public void SetSeed(int seed)
        {
            randomSeed = seed;
        }

        /// <summary>Genera el mundo completo de forma sincrónica (editor y pruebas).</summary>
        [ContextMenu("Generate")]
        public void Generate()
        {
            if (!Validate())
            {
                return;
            }

            System.Random rng = CreateRng();
            BeginGenerate(rng);
            StampRooms();
            SealPerimeter();
            SetRoomAnchors();
            SpawnContent(rng);
            CompleteGenerate(null);
        }

        /// <summary>
        /// Genera el mundo repartiendo el trabajo en varios frames y reportando el progreso (0..1).
        /// La pantalla de carga lo consume para no congelar el juego.
        /// </summary>
        public IEnumerator GenerateRoutine(System.Action<float> onProgress)
        {
            if (!Validate())
            {
                yield break;
            }

            System.Random rng = CreateRng();
            BeginGenerate(rng);
            Report(0.05f, onProgress);
            yield return null;

            int roomCount = Rooms.Count;
            for (int i = 0; i < roomCount; i++)
            {
                StampRoom(Rooms[i]);
                if ((i + 1) % 4 == 0 || i == roomCount - 1)
                {
                    Report(Mathf.Lerp(0.05f, 0.45f, (float)(i + 1) / Mathf.Max(1, roomCount)), onProgress);
                    yield return null;
                }
            }

            SealPerimeter();
            Report(0.5f, onProgress);
            yield return null;

            SetRoomAnchors();

            if (contentSpawner != null)
            {
                contentSpawner.Configure(config);
                contentSpawner.BeginSpawn(rng);
                int spawnRooms = contentSpawner.RoomCount;
                for (int i = 0; i < spawnRooms; i++)
                {
                    contentSpawner.SpawnRoomAt(i, rng);
                    if ((i + 1) % 4 == 0 || i == spawnRooms - 1)
                    {
                        Report(Mathf.Lerp(0.5f, 0.95f, (float)(i + 1) / Mathf.Max(1, spawnRooms)), onProgress);
                        yield return null;
                    }
                }
            }

            CompleteGenerate(onProgress);
        }

        private bool Validate()
        {
            if (config == null || config.Bank == null || targetTilemap == null)
            {
                Debug.LogError("MapGenerator: config, bank or targetTilemap is missing.", this);
                return false;
            }

            return true;
        }

        private System.Random CreateRng()
        {
            return new System.Random(randomSeed >= 0 ? randomSeed : config.Seed);
        }

        private void BeginGenerate(System.Random rng)
        {
            targetTilemap.ClearAllTiles();
            Rooms.Clear();
            Connections.Clear();
            Progress = 0f;
            RoomLayout.Build(config, rng, Rooms, Connections);
        }

        private void SetRoomAnchors()
        {
            StartChunk = StartRoom.originCell;
            BossOriginChunk = BossRoom.originCell;
        }

        private void CompleteGenerate(System.Action<float> onProgress)
        {
            Progress = 1f;
            onProgress?.Invoke(Progress);
            Generated?.Invoke();
        }

        private void Report(float value, System.Action<float> onProgress)
        {
            Progress = Mathf.Clamp01(value);
            onProgress?.Invoke(Progress);
        }

        private void StampRooms()
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                StampRoom(Rooms[i]);
            }
        }

        private void StampRoom(RoomPlacement room)
        {
            if (room.chunk == null)
            {
                return;
            }

            TilesetCatalogSO catalog = Tileset;
            TileBase wall = catalog != null && catalog.WallTiles.Count > 0 ? catalog.WallTiles[0] : null;
            int cellSize = config != null ? Mathf.Max(1, config.ChunkSize) : 0;

            Vector2Int footprint = new Vector2Int(
                room.sizeInCells.x * cellSize,
                room.sizeInCells.y * cellSize);

            if (cellSize > 0 && (room.chunk.Size.x != footprint.x || room.chunk.Size.y != footprint.y))
            {
                Debug.LogError($"MapGenerator: chunk '{room.chunk.name}' size {room.chunk.Size} != footprint {footprint}; padding with walls.", room.chunk);
            }

            TilemapStamper.StampFootprint(targetTilemap, room.chunk, ChunkToCell(room.originCell), footprint, wall);
        }

        /// <summary>
        /// Sella con muro el borde exterior del área ocupada por los cuartos. Cierra las puertas que dan
        /// hacia afuera y las columnas/filas que un chunk corto deje vacías en el límite del mapa.
        /// </summary>
        private void SealPerimeter()
        {
            if (!sealPerimeter || targetTilemap == null || config == null || Rooms.Count == 0)
            {
                return;
            }

            TilesetCatalogSO catalog = Tileset;
            if (catalog == null || catalog.WallTiles == null || catalog.WallTiles.Count == 0)
            {
                return;
            }

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            for (int i = 0; i < Rooms.Count; i++)
            {
                RoomPlacement room = Rooms[i];
                minX = Mathf.Min(minX, room.originCell.x);
                minY = Mathf.Min(minY, room.originCell.y);
                maxX = Mathf.Max(maxX, room.originCell.x + room.sizeInCells.x);
                maxY = Mathf.Max(maxY, room.originCell.y + room.sizeInCells.y);
            }

            Vector3Int min = ChunkToCell(new Vector2Int(minX, minY));
            Vector3Int max = ChunkToCell(new Vector2Int(maxX, maxY));
            BoundsInt bounds = new BoundsInt(min.x, min.y, 0, max.x - min.x, max.y - min.y, 1);

            TilemapStamper.FillBorder(targetTilemap, bounds, catalog.WallTiles[0]);
        }

        private void CarveConnections()
        {
            int cellSize = Mathf.Max(1, config.ChunkSize);
            int opening = Mathf.Clamp(config.OpeningWidth, 1, Mathf.Max(1, cellSize - 2));
            int offset = (cellSize - opening) / 2;
            TileBase floorTile = Tileset != null && Tileset.FloorTiles.Count > 0 ? Tileset.FloorTiles[0] : null;

            for (int i = 0; i < Connections.Count; i++)
            {
                RoomConnection connection = Connections[i];
                Vector3Int origin = ChunkToCell(connection.cell);

                if (connection.side == RoomSide.Right)
                {
                    Vector3Int min = new Vector3Int(origin.x + cellSize - 1, origin.y + offset, origin.z);
                    TilemapStamper.FillCells(targetTilemap, min, new Vector3Int(2, opening, 1), floorTile);
                }
                else
                {
                    Vector3Int min = new Vector3Int(origin.x + offset, origin.y + cellSize - 1, origin.z);
                    TilemapStamper.FillCells(targetTilemap, min, new Vector3Int(opening, 2, 1), floorTile);
                }
            }
        }

        private void SpawnContent(System.Random rng)
        {
            if (contentSpawner == null)
            {
                return;
            }

            contentSpawner.Configure(config);
            contentSpawner.SpawnAll(rng);
        }

        /// <summary>Convierte una celda de rejilla (chunk) a la esquina en coordenadas de tilemap.</summary>
        public Vector3Int ChunkToCell(Vector2Int chunk)
        {
            return new Vector3Int(chunk.x * config.ChunkSize, chunk.y * config.ChunkSize, 0);
        }

        /// <summary>Devuelve el centro de un chunk en coordenadas de tilemap.</summary>
        public Vector3Int ChunkCenterCell(Vector2Int chunk)
        {
            Vector3Int origin = ChunkToCell(chunk);
            int half = config != null ? Mathf.Max(0, config.ChunkSize / 2) : 0;
            return new Vector3Int(origin.x + half, origin.y + half, origin.z);
        }

        /// <summary>Devuelve el centro (en celdas de tilemap) de un cuarto colocado.</summary>
        public Vector3Int RoomCenterCell(RoomPlacement room)
        {
            Vector3Int origin = ChunkToCell(room.originCell);
            int size = config != null ? config.ChunkSize : 0;
            return new Vector3Int(
                origin.x + room.sizeInCells.x * size / 2,
                origin.y + room.sizeInCells.y * size / 2,
                origin.z);
        }

        private RoomPlacement FindRoom(CellRole role)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i].role == role)
                {
                    return Rooms[i];
                }
            }

            return default;
        }
    }
}
