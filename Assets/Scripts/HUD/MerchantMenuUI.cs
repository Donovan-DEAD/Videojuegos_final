using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Menú temporal de compra del comerciante. Layout de selección + detalle, listo para cambiar arte.</summary>
    public class MerchantMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text balanceLabel;
        [SerializeField] private Text confirmationLabel;

        [Header("Detalle del objeto")]
        [SerializeField] private Image itemImage;
        [SerializeField] private Text itemNameLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text priceLabel;
        [SerializeField] private Button confirmButton;

        [Header("Selectores")]
        [SerializeField] private Button healButton;
        [SerializeField] private Button damageButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button healthButton;
        [SerializeField] private Button closeButton;

        [Header("Costos")]
        [SerializeField] private int healCost = 5;
        [SerializeField] private int damageCost = 10;
        [SerializeField] private int speedCost = 10;
        [SerializeField] private int healthCost = 15;

        private enum ItemKind { None, Heal, Damage, Speed, Health }
        private ItemKind selected = ItemKind.None;

        private PlayerBuffs buffs;
        private PotionInventory inventory;
        private Health health;

        private void Awake()
        {
            if (healButton != null) healButton.onClick.AddListener(() => Select(ItemKind.Heal));
            if (damageButton != null) damageButton.onClick.AddListener(() => Select(ItemKind.Damage));
            if (speedButton != null) speedButton.onClick.AddListener(() => Select(ItemKind.Speed));
            if (healthButton != null) healthButton.onClick.AddListener(() => Select(ItemKind.Health));
            if (confirmButton != null) confirmButton.onClick.AddListener(Confirm);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            buffs = player != null ? player.GetComponent<PlayerBuffs>() : null;
            inventory = player != null ? player.GetComponent<PotionInventory>() : null;
            health = player != null ? player.GetComponent<Health>() : null;

            Close();
        }

        private void Start()
        {
            if (GameFlowManager.Instance != null) GameFlowManager.Instance.FragmentsChanged += OnFragmentsChanged;
        }

        private void OnDestroy()
        {
            if (GameFlowManager.Instance != null) GameFlowManager.Instance.FragmentsChanged -= OnFragmentsChanged;
        }

        public void Open()
        {
            if (MenuManager.Instance != null) MenuManager.Instance.Open(this, Close);
            UpdateBalance();
            ShowMessage("");
            if (panel != null) panel.SetActive(true);
        }

        public void Close()
        {
            if (MenuManager.Instance != null) MenuManager.Instance.Release(this);
            if (panel != null) panel.SetActive(false);
        }

        private void OnFragmentsChanged(int total)
        {
            if (balanceLabel != null) balanceLabel.text = "Fragmentos: " + total;
        }

        private void UpdateBalance()
        {
            int total = GameFlowManager.Instance != null ? GameFlowManager.Instance.Fragments : 0;
            if (balanceLabel != null) balanceLabel.text = "Fragmentos: " + total;
        }

        private void Select(ItemKind kind)
        {
            selected = kind;
            if (itemNameLabel != null) itemNameLabel.text = NameOf(kind);
            if (descriptionLabel != null) descriptionLabel.text = DescriptionOf(kind);
            if (priceLabel != null) priceLabel.text = "Precio: " + CostOf(kind);
            if (itemImage != null) itemImage.color = ColorOf(kind);
            ShowMessage("");
        }

        private void Confirm()
        {
            if (selected == ItemKind.None) { ShowMessage("Selecciona un objeto"); return; }

            int cost = CostOf(selected);
            if (GameFlowManager.Instance == null || !GameFlowManager.Instance.SpendFragments(cost))
            {
                ShowMessage("Fragmentos insuficientes");
                return;
            }

            Apply(selected);
            UpdateBalance();
            ShowMessage("Comprado: " + NameOf(selected));
        }

        private void Apply(ItemKind kind)
        {
            if (inventory == null) return;
            switch (kind)
            {
                case ItemKind.Heal: inventory.Add(PotionKind.Heal); break;
                case ItemKind.Damage: inventory.Add(PotionKind.Damage); break;
                case ItemKind.Speed: inventory.Add(PotionKind.Speed); break;
                case ItemKind.Health: inventory.Add(PotionKind.MaxHealth); break;
            }
        }

        private void ShowMessage(string msg)
        {
            if (confirmationLabel != null) confirmationLabel.text = msg;
        }

        private string NameOf(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Heal: return "Curación";
                case ItemKind.Damage: return "Daño +";
                case ItemKind.Speed: return "Velocidad +";
                case ItemKind.Health: return "Vida máx +";
                default: return "";
            }
        }

        private string DescriptionOf(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Heal: return "Recupera toda la vida.";
                case ItemKind.Damage: return "Aumenta el daño temporalmente.";
                case ItemKind.Speed: return "Aumenta la velocidad temporalmente.";
                case ItemKind.Health: return "Aumenta la vida máxima temporalmente.";
                default: return "";
            }
        }

        private int CostOf(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Heal: return healCost;
                case ItemKind.Damage: return damageCost;
                case ItemKind.Speed: return speedCost;
                case ItemKind.Health: return healthCost;
                default: return 0;
            }
        }

        private static Color ColorOf(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Heal: return new Color(0.3f, 0.9f, 0.4f);
                case ItemKind.Damage: return new Color(0.9f, 0.35f, 0.3f);
                case ItemKind.Speed: return new Color(0.4f, 0.7f, 1f);
                case ItemKind.Health: return new Color(0.9f, 0.7f, 0.3f);
                default: return Color.gray;
            }
        }
    }
}
