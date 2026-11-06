using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Clasificación visual de un tileset para pintar cuartos por rol (centro/borde de piso,
    /// cuerpo/remate de muro y agua) en vez de elegir tiles al azar.
    /// </summary>
    [CreateAssetMenu(fileName = "TilePainterProfile", menuName = "Madness Dreams/Tile Painter Profile")]
    public class TilePainterProfileSO : ScriptableObject
    {
        [Header("Piso")]
        [SerializeField] private List<TileBase> floorCenter = new List<TileBase>();
        [SerializeField] private List<TileBase> floorAccent = new List<TileBase>();
        [SerializeField] private List<TileBase> floorEdgeTop = new List<TileBase>();
        [SerializeField] private List<TileBase> floorEdgeBottom = new List<TileBase>();
        [SerializeField] private List<TileBase> floorEdgeLeft = new List<TileBase>();
        [SerializeField] private List<TileBase> floorEdgeRight = new List<TileBase>();

        [Header("Muro")]
        [SerializeField] private List<TileBase> wallBody = new List<TileBase>();
        [SerializeField] private List<TileBase> wallTop = new List<TileBase>();
        [SerializeField] private List<TileBase> wallCap = new List<TileBase>();

        [Header("Agua")]
        [SerializeField] private List<TileBase> waterCenter = new List<TileBase>();
        [SerializeField] private List<TileBase> waterEdge = new List<TileBase>();

        [Header("Pintado")]
        [SerializeField] private int cellSize = 16;
        [SerializeField] private int openingWidth = 4;
        [SerializeField, Range(0f, 1f)] private float accentChance = 0.12f;

        public int CellSize => Mathf.Max(1, cellSize);
        public int OpeningWidth => Mathf.Clamp(openingWidth, 1, Mathf.Max(1, cellSize - 2));
        public float AccentChance => Mathf.Clamp01(accentChance);
        public bool HasWater => Has(waterCenter) || Has(waterEdge);
        public List<TileBase> WaterCenter => waterCenter;

        /// <summary>Reemplaza la lista de tiles de agua (la usa el builder/migración).</summary>
        public void SetWaterTiles(List<TileBase> tiles)
        {
            waterCenter = tiles != null ? new List<TileBase>(tiles) : new List<TileBase>();
        }

        /// <summary>Elige el tile de agua: centro si existe; si no, borde; si no, null.</summary>
        public TileBase PickWater(int x, int y, int seed)
        {
            int hash = Hash(x, y, seed);
            if (Has(waterCenter)) return Pick(waterCenter, hash);
            return Pick(waterEdge, hash);
        }

        /// <summary>Elige el tile de piso según los muros vecinos (arriba/abajo/izquierda/derecha).</summary>
        public TileBase PickFloor(bool wallAbove, bool wallBelow, bool wallLeft, bool wallRight, int x, int y, int seed)
        {
            List<TileBase> list = null;

            if (wallAbove && Has(floorEdgeTop)) list = floorEdgeTop;
            else if (wallLeft && Has(floorEdgeLeft)) list = floorEdgeLeft;
            else if (wallBelow && Has(floorEdgeBottom)) list = floorEdgeBottom;
            else if (wallRight && Has(floorEdgeRight)) list = floorEdgeRight;

            if (!Has(list))
            {
                int hashCenter = Hash(x, y, seed);
                if (Has(floorAccent) && (hashCenter % 1000) < AccentChance * 1000f)
                {
                    return Pick(floorAccent, hashCenter);
                }

                list = floorCenter;
            }

            return Pick(list, Hash(x, y, seed));
        }

        /// <summary>Elige el tile de muro: tapa (muro de 2+ tiles), remate superior o cuerpo.</summary>
        public TileBase PickWall(bool cap, bool topExposed, int x, int y, int seed)
        {
            int hash = Hash(x, y, seed);

            if (cap && Has(wallCap)) return Pick(wallCap, hash);
            if (topExposed && Has(wallTop)) return Pick(wallTop, hash);
            if (Has(wallBody)) return Pick(wallBody, hash);

            return Pick(wallCap, hash) ?? Pick(wallTop, hash);
        }

        private static bool Has(List<TileBase> list)
        {
            return list != null && list.Count > 0;
        }

        private static TileBase Pick(List<TileBase> list, int hash)
        {
            if (!Has(list))
            {
                return null;
            }

            return list[hash % list.Count];
        }

        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int hash = (x * 73856093) ^ (y * 19349663) ^ (seed * 83492791);
                return hash & 0x7fffffff;
            }
        }
    }
}
