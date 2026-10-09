using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Categoría de un prop: decorativo o bloqueante del paso.
    /// </summary>
    public enum PropKind
    {
        Decor = 0,
        Barrier = 1
    }

    /// <summary>
    /// Definición de un prop colocable: prefab, categoría, huella en celdas, peso y si debe
    /// evitar los pasos.
    /// </summary>
    [CreateAssetMenu(fileName = "Prop", menuName = "Madness Dreams/Prop Definition")]
    public class PropDefinitionSO : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private PropKind kind = PropKind.Decor;
        [SerializeField] private Vector2Int footprint = Vector2Int.one;
        [SerializeField] private int weight = 1;
        [SerializeField] private bool avoidPassages = true;

        public GameObject Prefab => prefab;
        public PropKind Kind => kind;
        public Vector2Int Footprint => footprint;
        public int Weight => Mathf.Max(0, weight);
        public bool AvoidPassages => avoidPassages;
    }
}
