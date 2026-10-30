using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Toast que aparece al desbloquear una habilidad (fade con PrimeTween).</summary>
    public class UnlockToastWidget : MonoBehaviour
    {
        [SerializeField] private Text messageLabel;
        [SerializeField] private float showDuration = 2.5f;
        [SerializeField] private float fadeDuration = 0.5f;

        private PlayerAbilities abilities;
        private Sequence sequence;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            abilities = player != null ? player.GetComponent<PlayerAbilities>() : null;
            if (abilities != null) abilities.AbilityLearned += OnAbilityLearned;
            if (messageLabel != null) messageLabel.color = new Color(1f, 1f, 1f, 0f);
        }

        private void OnDestroy()
        {
            if (abilities != null) abilities.AbilityLearned -= OnAbilityLearned;
        }

        private void OnAbilityLearned(BossAbilitySO ability)
        {
            if (messageLabel != null)
            {
                messageLabel.text = "¡Habilidad desbloqueada: " + (ability != null ? ability.name : "") + "!";
            }
            Show();
        }

        private void Show()
        {
            if (sequence.isAlive) sequence.Stop();
            if (messageLabel == null) return;

            messageLabel.color = new Color(1f, 1f, 1f, 1f);
            sequence = Sequence.Create()
                .Chain(Tween.Alpha(messageLabel, 1f, fadeDuration))
                .ChainDelay(showDuration)
                .Chain(Tween.Alpha(messageLabel, 0f, fadeDuration))
                .ChainCallback(() => messageLabel.color = new Color(1f, 1f, 1f, 0f));
        }
    }
}
