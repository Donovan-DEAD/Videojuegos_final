using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Datos de un cuarto colocado: plantilla (chunk), tamaño y origen en celdas, tag temático y rol.</summary>
    [System.Serializable]
    public struct RoomPlacement
    {
        public MapChunkSO chunk;
        public Vector2Int sizeInCells;
        public Vector2Int originCell;
        public RoomTag tag;
        public CellRole role;

        public bool IsStart => role == CellRole.Start;
        public bool IsBoss => role == CellRole.Boss;
    }
}
