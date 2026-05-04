using UnityEngine;
using System.Collections.Generic;

namespace Telekinesis
{
    public class TelekinesisManager : MonoBehaviour
    {
        [SerializeField] private TelekinesisConfig config;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Camara camara;
        [SerializeField] private TelekinesisOutlineController outlineController;
        [SerializeField] private PlayerMovimiento playerMovimiento;

        [Header("Sistema de Cooldown UI")]
        [SerializeField] private HabilidadCooldown uiCooldown;

        private TelekinesisInputHandler inputHandler;
        private TelekinesisState currentState = TelekinesisState.Idle;
        private MovableObject currentTarget;
        private List<MovableObject> nearbyObjects = new List<MovableObject>();
        private float scanRefreshTimer;
        private Transform originalCameraTarget;

        // -------------------------------------------------- Unity

        private void Awake()
        {
            inputHandler = GetComponent<TelekinesisInputHandler>();
            inputHandler.OnActionKeyPressed += HandleActionInput;
            inputHandler.OnCancelKeyPressed += Cancel;
        }

        private void OnDestroy()
        {
            inputHandler.OnActionKeyPressed -= HandleActionInput;
            inputHandler.OnCancelKeyPressed -= Cancel;
        }

        private void Update()
        {
            // 🔹 ACTUALIZAR FLECHA EN TIEMPO REAL (del Script 2)
            if (currentState == TelekinesisState.Aiming && currentTarget != null)
            {
                Vector3 direction = GetWorldDirection(inputHandler.LastDirection);
                currentTarget.UpdateArrow(direction);
            }

            // 🔹 ESCANEO (de ambos scripts)
            if (currentState != TelekinesisState.Scanning) return;

            scanRefreshTimer += Time.deltaTime;
            if (scanRefreshTimer >= 0.1f)
            {
                scanRefreshTimer = 0f;
                List<MovableObject> newNearby = FindAllNearby();

                bool listChanged = newNearby.Count != nearbyObjects.Count;
                nearbyObjects = newNearby;

                if (nearbyObjects.Count == 0)
                {
                    outlineController.HideOutlines();
                    currentTarget = null;
                    return;
                }

                if (listChanged)
                {
                    currentTarget = FindNearestFrom(nearbyObjects);
                    outlineController.ShowOutlines(nearbyObjects, currentTarget);
                    return;
                }
            }

            if (nearbyObjects.Count == 0) return;

            MovableObject nearest = FindNearestFrom(nearbyObjects);
            if (nearest == currentTarget) return;

            currentTarget = nearest;
            outlineController.ShowOutlines(nearbyObjects, currentTarget);
        }

        // -------------------------------------------------- Input

        private void HandleActionInput()
        {
            // 🔹 BLOQUEO POR COOLDOWN
            if (uiCooldown != null && uiCooldown.EstaEnEnfriamiento)
            {
                Debug.Log("[Telekinesis] En cooldown.");
                return;
            }

            // 🔹 BLOQUEO POR OTRA HABILIDAD
            if (!AbilityManager.Instance.CanUseAbility(this)) return;

            switch (currentState)
            {
                case TelekinesisState.Idle:
                    EnterScanning();
                    break;

                case TelekinesisState.Scanning:
                    EnterAiming();
                    break;

                case TelekinesisState.Aiming:
                    ApplyForce();
                    break;
            }
        }

        // -------------------------------------------------- Estados

        private void EnterScanning()
        {
            AbilityManager.Instance.RegisterAbility(this);

            nearbyObjects = FindAllNearby();
            currentTarget = FindNearestFrom(nearbyObjects);
            currentState = TelekinesisState.Scanning;

            if (nearbyObjects.Count > 0)
                outlineController.ShowOutlines(nearbyObjects, currentTarget);

            if (uiCooldown != null)
                uiCooldown.EstablecerUsoActivo(true);

            Debug.Log("[Telekinesis] Escaneando.");
        }

        private void EnterAiming()
        {
            if (currentTarget == null) return;

            currentState = TelekinesisState.Aiming;
            outlineController.HideOutlines();

            playerMovimiento.enabled = false;

            originalCameraTarget = playerTransform;
            camara.SetTarget(currentTarget.transform);

            // 🔹 Mostrar flecha (Script 2)
            Vector3 initialDirection = GetWorldDirection(inputHandler.LastDirection);
            currentTarget.ShowArrow(initialDirection);

            Debug.Log($"[Telekinesis] Apuntando a: {currentTarget.gameObject.name}");
        }

        private void ApplyForce()
        {
            if (currentTarget == null) return;

            Vector3 direction = GetWorldDirection(inputHandler.LastDirection);

            // 🔹 ocultar flecha
            currentTarget.HideArrow();

            currentTarget.ApplyForce(direction, config.pushForce);

            // 🔹 activar cooldown
            if (uiCooldown != null)
                uiCooldown.IniciarCooldown();

            Cancel();
        }

        private void Cancel()
        {
            if (uiCooldown != null)
                uiCooldown.EstablecerUsoActivo(false);

            AbilityManager.Instance.ClearAbility(this);

            if (currentState == TelekinesisState.Idle) return;

            if (currentTarget != null)
                currentTarget.HideArrow();

            outlineController.HideOutlines();
            camara.SetTarget(originalCameraTarget);

            playerMovimiento.enabled = true;

            currentTarget = null;
            currentState = TelekinesisState.Idle;

            Debug.Log("[Telekinesis] Cancelada.");
        }

        // -------------------------------------------------- Dirección (versión mejorada combinada)

        private Vector3 GetWorldDirection(Vector3 inputDir)
        {
            Transform cam = Camera.main.transform;
        
            // Direcciones de la cámara en plano horizontal
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;
        
            camForward.y = 0;
            camRight.y = 0;
        
            camForward.Normalize();
            camRight.Normalize();
        
            // Convertimos input a mundo RELATIVO A LA CÁMARA
            Vector3 direction = camForward * inputDir.z + camRight * inputDir.x;
        
            // Si no hay input → hacia delante de la cámara
            if (direction.sqrMagnitude < 0.01f)
                direction = camForward;
        
            return direction.normalized;
        }

        // -------------------------------------------------- Detección

        private List<MovableObject> FindAllNearby()
        {
            Collider[] hits = Physics.OverlapSphere(
                playerTransform.position,
                config.detectionRadius
            );

            List<MovableObject> result = new List<MovableObject>();

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out MovableObject obj))
                    result.Add(obj);
            }

            return result;
        }

        private MovableObject FindNearestFrom(List<MovableObject> objects)
        {
            MovableObject nearest = null;
            float bestDist = float.MaxValue;

            foreach (MovableObject obj in objects)
            {
                float dist = Vector3.Distance(playerTransform.position, obj.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = obj;
                }
            }

            return nearest;
        }
    }
}