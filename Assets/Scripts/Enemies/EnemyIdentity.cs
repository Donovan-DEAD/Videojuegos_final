using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Identificación visual de un enemigo: una etiqueta flotante con su nombre. Se oculta al morir.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyIdentity : MonoBehaviour
    {
        private static Font labelFont;

        private SpriteRenderer body;
        private TextMesh label;
        private TextMesh labelShadow;

        /// <summary>Aplica el nombre del arquetipo, creando la etiqueta si falta.</summary>
        public void Apply(EnemyStatsSO stats)
        {
            if (stats == null)
            {
                return;
            }

            body = GetComponent<SpriteRenderer>();
            EnsureLabel(stats.DisplayName, stats.IdentityColor);
        }

        /// <summary>Muestra u oculta la etiqueta (se oculta al morir).</summary>
        public void SetVisible(bool visible)
        {
            if (label != null) label.gameObject.SetActive(visible);
            if (labelShadow != null) labelShadow.gameObject.SetActive(visible);
        }

        private int BodyBaseOrder()
        {
            var bodySort = GetComponent<YSortOrder>();
            return bodySort != null ? bodySort.BaseOrder : 0;
        }

        private void EnsureLabel(string text, Color color)
        {
            if (label == null)
            {
                labelShadow = CreateLabel("NameLabelShadow", new Color(0f, 0f, 0f, 0.85f), 200);
                labelShadow.transform.localPosition = new Vector3(0.02f, 0.87f, 0.01f);

                label = CreateLabel("NameLabel", color, 200);
                label.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            }

            label.text = text;
            label.color = color;
            labelShadow.text = text;
        }

        private TextMesh CreateLabel(string objectName, Color color, int orderOffset)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(transform, false);

            TextMesh mesh = go.AddComponent<TextMesh>();
            mesh.text = string.Empty;
            mesh.font = GetFont();
            mesh.fontSize = 48;
            mesh.characterSize = 0.07f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mesh.font.material;
                renderer.sortingLayerID = body != null ? body.sortingLayerID : 0;
            }

            var labelSort = go.AddComponent<YSortOrder>();
            labelSort.SetBaseOrder(BodyBaseOrder() + orderOffset);

            return mesh;
        }

        private static Font GetFont()
        {
            if (labelFont != null) return labelFont;

            try { labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (System.Exception) { labelFont = null; }

            if (labelFont == null)
            {
                try { labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch (System.Exception) { labelFont = null; }
            }

            return labelFont;
        }
    }
}
