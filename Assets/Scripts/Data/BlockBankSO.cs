using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Banco de fragmentos de mapa de un mundo: agrupa salas normales, la sala de inicio y la
    /// arena de jefe, y permite consultarlos por índice, tamaño o etiqueta.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockBank", menuName = "Madness Dreams/Block Bank")]
    public class BlockBankSO : ScriptableObject
    {
        [Header("Normal chunks (rooms)")]
        [SerializeField] private List<MapChunkSO> normalChunks = new List<MapChunkSO>();

        [Header("Start room")]
        [SerializeField] private MapChunkSO startRoom;

        [Header("Boss arena")]
        [SerializeField] private MapChunkSO bossArena;

        public List<MapChunkSO> NormalChunks => normalChunks;
        public MapChunkSO StartRoom => startRoom;
        public MapChunkSO BossArena => bossArena;

        public int NormalChunkCount => normalChunks == null ? 0 : normalChunks.Count;

        /// <summary>
        /// Devuelve el fragmento normal en la posición indicada, envolviendo el índice para
        /// aceptar valores fuera de rango.
        /// </summary>
        public MapChunkSO GetChunk(int index)
        {
            if (normalChunks == null || normalChunks.Count == 0)
            {
                return null;
            }

            index = ((index % normalChunks.Count) + normalChunks.Count) % normalChunks.Count;
            return normalChunks[index];
        }

        public MapChunkSO GetRandomChunk(System.Random rng)
        {
            if (normalChunks == null || normalChunks.Count == 0)
            {
                return null;
            }

            return normalChunks[rng.Next(normalChunks.Count)];
        }

        /// <summary>
        /// Rellena <paramref name="results"/> (vaciándolo antes) con los fragmentos normales cuyo
        /// tamaño resuelto coincide con el indicado.
        /// </summary>
        public void FindBySize(Vector2Int sizeInCells, int cellSize, List<MapChunkSO> results)
        {
            results.Clear();
            if (normalChunks == null)
            {
                return;
            }

            for (int i = 0; i < normalChunks.Count; i++)
            {
                MapChunkSO chunk = normalChunks[i];
                if (chunk != null && chunk.ResolveSizeInCells(cellSize) == sizeInCells)
                {
                    results.Add(chunk);
                }
            }
        }

        /// <summary>
        /// Rellena <paramref name="results"/> (vaciándolo antes) con los fragmentos normales que
        /// tienen la etiqueta de sala indicada.
        /// </summary>
        public void FindByTag(RoomTag tag, List<MapChunkSO> results)
        {
            results.Clear();
            if (normalChunks == null)
            {
                return;
            }

            for (int i = 0; i < normalChunks.Count; i++)
            {
                MapChunkSO chunk = normalChunks[i];
                if (chunk != null && chunk.Tag == tag)
                {
                    results.Add(chunk);
                }
            }
        }
    }
}
