using UnityEngine;

namespace Possession
{
    public static class SpawnFinder
    {
        private const int MaxAttempts = 16;

        public static Vector3 FindFreePosition(Vector3 origin, float searchRadius, float playerHeight = 1.8f)
        {
            float playerHalfHeight = playerHeight / 2f;
            float playerRadius     = 0.4f;

            for (int i = 0; i < MaxAttempts; i++)
            {
                float angle = i * (360f / MaxAttempts) * Mathf.Deg2Rad;

                Vector3 candidate = origin + new Vector3(
                    Mathf.Cos(angle) * searchRadius,
                    1f,
                    Mathf.Sin(angle) * searchRadius
                );

                // Ajusta la altura al suelo bajo ese punto
                if (Physics.Raycast(candidate + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
                    candidate.y = hit.point.y;

                if (IsPositionFree(candidate, playerHalfHeight, playerRadius))
                {
                    Debug.Log($"[SpawnFinder] Posición libre encontrada en: {candidate}");
                    return candidate;
                }
            }

            Debug.LogWarning("[SpawnFinder] No se encontró posición libre, usando fallback lateral.");
            return origin + new Vector3(searchRadius, 0f, 0f);
        }

        private static bool IsPositionFree(Vector3 center, float halfHeight, float radius)
        {
            Vector3 bottom = center + Vector3.up * radius;
            Vector3 top    = center + Vector3.up * (halfHeight * 2f - radius);

            return !Physics.CheckCapsule(bottom, top, radius);
        }
    }
}