using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Minimapa que dibuja los cuartos del MapGenerator, marca los limpiados y sigue la posición del jugador.
    /// </summary>
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
        [SerializeField] private float padding = 6f;
        [SerializeField] private Color currentColor = Color.white;
        [SerializeField] private Color visitedColor = new Color(0.42f, 0.44f, 0.52f);

        private MapGenerator mapGenerator;
        private Transform player;
        private Image[] roomImages;
        private int width;
        private int height;
        private int chunkSize;
        private bool built;
        private Color[] roomBaseColors;
        private int currentRoomIndex = -1;
        private System.Collections.Generic.HashSet<int> visitedRooms = new System.Collections.Generic.HashSet<int>();

        /// <summary>
        /// Asocia el minimapa al MapGenerator (reconstruye al generarse) y al transform del jugador, y lo construye.
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

            Build();
        }

        /// <summary>
        /// Deja de escuchar la regeneración del mapa al destruirse.
        /// </summary>
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
            chunkSize = config != null ? config.ChunkSize : 16;
            width = config != null ? config.Width : 8;
            height = config != null ? config.Height : 8;
            cellSize = MapGrid.FitCellSize(container, width, height, cellGap, padding);

            MapGrid.ClearCells(container, playerMarker);

            if (mapGenerator != null && mapGenerator.Rooms.Count > 0)
            {
                roomImages = MapGrid.BuildRooms(container, mapGenerator.Rooms, height, cellSize, cellGap, startColor, normalColor, bossColor);
            }
            else
            {
                roomImages = null;
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

        /// <summary>
        /// Pinta el cuarto indicado con el color de cuarto limpiado.
        /// </summary>
        public void MarkCleared(int roomIndex)
        {
            if (roomImages == null || roomIndex < 0 || roomIndex >= roomImages.Length || roomImages[roomIndex] == null)
            {
                return;
            }

            roomImages[roomIndex].color = clearedColor;
            if (roomBaseColors != null && roomIndex < roomBaseColors.Length)
            {
                roomBaseColors[roomIndex] = clearedColor;
            }
        }

        /// <summary>Ilumina el cuarto actual del jugador y restaura el color del anterior.</summary>
        /// <summary>Marca el cuarto actual como visitado, pinta los visitados con tinte tenue y el actual en blanco.</summary>
        private void HighlightCurrentRoom()
        {
            if (roomImages == null || mapGenerator == null || player == null)
            {
                return;
            }
        
            int index = MapGrid.RoomIndexAt(mapGenerator.Rooms, player.position, chunkSize);
            if (index >= 0)
            {
                visitedRooms.Add(index);
            }
        
            for (int i = 0; i < roomImages.Length; i++)
            {
                if (roomImages[i] == null)
                {
                    continue;
                }
        
                Color baseColor = roomBaseColors != null && i < roomBaseColors.Length ? roomBaseColors[i] : normalColor;
                roomImages[i].color = visitedRooms.Contains(i) ? visitedColor : baseColor;
            }
        
            if (index >= 0 && index < roomImages.Length && roomImages[index] != null)
            {
                roomImages[index].color = currentColor;
            }
        
            currentRoomIndex = index;
        }
        private void LateUpdate()
        {
            if (!built || player == null || playerMarker == null)
            {
                return;
            }

            Vector2 target = MapGrid.MarkerPosition(player.position, chunkSize, cellSize, cellGap, width, height);
            playerMarker.anchoredPosition = Vector2.Lerp(playerMarker.anchoredPosition, target, Time.deltaTime * 12f);
            HighlightCurrentRoom();
        }
    }
}
