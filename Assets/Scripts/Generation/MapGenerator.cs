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

        public WorldConfigSO Config => config;
        public Tilemap TargetTilemap => targetTilemap;

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

        /// <summary>Genera el mundo completo (limpia, dispone cuartos, estampa tiles, conecta y puebla contenido).</summary>
        [ContextMenu("Generate")]
        public void Generate()
        {
            if (config == null || config.Bank == null || targetTilemap == null)
            {
                Debug.LogError("MapGenerator: config, bank or targetTilemap is missing.", this);
                return;
            }

            targetTilemap.ClearAllTiles();
            Rooms.Clear();
            Connections.Clear();

            //System.Random rng = new System.Random(randomSeed >= 0 ? randomSeed : config.Seed);
            System.Random rng = new System.Random();
            RoomLayout.Build(config, rng, Rooms, Connections);

            StampRooms();
            //CarveConnections();

            StartChunk = StartRoom.originCell;
            BossOriginChunk = BossRoom.originCell;

            SpawnContent(rng);

            Generated?.Invoke();
        }

        private void StampRooms()
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                RoomPlacement room = Rooms[i];
                if (room.chunk == null)
                {
                    continue;
                }

                TilemapStamper.Stamp(targetTilemap, room.chunk, ChunkToCell(room.originCell));
            }
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
