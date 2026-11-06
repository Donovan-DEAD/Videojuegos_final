namespace MadnessDreams
{
    /// <summary>
    /// Tipo de terreno de una celda del mapa. Es un flags para poder agregar tipos nuevos
    /// (lava, pasto, etc.) sin reescribir la clasificación.
    /// </summary>
    [System.Flags]
    public enum TerrainType
    {
        Void = 0,
        Floor = 1,
        Wall = 2,
        Water = 4,
        Lava = 8,
        Grass = 16
    }
}
