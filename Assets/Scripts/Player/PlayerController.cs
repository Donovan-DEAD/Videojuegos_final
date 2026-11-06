using UnityEngine;
using UnityEngine.InputSystem;

namespace MadnessDreams
{
    /// <summary>
    /// Lee la entrada de movimiento y aplica la velocidad al Rigidbody2D del jugador;
    /// el control puede suspenderse temporalmente (p. ej. durante un dash).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float waterSpeedMultiplier = 0.6f;

        private Rigidbody2D rb;
        private MapGenerator mapGenerator;
        private Vector2 movement;

        /// <summary>Indica si hay entrada de movimiento en el frame actual.</summary>
        public bool IsMoving => movement.sqrMagnitude > 0.0001f;
        /// <summary>Dirección de entrada actual (normalizada o cero).</summary>
        public Vector2 MoveDirection => movement;
        /// <summary>Permite habilitar o suspender el control de movimiento.</summary>
        public bool ControlEnabled { get; set; } = true;

        /// <summary>Ajusta la velocidad de movimiento (mínimo 0.1).</summary>
        public void SetMoveSpeed(float value)
        {
            moveSpeed = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mapGenerator = FindAnyObjectByType<MapGenerator>();
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

            rb.linearVelocity = movement * CurrentSpeed();
        }

        private float CurrentSpeed()
        {
            if (waterSpeedMultiplier >= 1f || mapGenerator == null || mapGenerator.Tileset == null || mapGenerator.TargetTilemap == null)
            {
                return moveSpeed;
            }

            return mapGenerator.GetTerrain(rb.position) == TerrainType.Water
                ? moveSpeed * waterSpeedMultiplier
                : moveSpeed;
        }
    }
}
