using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Muestra la intro del miniboss al entrar el jugador y luego lo activa.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class MiniBossArenaTrigger : MonoBehaviour
    {
        [SerializeField] private BossIntroSO intro;
        [SerializeField] private GameObject miniboss;

        public void Setup(BossIntroSO introData, GameObject boss)
        {
            intro = introData;
            miniboss = boss;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            if (BossIntroController.Instance != null && intro != null)
            {
                BossIntroController.Instance.Show(intro, Activate);
            }
            else
            {
                Activate();
            }
        }

        private void Activate()
        {
            if (miniboss != null) miniboss.SetActive(true);
        }
    }
}
