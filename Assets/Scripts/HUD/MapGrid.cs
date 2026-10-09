using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public static class MapGrid
    {
        public static Image[,] Build(RectTransform container, int width, int height, float cellSize, float cellGap,
            Vector2Int startChunk, Vector2Int bossChunk, int bossRegion,
            Color startColor, Color normalColor, Color bossColor)
        {
            var cells = new Image[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells[x, y] = CreateCell(container, x, y, cellSize, cellGap,
                        RoleColor(x, y, startChunk, bossChunk, bossRegion, startColor, normalColor, bossColor));
                }
            }

            return cells;
        }

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

        public static Vector2 MarkerPosition(Vector3 world, int chunkSize, float cellSize, float cellGap, int width, int height)
        {
            float cx = chunkSize > 0 ? world.x / chunkSize : 0f;
            float cy = chunkSize > 0 ? world.y / chunkSize : 0f;

            cx = Mathf.Clamp(cx, 0f, width);
            cy = Mathf.Clamp(cy, 0f, height);

            return new Vector2(cx * (cellSize + cellGap), -cy * (cellSize + cellGap));
        }

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

        private static Image CreateCell(RectTransform container, int x, int y, float cellSize, float cellGap, Color color)
        {
            var go = new GameObject("Cell", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(container, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(cellSize, cellSize);
            rt.anchoredPosition = new Vector2(x * (cellSize + cellGap), -y * (cellSize + cellGap));

            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}
