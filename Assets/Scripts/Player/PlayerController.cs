using UnityEngine;
using UnityEngine.InputSystem;

namespace MadnessDreams
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D rb;
        private Vector2 movement;

        public bool IsMoving => movement.sqrMagnitude > 0.0001f;
        public bool ControlEnabled { get; set; } = true;

        public void SetMoveSpeed(float value)
        {
            moveSpeed = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Time.timeScale == 0f)
            {
                movement = Vector2.zero;
                return;
            }

            movement = ReadMovement();
        }

        private static Vector2 ReadMovement()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                Vector2 keys = Vector2.zero;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) keys.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) keys.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) keys.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) keys.y += 1f;
                return Vector2.ClampMagnitude(keys, 1f);
            }

            Vector2 axes = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            return Vector2.ClampMagnitude(axes, 1f);
        }

        private void FixedUpdate()
        {
            if (!ControlEnabled)
            {
                return;
            }

            rb.linearVelocity = movement * moveSpeed;
        }
    }
}
