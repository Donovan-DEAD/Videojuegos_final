using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class MinimapWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private float cellSize = 18f;
        [SerializeField] private float cellGap = 2f;
        [SerializeField] private Color startColor = new Color(0.30f, 0.85f, 0.35f);
        [SerializeField] private Color normalColor = new Color(0.32f, 0.32f, 0.38f);
        [SerializeField] private Color bossColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private Color clearedColor = new Color(0.22f, 0.45f, 0.30f);
        [SerializeField] private Color playerColor = new Color(0.40f, 0.90f, 1f);

        private MapGenerator mapGenerator;
        private Transform player;
        private Image[,] cells;
        private int width;
        private int height;
        private int chunkSize;
        private bool built;

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

            Build();
        }

        private void OnDestroy()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated -= Build;
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
            cells = MapGrid.Build(container, width, height, cellSize, cellGap, startChunk, bossChunk, bossRegionSize, startColor, normalColor, bossColor);

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

        public void MarkCleared(Vector2Int cell)
        {
            if (cells == null || cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
            {
                return;
            }

            Image img = cells[cell.x, cell.y];
            if (img != null)
            {
                img.color = clearedColor;
            }
        }

        private void LateUpdate()
        {
            if (!built || player == null || playerMarker == null)
            {
                return;
            }

            playerMarker.anchoredPosition = MapGrid.MarkerPosition(player.position, chunkSize, cellSize, cellGap, width, height);
        }
    }
}
