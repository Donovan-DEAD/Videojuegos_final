using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    [CreateAssetMenu(fileName = "BlockBank", menuName = "Madness Dreams/Block Bank")]
    public class BlockBankSO : ScriptableObject
    {
        [Header("Normal chunks (rooms)")]
        [SerializeField] private List<MapChunkSO> normalChunks = new List<MapChunkSO>();

        [Header("Boss arena")]
        [SerializeField] private MapChunkSO bossArena;

        public List<MapChunkSO> NormalChunks => normalChunks;
        public MapChunkSO BossArena => bossArena;

        public int NormalChunkCount => normalChunks == null ? 0 : normalChunks.Count;

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
    }
}
