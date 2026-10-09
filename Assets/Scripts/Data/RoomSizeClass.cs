using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Clasificación de salas por su número de celdas.
    /// </summary>
    public enum RoomSizeClass
    {
        Small = 0,
        Wide = 1,
        Square = 2,
        Large = 3
    }

    /// <summary>
    /// Utilidades para trabajar con el tamaño de las salas: candidatos por clase, clasificación
    /// y recuento de celdas.
    /// </summary>
    public static class RoomSizes
    {
        private static readonly Vector2Int[] SmallCells = { new Vector2Int(1, 1) };
        private static readonly Vector2Int[] WideCells = { new Vector2Int(2, 1), new Vector2Int(1, 2) };
        private static readonly Vector2Int[] SquareCells = { new Vector2Int(2, 2) };
        private static readonly Vector2Int[] LargeCells = { new Vector2Int(3, 3) };

        /// <summary>
        /// Devuelve las posibles formas en celdas para la clase de tamaño indicada.
        /// </summary>
        public static Vector2Int[] Candidates(RoomSizeClass sizeClass)
        {
            switch (sizeClass)
            {
                case RoomSizeClass.Small: return SmallCells;
                case RoomSizeClass.Wide: return WideCells;
                case RoomSizeClass.Square: return SquareCells;
                default: return LargeCells;
            }
        }

        /// <summary>
        /// Clasifica una forma en celdas según su área total.
        /// </summary>
        public static RoomSizeClass Classify(Vector2Int cells)
        {
            int count = CellCount(cells);

            if (count <= 1)
            {
                return RoomSizeClass.Small;
            }

            if (count <= 2)
            {
                return RoomSizeClass.Wide;
            }

            if (count <= 4)
            {
                return RoomSizeClass.Square;
            }

            return RoomSizeClass.Large;
        }

        /// <summary>
        /// Devuelve el número de celdas (producto de sus lados, con un mínimo de 1 por lado).
        /// </summary>
        public static int CellCount(Vector2Int cells)
        {
            return Mathf.Max(1, cells.x) * Mathf.Max(1, cells.y);
        }
    }
}
