using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Emite partículas combinadas de todos los efectos activos mientras el jugador se mueve.</summary>
    public class PlayerEffectTrail : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private float moveThreshold = 0.02f;
        [SerializeField] private int particlesPerEffect = 1;
        [SerializeField] private Color damageColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color speedColor = new Color(0.35f, 0.65f, 1f);
        [SerializeField] private Color healthColor = new Color(0.4f, 1f, 0.5f);

        private PlayerBuffs buffs;
        private Vector3 lastPos;
        private bool emitting;
        private readonly List<Color> active = new List<Color>(3);
        private ParticleSystem.EmitParams emitParams;

        private void Awake()
        {
            buffs = GetComponent<PlayerBuffs>();
            if (particles == null) particles = GetComponentInChildren<ParticleSystem>(true);
            lastPos = transform.position;
        }

        private void LateUpdate()
        {
            if (particles == null) return;

            Vector3 pos = transform.position;
            Vector3 delta = pos - lastPos;
            lastPos = pos;
            bool moving = delta.sqrMagnitude > moveThreshold * moveThreshold;

            CollectActive();
            bool shouldEmit = moving && active.Count > 0;

            if (shouldEmit)
            {
                if (!emitting) { particles.Play(); emitting = true; }
                emitParams.position = particles.transform.position;
                for (int i = 0; i < active.Count; i++)
                {
                    emitParams.startColor = active[i];
                    particles.Emit(emitParams, particlesPerEffect);
                }
            }
            else if (emitting)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                emitting = false;
            }
        }

        /// <summary>Ráfaga puntual del color indicado (p. ej. al usar una poción).</summary>
        public void Burst(Color color, int count)
        {
            if (particles == null || count <= 0) return;
            emitParams.position = particles.transform.position;
            emitParams.startColor = color;
            particles.Emit(emitParams, count);
            emitting = true;
        }

        private void CollectActive()
        {
            active.Clear();
            if (buffs == null) return;
            if (buffs.GetTimerNormalized(BuffKind.Speed) > 0f) active.Add(speedColor);
            if (buffs.GetTimerNormalized(BuffKind.Damage) > 0f) active.Add(damageColor);
            if (buffs.GetTimerNormalized(BuffKind.MaxHealth) > 0f) active.Add(healthColor);
        }
    }
}
