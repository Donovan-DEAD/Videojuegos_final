using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Mantiene el collider de límites del mapa en sincronía con el mapa generado y se lo
    /// asigna al confiner 2D de la cámara para que esta no muestre fuera del mundo.
    /// </summary>
    public class CameraConfiner : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private CinemachineConfiner2D confiner;
        [SerializeField] private Tilemap tilemap;

        private BoxCollider2D boundsCollider;

        private void OnEnable()
        {
            if (mapGenerator == null) mapGenerator = FindAnyObjectByType<MapGenerator>();
            if (confiner == null) confiner = FindAnyObjectByType<CinemachineConfiner2D>();
            if (tilemap == null && mapGenerator != null) tilemap = mapGenerator.TargetTilemap;

            if (mapGenerator != null) mapGenerator.Generated += OnGenerated;
            OnGenerated();
        }

        private void OnDisable()
        {
            if (mapGenerator != null) mapGenerator.Generated -= OnGenerated;
        }

        private void OnGenerated()
        {
            Tilemap target = tilemap != null ? tilemap : (mapGenerator != null ? mapGenerator.TargetTilemap : null);
            if (target == null || confiner == null)
            {
                return;
            }

            Bounds bounds = target.localBounds;
            if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            {
                return;
            }

            EnsureBoundsCollider(target.transform);

            boundsCollider.transform.SetParent(target.transform, false);
            boundsCollider.transform.localPosition = bounds.center;
            boundsCollider.transform.localRotation = Quaternion.identity;
            boundsCollider.transform.localScale = Vector3.one;
            boundsCollider.size = new Vector2(bounds.size.x, bounds.size.y);
            boundsCollider.offset = Vector2.zero;

            confiner.BoundingShape2D = boundsCollider;
            confiner.InvalidateBoundingShapeCache();
            confiner.InvalidateLensCache();
        }

        private void EnsureBoundsCollider(Transform parent)
        {
            if (boundsCollider != null)
            {
                return;
            }

            GameObject go = new GameObject("CameraBounds");
            go.transform.SetParent(parent, false);
            boundsCollider = go.AddComponent<BoxCollider2D>();
            boundsCollider.enabled = false;
        }
    }
}
