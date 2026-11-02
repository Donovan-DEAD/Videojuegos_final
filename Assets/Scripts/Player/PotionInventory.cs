using System;
using UnityEngine;

namespace MadnessDreams
{
    public enum PotionKind { Heal = 0, Damage = 1, Speed = 2, MaxHealth = 3 }

    /// <summary>Inventario de pociones compradas; se activan con teclas numéricas.</summary>
    public class PotionInventory : MonoBehaviour
    {
        public event Action Changed;
        private readonly int[] counts = new int[4];

        public int Count(PotionKind kind) => counts[(int)kind];

        private void Awake()
        {
            if (GameFlowManager.Instance == null) return;
            for (int i = 0; i < counts.Length; i++) counts[i] = GameFlowManager.Instance.GetSavedPotion(i);
        }

        private void Save()
        {
            if (GameFlowManager.Instance == null) return;
            for (int i = 0; i < counts.Length; i++) GameFlowManager.Instance.SetSavedPotion(i, counts[i]);
        }

        public void Add(PotionKind kind, int amount = 1)
        {
            counts[(int)kind] += amount;
            Save();
            Changed?.Invoke();
        }

        public bool TryConsume(PotionKind kind)
        {
            if (counts[(int)kind] <= 0) return false;
            counts[(int)kind]--;
            Save();
            Changed?.Invoke();
            return true;
        }
    }
}
