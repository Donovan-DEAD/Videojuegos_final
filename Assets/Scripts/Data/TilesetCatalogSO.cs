using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Capa de terreno adicional (agua, lava, pasto...). El catálogo clasifica los tiles por capa;
    /// las capas extra se consultan después de piso/muro.
    /// </summary>
    [System.Serializable]
    public class TerrainTileLayer
    {
        public TerrainType type;
        public List<TileBase> tiles = new List<TileBase>();
    }

    /// <summary>
    /// Catálogo de tiles y sprites del tileset: qué tiles se usan como piso/muro, qué tiles
    /// pertenecen a cada terreno adicional y qué sprites como props decorativos o barreras.
    /// Es el único punto que hay que tocar para cambiar el arte del mapa.
    /// </summary>
    [CreateAssetMenu(fileName = "TilesetCatalog", menuName = "Madness Dreams/Tileset Catalog")]
    public class TilesetCatalogSO : ScriptableObject
    {
        [Header("Suelo")]
        [SerializeField] private List<TileBase> floorTiles = new List<TileBase>();

        [Header("Muro")]
        [SerializeField] private List<TileBase> wallTiles = new List<TileBase>();

        [Header("Terrenos adicionales")]
        [SerializeField] private List<TerrainTileLayer> terrainLayers = new List<TerrainTileLayer>();

        [Header("Objetos (sprites completos)")]
        [SerializeField] private List<Sprite> decorSprites = new List<Sprite>();
        [SerializeField] private List<Sprite> barrierSprites = new List<Sprite>();

        [Header("Selección (para re-construir el catálogo)")]
        [SerializeField] private string roomSheetPath = "Assets/Art/Tilesets/walls_floor.png";
        [SerializeField] private int roomSheetColumns = 13;
        [SerializeField] private List<int> floorIndices = new List<int>();
        [SerializeField] private List<int> wallIndices = new List<int>();
        [SerializeField] private List<int> waterIndices = new List<int>();
        [SerializeField] private string objectSheetPath = "Assets/Art/Tilesets/Objects.png";
        [SerializeField] private List<int> decorIndices = new List<int>();
        [SerializeField] private List<int> barrierIndices = new List<int>();

        public List<TileBase> FloorTiles => floorTiles;
        public List<TileBase> WallTiles => wallTiles;
        public List<TerrainTileLayer> TerrainTileLayers => terrainLayers;
        public List<Sprite> DecorSprites => decorSprites;
        public List<Sprite> BarrierSprites => barrierSprites;

        public string RoomSheetPath => roomSheetPath;
        public int RoomSheetColumns => Mathf.Max(1, roomSheetColumns);
        public List<int> FloorIndices => floorIndices;
        public List<int> WallIndices => wallIndices;
        public List<int> WaterIndices => waterIndices;
        public string ObjectSheetPath => objectSheetPath;
        public List<int> DecorIndices => decorIndices;
        public List<int> BarrierIndices => barrierIndices;

        /// <summary>Guarda la ruta del sheet de piso/muro y sus columnas (para el selector de la ventana).</summary>
        public void SetRoomSheet(string path, int columns)
        {
            roomSheetPath = path;
            roomSheetColumns = Mathf.Max(1, columns);
        }

        /// <summary>Guarda la ruta del sheet de objetos (props y barreras).</summary>
        public void SetObjectSheet(string path)
        {
            objectSheetPath = path;
        }

        /// <summary>Guarda la selección de índices de piso y muro.</summary>
        public void SetSelection(List<int> floors, List<int> walls)
        {
            floorIndices = floors != null ? new List<int>(floors) : new List<int>();
            wallIndices = walls != null ? new List<int>(walls) : new List<int>();
        }

        /// <summary>Guarda la selección de índices de agua.</summary>
        public void SetWaterSelection(List<int> water)
        {
            waterIndices = water != null ? new List<int>(water) : new List<int>();
        }

        /// <summary>Guarda la selección de índices de objetos: decorativos y barreras.</summary>
        public void SetObjectSelection(List<int> decor, List<int> barriers)
        {
            decorIndices = decor != null ? new List<int>(decor) : new List<int>();
            barrierIndices = barriers != null ? new List<int>(barriers) : new List<int>();
        }

        /// <summary>Elige un tile de piso al azar (o null si la lista está vacía).</summary>
        public TileBase PickFloor(System.Random rng)
        {
            return Pick(floorTiles, rng);
        }

        /// <summary>Elige un tile de muro al azar (o null si la lista está vacía).</summary>
        public TileBase PickWall(System.Random rng)
        {
            return Pick(wallTiles, rng);
        }

        /// <summary>Devuelve los tiles de una capa de terreno adicional, o null si no existe.</summary>
        public List<TileBase> GetTiles(TerrainType type)
        {
            if (terrainLayers == null)
            {
                return null;
            }

            for (int i = 0; i < terrainLayers.Count; i++)
            {
                TerrainTileLayer layer = terrainLayers[i];
                if (layer != null && layer.type == type)
                {
                    return layer.tiles;
                }
            }

            return null;
        }

        /// <summary>Atajo para los tiles de agua.</summary>
        public List<TileBase> WaterTiles => GetTiles(TerrainType.Water);

        /// <summary>
        /// Clasifica un tile: null = Void (fuera del mapa), muro = Wall, capa adicional = su tipo,
        /// resto = Floor. Las celdas de piso sin tile no se consideran transitables automáticamente.
        /// </summary>
        public TerrainType GetTerrain(TileBase tile)
        {
            if (tile == null)
            {
                return TerrainType.Void;
            }

            if (IsWall(tile))
            {
                return TerrainType.Wall;
            }

            if (terrainLayers != null)
            {
                for (int i = 0; i < terrainLayers.Count; i++)
                {
                    TerrainTileLayer layer = terrainLayers[i];
                    if (layer != null && ContainsTileOrSprite(layer.tiles, tile))
                    {
                        return layer.type;
                    }
                }
            }

            return TerrainType.Floor;
        }

        /// <summary>
        /// Indica si un tile pertenece al conjunto de muros del catálogo (las celdas con un tile que
        /// no sea de muro se consideran piso transitable).
        /// </summary>
        public bool IsWall(TileBase tile)
        {
            return ContainsTileOrSprite(wallTiles, tile);
        }

        /// <summary>
        /// Resuelve tiles capturados directamente del atlas al tile canónico del catálogo.
        /// Los tiles del atlas suelen tener colisión Sprite por defecto, mientras que el catálogo
        /// configura el piso sin colisión y los muros con colisión de grilla.
        /// </summary>
        public TileBase ResolveTile(TileBase tile)
        {
            if (tile == null)
            {
                return null;
            }

            TileBase resolved = FindMatchingTile(wallTiles, tile);
            if (resolved != null)
            {
                return resolved;
            }

            resolved = FindMatchingTile(floorTiles, tile);
            if (resolved != null)
            {
                return resolved;
            }

            if (terrainLayers != null)
            {
                for (int i = 0; i < terrainLayers.Count; i++)
                {
                    TerrainTileLayer layer = terrainLayers[i];
                    resolved = layer != null ? FindMatchingTile(layer.tiles, tile) : null;
                    if (resolved != null)
                    {
                        return resolved;
                    }
                }
            }

            return tile;
        }

        private static bool ContainsTileOrSprite(List<TileBase> tiles, TileBase tile)
        {
            return FindMatchingTile(tiles, tile) != null;
        }

        private static TileBase FindMatchingTile(List<TileBase> tiles, TileBase tile)
        {
            if (tiles == null || tile == null)
            {
                return null;
            }

            if (tiles.Contains(tile))
            {
                return tile;
            }

            Sprite sourceSprite = GetSprite(tile);
            if (sourceSprite == null)
            {
                return null;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                TileBase candidate = tiles[i];
                if (candidate != null && GetSprite(candidate) == sourceSprite)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Sprite GetSprite(TileBase tile)
        {
            return tile is Tile spriteTile ? spriteTile.sprite : null;
        }

        private static T Pick<T>(List<T> list, System.Random rng) where T : class
        {
            if (list == null || list.Count == 0)
            {
                return null;
            }

            return list[rng.Next(list.Count)];
        }
    }
}
