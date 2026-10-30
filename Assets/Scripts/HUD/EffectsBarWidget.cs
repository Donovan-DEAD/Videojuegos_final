using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Barra de efectos activos (derecha, abajo del mapa).</summary>
    public class EffectsBarWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private EffectsBarEntry entryPrefab;
        [SerializeField] private Sprite damageSprite;
        [SerializeField] private Sprite speedSprite;
        [SerializeField] private Sprite healthSprite;

        private PlayerBuffs buffs;
        private readonly List<EffectsBarEntry> entries = new List<EffectsBarEntry>();

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            buffs = player != null ? player.GetComponent<PlayerBuffs>() : null;
            if (buffs != null) buffs.BuffsChanged += Rebuild;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (buffs != null) buffs.BuffsChanged -= Rebuild;
        }

        private void Update()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null) entries[i].Tick();
            }
        }

        private void Rebuild()
        {
            Clear();
            if (buffs == null) return;

            TryAdd(BuffKind.Damage, damageSprite);
            TryAdd(BuffKind.Speed, speedSprite);
            TryAdd(BuffKind.MaxHealth, healthSprite);
        }

        private void TryAdd(BuffKind kind, Sprite sprite)
        {
            if (buffs.GetStacks(kind) <= 0 || entryPrefab == null) return;

            EffectsBarEntry entry = Instantiate(entryPrefab, container);
            entry.Bind(sprite, buffs.GetStacks(kind), buffs, kind);
            entries.Add(entry);
        }

        private void Clear()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null) Destroy(entries[i].gameObject);
            }
            entries.Clear();
        }
    }
}
