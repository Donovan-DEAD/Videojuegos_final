using UnityEngine;
using UnityEngine.InputSystem;

namespace MadnessDreams
{
    /// <summary>Mob comerciante (no enemigo): interactúa con el jugador y abre el menú de compra.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class MerchantController : MonoBehaviour
    {
        [SerializeField] private MerchantMenuUI menu;
        [SerializeField] private GameObject hint;
        [SerializeField] private float interactRadius = 1.8f;

        private bool playerInRange;
        private Transform player;

        private void Start()
        {
            if (menu == null) menu = FindAnyObjectByType<MerchantMenuUI>(FindObjectsInactive.Include);
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            if (hint != null) hint.SetActive(false);
        }

        private void Update()
        {
            if (!playerInRange && player != null && Vector2.Distance(transform.position, player.position) <= interactRadius)
            {
                playerInRange = true;
                if (hint != null) hint.SetActive(true);
            }

            if (!playerInRange || menu == null) return;
            if (PressedInteract()) menu.Open();
        }

        private static bool PressedInteract()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tKey.wasPressedThisFrame) return true;
            try { return Input.GetKeyDown(KeyCode.T); } catch { return false; }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            if (hint != null) hint.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            if (hint != null) hint.SetActive(false);
        }
    }
}
