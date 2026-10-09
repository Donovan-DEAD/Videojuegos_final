using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    public class MapChunkCaptureEditor : EditorWindow
    {
        private Tilemap source;
        private MapChunkSO target;

        [MenuItem("Tools/Madness Dreams/Map Chunk Capture")]
        public static void Open()
        {
            GetWindow<MapChunkCaptureEditor>("Map Chunk Capture");
        }

        private void OnGUI()
        {
            source = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", source, typeof(Tilemap), true);
            target = (MapChunkSO)EditorGUILayout.ObjectField("Target MapChunkSO", target, typeof(MapChunkSO), false);

            bool canCapture = source != null && target != null;
            using (new EditorGUI.DisabledScope(!canCapture))
            {
                if (GUILayout.Button("Capture"))
                {
                    Capture();
                }
            }
        }

        private void Capture()
        {
            BoundsInt bounds = source.cellBounds;
            Vector2Int size = new Vector2Int(bounds.size.x, bounds.size.y);

            TileBase[] flattened = source.GetTilesBlock(bounds);

            target.SetSize(size);
            target.SetTileData(flattened);

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            Debug.Log($"Captured {size.x}x{size.y} tiles into {target.name}.");
        }

    }
}
