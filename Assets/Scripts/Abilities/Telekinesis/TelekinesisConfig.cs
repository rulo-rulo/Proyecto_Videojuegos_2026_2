using UnityEngine;

namespace Telekinesis
{
    [CreateAssetMenu(fileName = "TelekinesisConfig", menuName = "Telekinesis/Config")]
    public class TelekinesisConfig : ScriptableObject
    {
        [Header("Detección")]
        [Tooltip("Radio máximo para detectar objetos movibles")]
        public float detectionRadius = 15f;

        [Header("Fuerza")]
        [Tooltip("Fuerza aplicada al objeto al confirmar")]
        public float pushForce = 10f;

        [Header("Cámara")]
        [Tooltip("Velocidad a la que la cámara se desplaza al objeto")]
        public float cameraSpeed = 5f;
    }
}
