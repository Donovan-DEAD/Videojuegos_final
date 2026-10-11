using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Catálogo de tiles y sprites del tileset: qué tiles se usan como piso/muro y qué sprites como
    /// props decorativos o barreras. Es el único punto que hay que tocar para cambiar el arte del mapa.
    /// </summary>
    [CreateAssetMenu(fileName = "TilesetCatalog", menuName = "Madness Dreams/Tileset Catalog")]
    public class TilesetCatalogSO : ScriptableObject
    {
        [Header("Suelo")]
        [SerializeField] private List<TileBase> floorTiles = new List<TileBase>();

        [Header("Muro")]
        [SerializeField] private List<TileBase> wallTiles = new List<TileBase>();

        [Header("Objetos (sprites completos)")]
        [SerializeField] private List<Sprite> decorSprites = new List<Sprite>();
        [SerializeField] private List<Sprite> barrierSprites = new List<Sprite>();

        [Header("Selección (para re-construir el catálogo)")]
        [SerializeField] private string roomSheetPath = "Assets/Art/Tilesets/walls_floor.png";
        [SerializeField] private int roomSheetColumns = 13;
        [SerializeField] private List<int> floorIndices = new List<int>();
        [SerializeField] private List<int> wallIndices = new List<int>();
        [SerializeField] private string objectSheetPath = "Assets/Art/Tilesets/Objects.png";
        [SerializeField] private List<int> decorIndices = new List<int>();
        [SerializeField] private List<int> barrierIndices = new List<int>();

        public List<TileBase> FloorTiles => floorTiles;
        public List<TileBase> WallTiles => wallTiles;
        public List<Sprite> DecorSprites => decorSprites;
        public List<Sprite> BarrierSprites => barrierSprites;

        public string RoomSheetPath => roomSheetPath;
        public int RoomSheetColumns => Mathf.Max(1, roomSheetColumns);
        public List<int> FloorIndices => floorIndices;
        public List<int> WallIndices => wallIndices;
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

        /// <summary>
        /// Indica si un tile pertenece al conjunto de muros del catálogo (las celdas con un tile que
        /// no sea de muro se consideran piso transitable).
        /// </summary>
        public bool IsWall(TileBase tile)
        {
            return tile != null && wallTiles != null && wallTiles.Contains(tile);
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
