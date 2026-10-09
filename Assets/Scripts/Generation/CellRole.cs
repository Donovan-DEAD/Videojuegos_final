namespace MadnessDreams
{
    /// <summary>Rol de una celda/cuarto en la rejilla generada.</summary>
    public enum CellRole
    {
        /// <summary>Celda vacía, sin cuarto.</summary>
        Empty = 0,
        /// <summary>Cuarto inicial del jugador.</summary>
        Start = 1,
        /// <summary>Cuarto normal de combate/exploración.</summary>
        Normal = 2,
        /// <summary>Cuarto de jefe, el más lejano al inicio.</summary>
        Boss = 3
    }
}
