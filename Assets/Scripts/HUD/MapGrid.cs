using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Utilidades estáticas para dibujar el mapa/minimapa (un rectángulo por cuarto o por celda) y calcular la posición del marcador del jugador.
    /// </summary>
    public static class MapGrid
    {
        /// <summary>
        /// Dibuja un rectángulo por cuarto (admite cuartos de varias celdas) y devuelve las imágenes creadas.
        /// </summary>
        public static Image[] BuildRooms(RectTransform container, List<RoomPlacement> rooms, int gridHeight, float cellSize, float cellGap,
            Color startColor, Color normalColor, Color bossColor)
        {
            Image[] images = new Image[rooms.Count];

            for (int i = 0; i < rooms.Count; i++)
            {
                RoomPlacement room = rooms[i];
                float w = room.sizeInCells.x * cellSize + (room.sizeInCells.x - 1) * cellGap;
                float h = room.sizeInCells.y * cellSize + (room.sizeInCells.y - 1) * cellGap;
                float x = room.originCell.x * (cellSize + cellGap);
                // UI coordinates start at the top, while world/grid Y grows upward.
                float y = (gridHeight - room.originCell.y - room.sizeInCells.y) * (cellSize + cellGap);

                Color color = room.role == CellRole.Start ? startColor : (room.role == CellRole.Boss ? bossColor : normalColor);
                images[i] = CreateRect(container, x, y, w, h, color);
            }

            return images;
        }

        /// <summary>
        /// Alternativa por celda (legacy), usada cuando no hay MapGenerator (p. ej. la escena Greybox); devuelve la matriz de imágenes.
        /// </summary>
        public static Image[,] Build(RectTransform container, int width, int height, float cellSize, float cellGap,
            Vector2Int startChunk, Vector2Int bossChunk, int bossRegion,
            Color startColor, Color normalColor, Color bossColor)
        {
            var cells = new Image[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float uiY = (height - 1 - y) * (cellSize + cellGap);
                    cells[x, y] = CreateRect(container, x * (cellSize + cellGap), uiY, cellSize, cellSize,
                        RoleColor(x, y, startChunk, bossChunk, bossRegion, startColor, normalColor, bossColor));
                }
            }

            return cells;
        }

        /// <summary>
        /// Devuelve el color de la celda según sea la sala de inicio, la región del jefe o una sala normal.
        /// </summary>
        public static Color RoleColor(int x, int y, Vector2Int startChunk, Vector2Int bossChunk, int bossRegion,
            Color startColor, Color normalColor, Color bossColor)
        {
            if (x == startChunk.x && y == startChunk.y)
            {
                return startColor;
            }

            if (x >= bossChunk.x && x < bossChunk.x + bossRegion && y >= bossChunk.y && y < bossChunk.y + bossRegion)
            {
                return bossColor;
            }

            return normalColor;
        }

        /// <summary>
        /// Convierte una posición de mundo en la posición de UI del marcador del jugador dentro de la cuadrícula.
        /// </summary>
        public static Vector2 MarkerPosition(Vector3 world, int chunkSize, float cellSize, float cellGap, int width, int height)
        {
            float cx = chunkSize > 0 ? world.x / chunkSize : 0f;
            float cy = chunkSize > 0 ? world.y / chunkSize : 0f;

            cx = Mathf.Clamp(cx, 0f, width);
            cy = Mathf.Clamp(cy, 0f, height);

            float uiY = -(height - cy) * (cellSize + cellGap);
            return new Vector2(cx * (cellSize + cellGap), uiY);
        }

        /// <summary>
        /// Destruye los hijos de <paramref name="container"/> salvo <paramref name="keep"/>, para reconstruir la cuadrícula.
        /// </summary>
        public static void ClearCells(RectTransform container, RectTransform keep)
        {
            if (container == null)
            {
                return;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Transform child = container.GetChild(i);
                if (keep != null && child == keep)
                {
                    continue;
                }

                Object.Destroy(child.gameObject);
            }
        }

        private static Image CreateRect(RectTransform container, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject("Room", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(container, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, -y);

            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}
