using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Utilidades de terreno para spawns: compatibilidad hábitat/terreno y búsqueda de una
    /// posición válida alrededor de un centro (la usan el spawner y los jefes al invocar).
    /// </summary>
    public static class TerrainSpawnUtility
    {
        /// <summary>True si el terreno es válido para el hábitat indicado.</summary>
        public static bool IsCompatible(EnemyHabitat habitat, TerrainType terrain)
        {
            if (terrain == TerrainType.Wall || terrain == TerrainType.Void)
            {
                return false;
            }

            if ((habitat & EnemyHabitat.Flying) != 0)
            {
                return true;
            }

            bool ground = (habitat & EnemyHabitat.Ground) != 0 && terrain == TerrainType.Floor;
            bool water = (habitat & EnemyHabitat.Water) != 0 && terrain == TerrainType.Water;
            return ground || water;
        }

        /// <summary>Lee el hábitat del prefab (EnemyAI); Ground si no lo encuentra.</summary>
        public static EnemyHabitat GetHabitat(GameObject prefab)
        {
            if (prefab == null)
            {
                return EnemyHabitat.Ground;
            }

            EnemyAI ai = prefab.GetComponent<EnemyAI>();
            if (ai != null && ai.Stats != null)
            {
                return ai.Stats.Habitat;
            }

            return EnemyHabitat.Ground;
        }

        /// <summary>
        /// Busca una posición compatible alrededor de <paramref name="center"/>; si no la encuentra
        /// (o no hay MapGenerator), devuelve el centro.
        /// </summary>
        public static Vector3 ResolveSpawnPosition(GameObject prefab, Vector3 center, float minRadius, float maxRadius)
        {
            MapGenerator map = Object.FindAnyObjectByType<MapGenerator>();
            EnemyHabitat habitat = GetHabitat(prefab);

            if (map == null)
            {
                Vector2 fallbackOffset = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                return center + (Vector3)fallbackOffset;
            }

            for (int i = 0; i < 12; i++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                Vector3 candidate = center + (Vector3)offset;
                if (IsCompatible(habitat, map.GetTerrain(candidate)))
                {
                    return candidate;
                }
            }

            return center;
        }

        /// <summary>
        /// Busca en espiral la celda de terreno <paramref name="target"/> más cercana a
        /// <paramref name="origin"/> y devuelve su centro en mundo.
        /// </summary>
        public static bool FindNearestTerrain(Vector3 origin, TerrainType target, int radius, out Vector3 position)
        {
            position = origin;
            MapGenerator map = Object.FindAnyObjectByType<MapGenerator>();
            if (map == null || map.TargetTilemap == null || map.Tileset == null)
            {
                return false;
            }

            Vector3Int center = map.TargetTilemap.WorldToCell(origin);
            int max = Mathf.Clamp(radius, 1, 30);

            for (int r = 0; r <= max; r++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r)
                        {
                            continue;
                        }

                        Vector3Int cell = new Vector3Int(center.x + dx, center.y + dy, center.z);
                        if (map.GetTerrain(cell) == target)
                        {
                            position = map.TargetTilemap.GetCellCenterWorld(cell);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
