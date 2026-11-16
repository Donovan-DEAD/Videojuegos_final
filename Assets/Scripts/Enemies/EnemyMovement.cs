using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Mueve al enemigo aplicando velocidades a su Rigidbody2D, con un multiplicador global de velocidad
    /// (modificable p. ej. por SlowEffect) y restricciones de terreno según su hábitat.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float terrainLookAhead = 0.35f;
        [SerializeField] private LayerMask barrierMask;

        private Rigidbody2D rb;
        private Collider2D bodyCollider;
        private MapGenerator mapGenerator;
        private EnemyHabitat habitat = EnemyHabitat.Ground;

        /// <summary>Factor aplicado a todos los desplazamientos del enemigo.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>True si el último intento de movimiento quedó bloqueado por el terreno.</summary>
        public bool LastMoveBlocked { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mapGenerator = FindAnyObjectByType<MapGenerator>();
            bodyCollider = GetComponent<Collider2D>();
            if (barrierMask.value == 0) barrierMask = LayerMask.GetMask("Walls");
        }

        /// <summary>Define el terreno que puede pisar el enemigo (lo llama EnemyAI).</summary>
        public void SetHabitat(EnemyHabitat value)
        {
            habitat = value;
        }

        /// <summary>
        /// Fija al enemigo como estacionario (cinemático) o dinámico. Un enemigo estacionario no puede
        /// ser empujado por el jugador ni se mueve por fuerzas externas.
        /// </summary>
        public void SetStationary(bool value)
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            rb.bodyType = value ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            if (value)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        /// <summary>Terreno de la celda actual (Void si no hay MapGenerator).</summary>
        public TerrainType CurrentTerrain
        {
            get
            {
                if (mapGenerator == null || mapGenerator.Tileset == null || mapGenerator.TargetTilemap == null)
                {
                    return TerrainType.Void;
                }

                return mapGenerator.GetTerrain(rb.position);
            }
        }

        /// <summary>True si la celda actual es agua.</summary>
        public bool IsOnWater => CurrentTerrain == TerrainType.Water;

        /// <summary>True si la celda actual es del terreno indicado.</summary>
        public bool IsOnTerrain(TerrainType terrain)
        {
            return CurrentTerrain == terrain;
        }

        /// <summary>True si el enemigo se está desplazando.</summary>
        public bool IsMoving => rb != null && rb.linearVelocity.sqrMagnitude > 0.0001f;

        /// <summary>Se mueve en la dirección dada a la velocidad indicada.</summary>
        public void MoveDirection(Vector2 direction, float speed)
        {
            ApplyMovement(direction, speed);
        }

        /// <summary>Avanza hacia un punto, deteniéndose si ya está sobre él.</summary>
        public void MoveTowards(Vector3 target, float speed)
        {
            Vector2 direction = (Vector2)(target - transform.position);
            if (direction.sqrMagnitude < 0.0001f)
            {
                Stop();
                return;
            }

            ApplyMovement(direction, speed);
        }

        /// <summary>Fija directamente la velocidad del Rigidbody2D.</summary>
        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
            LastMoveBlocked = false;
        }

        /// <summary>Detiene el movimiento.</summary>
        public void Stop()
        {
            rb.linearVelocity = Vector2.zero;
            LastMoveBlocked = false;
        }

        /// <summary>True si la celda actual es un muro (para que un volador no se estacione dentro).</summary>
        public bool IsOnWallCell()
        {
            if (mapGenerator == null || mapGenerator.Tileset == null || mapGenerator.TargetTilemap == null)
            {
                return false;
            }

            return mapGenerator.GetTerrain(rb.position) == TerrainType.Wall;
        }

        private void ApplyMovement(Vector2 direction, float speed)
        {
            Vector2 normalized = direction.normalized;
            if (TryVelocity(normalized, speed))
            {
                return;
            }

            Vector2 horizontal = new Vector2(normalized.x, 0f);
            if (Mathf.Abs(horizontal.x) > 0.001f && TryVelocity(horizontal.normalized, speed))
            {
                return;
            }

            Vector2 vertical = new Vector2(0f, normalized.y);
            if (Mathf.Abs(vertical.y) > 0.001f && TryVelocity(vertical.normalized, speed))
            {
                return;
            }

            rb.linearVelocity = Vector2.zero;
            LastMoveBlocked = true;
        }

        private bool TryVelocity(Vector2 direction, float speed)
        {
            Vector2 next = rb.position + direction * terrainLookAhead;
            if (!CanStandAt(next) || BlockedByCollider(direction))
            {
                return false;
            }

            rb.linearVelocity = direction * speed * SpeedMultiplier;
            LastMoveBlocked = false;
            return true;
        }

        /// <summary>True si hay una barrera (collider en Walls) delante del enemigo.</summary>
        private bool BlockedByCollider(Vector2 direction)
        {
            if ((habitat & EnemyHabitat.Flying) != 0 || bodyCollider == null || barrierMask.value == 0)
            {
                return false;
            }

            float radius = bodyCollider.bounds.extents.x;
            if (bodyCollider is CircleCollider2D circle)
            {
                radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            }

            Vector2 forward = direction.normalized;
            Vector2 side = new Vector2(-forward.y, forward.x);
            float originOffset = Mathf.Max(0.1f, radius * 0.5f);
            float rayDistance = terrainLookAhead + radius * 0.5f;
            Vector2 originA = rb.position + forward * originOffset + side * radius * 0.4f;
            Vector2 originB = rb.position + forward * originOffset - side * radius * 0.4f;

            RaycastHit2D hitA = Physics2D.Raycast(originA, forward, rayDistance, barrierMask);
            RaycastHit2D hitB = Physics2D.Raycast(originB, forward, rayDistance, barrierMask);
            return hitA.collider != null || hitB.collider != null;
        }

        private bool CanStandAt(Vector2 position)
        {
            if ((habitat & EnemyHabitat.Flying) != 0)
            {
                return true;
            }

            if (mapGenerator == null || mapGenerator.Tileset == null || mapGenerator.TargetTilemap == null)
            {
                return true;
            }

            TerrainType terrain = mapGenerator.GetTerrain(position);
            if ((habitat & EnemyHabitat.Ground) != 0 && terrain == TerrainType.Floor)
            {
                return true;
            }

            if ((habitat & EnemyHabitat.Water) != 0 && terrain == TerrainType.Water)
            {
                return true;
            }

            return false;
        }
    }
}
