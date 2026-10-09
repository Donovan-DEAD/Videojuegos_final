using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Lado por el que una celda/cuarto comparte borde en la rejilla.</summary>
    public enum RoomSide
    {
        /// <summary>Conexión hacia la celda derecha.</summary>
        Right = 0,
        /// <summary>Conexión hacia la celda superior.</summary>
        Up = 1
    }

    /// <summary>Describe una conexión entre dos cuartos: índices, lado y celda de la rejilla donde se abre.</summary>
    [System.Serializable]
    public struct RoomConnection
    {
        public int roomA;
        public int roomB;
        public RoomSide side;
        public Vector2Int cell;
    }
}
