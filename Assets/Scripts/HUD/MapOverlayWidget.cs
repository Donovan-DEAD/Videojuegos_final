using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class MapOverlayWidget : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private Button closeButton;
        [SerializeField] private float cellSize = 46f;
        [SerializeField] private float cellGap = 4f;
        [SerializeField] private Color startColor = new Color(0.30f, 0.85f, 0.35f);
        [SerializeField] private Color normalColor = new Color(0.32f, 0.32f, 0.38f);
        [SerializeField] private Color bossColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private Color playerColor = new Color(0.40f, 0.90f, 1f);

        private MapGenerator mapGenerator;
        private Transform player;
        private int width;
        private int height;
        private int chunkSize;
        private bool built;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            Close();
        }

        private void OnEnable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated += Build;
            }
        }

        private void OnDisable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated -= Build;
            }
        }

        public void Bind(MapGenerator generator, Transform playerTransform)
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated -= Build;
            }

            mapGenerator = generator;
            player = playerTransform;

            if (mapGenerator != null)
            {
                mapGenerator.Generated += Build;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            bool pressed = keyboard != null ? keyboard.mKey.wasPressedThisFrame : Input.GetKeyDown(KeyCode.M);

            if (pressed)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            Build();

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Close()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void Build()
        {
            WorldConfigSO config = mapGenerator != null ? mapGenerator.Config : null;
            Vector2Int startChunk;
            Vector2Int bossChunk;
            int bossRegionSize;

            if (config != null)
            {
                width = config.Width;
                height = config.Height;
                chunkSize = config.ChunkSize;
                bossRegionSize = config.BossRegionSize;
                startChunk = mapGenerator.StartChunk;
                bossChunk = mapGenerator.BossOriginChunk;
            }
            else
            {
                width = 8;
                height = 8;
                chunkSize = 16;
                bossRegionSize = 2;
                startChunk = new Vector2Int(width / 2, height / 2);
                bossChunk = new Vector2Int(width - bossRegionSize, height - bossRegionSize);
            }

            MapGrid.ClearCells(container, playerMarker);
            MapGrid.Build(container, width, height, cellSize, cellGap, startChunk, bossChunk, bossRegionSize, startColor, normalColor, bossColor);

            if (playerMarker != null)
            {
                Image marker = playerMarker.GetComponent<Image>();
                if (marker != null)
                {
                    marker.color = playerColor;
                }

                playerMarker.SetAsLastSibling();
            }

            built = true;
        }

        private void LateUpdate()
        {
            if (!IsOpen || !built || player == null || playerMarker == null)
            {
                return;
            }

            playerMarker.anchoredPosition = MapGrid.MarkerPosition(player.position, chunkSize, cellSize, cellGap, width, height);
        }
    }
}
