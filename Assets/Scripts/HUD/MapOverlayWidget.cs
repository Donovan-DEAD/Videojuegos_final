using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Overlay de mapa a pantalla completa (tecla M): reconstruye la cuadrícula de cuartos y posiciona el marcador del jugador.
    /// </summary>
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

        /// <summary>
        /// Indica si el overlay está actualmente abierto (su raíz activa).
        /// </summary>
        public bool IsOpen => root != null && root.activeSelf;

        /// <summary>
        /// Enlaza el botón de cierre y arranca con el overlay cerrado.
        /// </summary>
        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            Close();
        }

        /// <summary>
        /// Se suscribe a la regeneración del mapa al activarse.
        /// </summary>
        private void OnEnable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated += Build;
            }
        }

        /// <summary>
        /// Deja de escuchar la regeneración del mapa al desactivarse.
        /// </summary>
        private void OnDisable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated -= Build;
            }
        }

        /// <summary>
        /// Asocia el overlay al MapGenerator (para reconstruir al generarse) y al transform del jugador.
        /// </summary>
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

        /// <summary>
        /// Abre el overlay si está cerrado o lo cierra si ya está abierto.
        /// </summary>
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

        /// <summary>
        /// Reconstruye el mapa y muestra el overlay.
        /// </summary>
        public void Open()
        {
            Build();

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        /// <summary>
        /// Oculta el overlay.
        /// </summary>
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
            chunkSize = config != null ? config.ChunkSize : 16;
            width = config != null ? config.Width : 8;
            height = config != null ? config.Height : 8;

            MapGrid.ClearCells(container, playerMarker);

            if (mapGenerator != null && mapGenerator.Rooms.Count > 0)
            {
                MapGrid.BuildRooms(container, mapGenerator.Rooms, height, cellSize, cellGap, startColor, normalColor, bossColor);
            }
            else
            {
                Vector2Int start = new Vector2Int(width / 2, height / 2);
                Vector2Int boss = new Vector2Int(width - 1, height - 1);
                MapGrid.Build(container, width, height, cellSize, cellGap, start, boss, 1, startColor, normalColor, bossColor);
            }

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
