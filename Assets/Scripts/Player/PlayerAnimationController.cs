using UnityEngine;

namespace MadnessDreams
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string stateParameter = "State";
        [SerializeField] private PlayerController controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Health health;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<PlayerController>();
            if (combat == null) combat = GetComponent<PlayerCombat>();
            if (health == null) health = GetComponent<Health>();
        }

        public AnimationState CurrentState { get; private set; } = AnimationState.Idle;

        private void Update()
        {
            CurrentState = DetermineState();

            if (animator == null)
            {
                return;
            }

            animator.SetInteger(stateParameter, (int)CurrentState);
        }

        private AnimationState DetermineState()
        {
            if (health != null && health.IsDead)
            {
                return AnimationState.Death;
            }

            if (combat != null)
            {
                if (combat.IsRangedAttacking) return AnimationState.RangedAttack;
                if (combat.IsMeleeAttacking) return AnimationState.MeleeAttack;
            }

            if (controller != null && controller.IsMoving)
            {
                return AnimationState.Move;
            }

            return AnimationState.Idle;
        }
    }
}
