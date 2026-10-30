using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Barra de pociones/efectos (derecha, debajo del mapa).</summary>
    public class EffectsBarWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private EffectsBarEntry entryPrefab;
        [SerializeField] private Sprite healSprite;
        [SerializeField] private Sprite damageSprite;
        [SerializeField] private Sprite speedSprite;
        [SerializeField] private Sprite healthSprite;

        private PotionInventory inventory;
        private PlayerBuffs buffs;
        private readonly List<EffectsBarEntry> entries = new List<EffectsBarEntry>();

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            inventory = player != null ? player.GetComponent<PotionInventory>() : null;
            buffs = player != null ? player.GetComponent<PlayerBuffs>() : null;
            if (inventory != null) inventory.Changed += Rebuild;
            if (buffs != null) buffs.BuffsChanged += Rebuild;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= Rebuild;
            if (buffs != null) buffs.BuffsChanged -= Rebuild;
        }

        private void Update()
        {
            for (int i = 0; i < entries.Count; i++) if (entries[i] != null) entries[i].Tick();
        }

        private void Rebuild()
        {
            Clear();
            if (inventory == null) return;

            TryAdd(PotionKind.Heal, healSprite);
            TryAdd(PotionKind.Damage, damageSprite);
            TryAdd(PotionKind.Speed, speedSprite);
            TryAdd(PotionKind.MaxHealth, healthSprite);
        }

        private void TryAdd(PotionKind kind, Sprite sprite)
        {
            if (entryPrefab == null) return;

            bool active = kind != PotionKind.Heal && buffs != null && buffs.GetStacks(BuffKindOf(kind)) > 0;
            int count = inventory.Count(kind);
            if (count <= 0 && !active) return;

            EffectsBarEntry entry = Instantiate(entryPrefab, container);
            entry.Bind(sprite, inventory, buffs, kind);
            entries.Add(entry);
        }

        private static BuffKind BuffKindOf(PotionKind kind)
        {
            switch (kind)
            {
                case PotionKind.Damage: return BuffKind.Damage;
                case PotionKind.Speed: return BuffKind.Speed;
                default: return BuffKind.MaxHealth;
            }
        }

        private void Clear()
        {
            for (int i = 0; i < entries.Count; i++) if (entries[i] != null) Destroy(entries[i].gameObject);
            entries.Clear();
        }
    }
}
