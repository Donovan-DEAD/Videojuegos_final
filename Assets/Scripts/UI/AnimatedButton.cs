using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Componente reutilizable tipo "React": añade hover/press con escala y tinte a cualquier Button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.06f;
        [SerializeField] private float pressScale = 0.94f;
        [SerializeField] private float duration = 0.15f;

        private RectTransform rect;
        private Graphic graphic;
        private Color baseColor;
        private Color hoverColor;
        private Color pressColor;
        private bool pointerInside;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            graphic = GetComponent<Graphic>();

            if (graphic != null)
            {
                baseColor = graphic.color;
                hoverColor = Color.Lerp(baseColor, Color.white, 0.22f);
                pressColor = Color.Lerp(baseColor, Color.black, 0.18f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            SetState(hoverScale, hoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            SetState(1f, baseColor);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetState(pressScale, pressColor);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetState(pointerInside ? hoverScale : 1f, pointerInside ? hoverColor : baseColor);
        }

        private void SetState(float scale, Color color)
        {
            if (rect != null) Tween.Scale(rect, Vector3.one * scale, duration, Ease.OutQuad);
            if (graphic != null) Tween.Color(graphic, color, duration);
        }
    }
}
