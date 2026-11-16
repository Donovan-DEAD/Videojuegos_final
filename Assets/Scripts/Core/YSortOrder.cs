using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Ordena los renderers del objeto por su posición Y: los que están más abajo se dibujan
    /// adelante. Se recalcula en LateUpdate (barato); baseOrder permite sesgos por tipo
    /// (p. ej. proyectiles +1000 para que siempre se vean encima).
    /// </summary>
    public class YSortOrder : MonoBehaviour
    {
        [SerializeField] private int baseOrder = 0;
        [SerializeField] private bool includeChildren = false;

        private Renderer[] renderers;
        /// <summary>Base de orden serializada (para que EnemyIdentity calcule offsets).</summary>
        public int BaseOrder => baseOrder;

        /// <summary>Fija la base de orden en runtime (lo usa EnemyIdentity para badges/labels).</summary>
        public void SetBaseOrder(int value)
        {
            baseOrder = value;
            Apply();
        }
        private Transform cachedTransform;

        private void Awake()
        {
            cachedTransform = transform;
            renderers = includeChildren
                ? GetComponentsInChildren<Renderer>(true)
                : new Renderer[] { GetComponent<Renderer>() };
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            int order = baseOrder - Mathf.RoundToInt(cachedTransform.position.y * 100f);
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].sortingOrder = order;
            }
        }
    }
}