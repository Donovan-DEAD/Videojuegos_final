using UnityEngine;

namespace MadnessDreams
{
    public class SlowEffect : MonoBehaviour
    {
        [SerializeField] private EnemyMovement movement;

        private float timer;
        private float multiplier = 1f;

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<EnemyMovement>();
            }
        }

        public void Apply(float slowMultiplier, float duration)
        {
            multiplier = Mathf.Clamp(slowMultiplier, 0.05f, 1f);
            timer = duration;

            if (movement != null)
            {
                movement.SpeedMultiplier = multiplier;
            }
        }

        private void Update()
        {
            if (timer <= 0f)
            {
                return;
            }

            timer -= Time.deltaTime;

            if (timer <= 0f && movement != null)
            {
                movement.SpeedMultiplier = 1f;
            }
        }
    }
}
