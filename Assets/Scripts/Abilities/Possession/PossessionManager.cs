using UnityEngine;
using System.Collections.Generic;

namespace Possession
{
    public class PossessionManager : MonoBehaviour
    {
        [Header("Referencias de Configuración")]
        [SerializeField] private PossessionConfig config;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private OutlineController outlineController;
        [SerializeField] private Camara camara;
        [SerializeField] private PlayerMovimiento playerMovimiento;
        [SerializeField] private CharacterController playerController;
        [SerializeField] private GameObject playerModel;
        [SerializeField] private float possessionDuration = 5f;

        [Header("Sistema de Cooldown UI")]
        [SerializeField] private HabilidadCooldown uiCooldown;

        private InputHandler inputHandler;
        private PossessionState currentState = PossessionState.Free;
        private IPossessable currentTarget;
        private List<IPossessable> nearbyPossessables = new List<IPossessable>();
        private float scanRefreshTimer;
        private float possessionTimer;
        private bool isTimerRunning = false;
        private Collider[] playerColliders;

        public PossessionState CurrentState => currentState;
        public float PossessionTimer => possessionTimer;
        public float PossessionDuration => possessionDuration;
        public IPossessable CurrentTarget => currentTarget;

        // -------------------------------------------------- Unity Lifecycle

        private void Awake()
        {
            inputHandler = GetComponent<InputHandler>();
            inputHandler.OnPossessionKeyPressed += HandlePossessionInput;
            playerColliders = playerTransform.GetComponentsInChildren<Collider>();
        }

        private void OnDestroy()
        {
            inputHandler.OnPossessionKeyPressed -= HandlePossessionInput;
        }

        private void Update()
        {
            // Gestión del tiempo de posesión (cuánto tiempo estamos dentro del objeto)
            if (currentState == PossessionState.Possessing && isTimerRunning)
            {
                possessionTimer -= Time.deltaTime;

                if (possessionTimer <= 0f)
                {
                    isTimerRunning = false;
                    Depossess();
                    return;
                }
            }

            if (currentState != PossessionState.Scanning) return;

            // Lógica de escaneo de objetos cercanos
            scanRefreshTimer += Time.deltaTime;
            if (scanRefreshTimer >= 0.1f)
            {
                scanRefreshTimer = 0f;
                List<IPossessable> newNearby = FindAllNearby();

                bool listChanged = newNearby.Count != nearbyPossessables.Count;
                nearbyPossessables = newNearby;

                if (nearbyPossessables.Count == 0)
                {
                    outlineController.HideOutlines();
                    currentTarget = null;
                    return;
                }

                if (listChanged)
                {
                    IPossessable nearest = FindNearestFrom(nearbyPossessables);
                    currentTarget = nearest;
                    outlineController.ShowOutlines(nearbyPossessables, currentTarget);
                    return;
                }
            }

            if (nearbyPossessables.Count == 0) return;

            IPossessable newNearest = FindNearestFrom(nearbyPossessables);
            if (newNearest == null || newNearest == currentTarget) return;

            currentTarget = newNearest;
            outlineController.ShowOutlines(nearbyPossessables, currentTarget);
        }

        // -------------------------------------------------- Lógica de Input

        private void HandlePossessionInput()
        {
            if (uiCooldown != null && uiCooldown.EstaEnEnfriamiento)
            {
                Debug.Log("[Possession] La habilidad aún no está lista.");
                return;
            }

            if (!AbilityManager.Instance.CanUseAbility(this)) return;

            switch (currentState)
            {
                case PossessionState.Free:
                    EnterScanning();
                    break;

                case PossessionState.Scanning:
                    // Si estamos apuntando a algo, lo poseemos. 
                    // Si no, usamos la Z para apagar el escáner.
                    if (currentTarget != null)
                    {
                        TryPossess();
                    }
                    else
                    {
                        CancelScanning();
                    }
                    break;

                case PossessionState.Possessing:
                    // Si volvemos a pulsar Z mientras poseemos, salimos.
                    isTimerRunning = false;
                    Depossess();
                    break;
            }
        }

        // -------------------------------------------------- Gestión de Estados

        private void CancelScanning()
        {
            // Apagamos el icono si decidimos cancelar el escaneo
            if (uiCooldown != null) uiCooldown.EstablecerUsoActivo(false);

            AbilityManager.Instance.ClearAbility(this);

            outlineController.HideOutlines();
            currentTarget = null;
            currentState = PossessionState.Free;
            Debug.Log("[Possession] Escaneo cancelado.");
        }

        private void EnterScanning()
        {
            if (uiCooldown != null) uiCooldown.EstablecerUsoActivo(true);

            AbilityManager.Instance.RegisterAbility(this);

            nearbyPossessables = FindAllNearby();
            currentTarget = FindNearestFrom(nearbyPossessables);
            currentState = PossessionState.Scanning;

            if (nearbyPossessables.Count > 0)
                outlineController.ShowOutlines(nearbyPossessables, currentTarget);

            Debug.Log("[Possession] Escaneo iniciado.");
        }

        private void TryPossess()
        {
            if (currentTarget == null) return;

            currentState = PossessionState.Possessing;
            outlineController.HideOutlines();
            playerMovimiento.enabled = false;
            playerController.enabled = false;
            playerModel.SetActive(false);

            foreach (Collider col in playerColliders)
            {
                col.enabled = false;
            }

            camara.SetTarget(currentTarget.Transform);

            float speed = config.GetSpeedForWeight(currentTarget.WeightClass);
            currentTarget.OnPossess(speed);

            possessionTimer = possessionDuration;
            isTimerRunning = true;

            Debug.Log($"[Possession] Poseyendo objeto...");
        }

        private void Depossess()
        {
            if (uiCooldown != null) uiCooldown.EstablecerUsoActivo(false);

            AbilityManager.Instance.ClearAbility(this);

            if (currentTarget == null) return;

            currentTarget.OnDepossess();

            Vector3 spawnPosition = SpawnFinder.FindFreePosition(
                currentTarget.Transform.position,
                config.spawnSearchRadius
            );

            // Desactivamos CC temporalmente para mover el transform físicamente
            playerController.enabled = false;
            playerTransform.position = spawnPosition;
            playerController.enabled = true;

            playerModel.SetActive(true);
            playerMovimiento.enabled = true;
            camara.SetTarget(playerTransform);

            foreach (Collider col in playerColliders)
            {
                col.enabled = true;
            }

            playerModel.SetActive(true);
            playerMovimiento.enabled = true;
            camara.SetTarget(playerTransform);

            currentTarget = null;
            currentState = PossessionState.Free;

            if (uiCooldown != null)
            {
                uiCooldown.IniciarCooldown();
            }

            Debug.Log("[Possession] Jugador expulsado del objeto.");
        }

        // -------------------------------------------------- Métodos de Detección

        private List<IPossessable> FindAllNearby()
        {
            Collider[] hits = Physics.OverlapSphere(
                playerTransform.position,
                config.detectionRadius
            );

            List<IPossessable> result = new List<IPossessable>();

            foreach (Collider hit in hits)
            {
                // Buscamos el componente que implementa la interfaz possessable
                if (hit.TryGetComponent(out IPossessable candidate))
                    result.Add(candidate);
            }

            return result;
        }

        private IPossessable FindNearestFrom(List<IPossessable> possessables)
        {
            IPossessable nearest = null;
            float bestDist = float.MaxValue;

            foreach (IPossessable p in possessables)
            {
                float dist = Vector3.Distance(playerTransform.position, p.Transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = p;
                }
            }

            return nearest;
        }
    }
}