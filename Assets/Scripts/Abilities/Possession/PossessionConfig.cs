using UnityEngine;

namespace Possession
{
    [CreateAssetMenu(fileName = "PossessionConfig", menuName = "Possession/Config")]
    public class PossessionConfig : ScriptableObject
    {
        [Header("Velocidades de movimiento")]
        public float lightSpeed  = 8f;
        public float mediumSpeed = 5f;
        public float heavySpeed  = 2.5f;

        [Header("Detección")]
        [Tooltip("Radio máximo para detectar objetos poseíbles")]
        public float detectionRadius = 15f;

        [Header("Reaparición")]
        [Tooltip("Radio de búsqueda de espacio libre al desposeerse")]
        public float spawnSearchRadius = 3f;

        public float GetSpeedForWeight(WeightClass weight) => weight switch
        {
            WeightClass.Light  => lightSpeed,
            WeightClass.Medium => mediumSpeed,
            WeightClass.Heavy  => heavySpeed,
            _                  => mediumSpeed
        };
    }
}