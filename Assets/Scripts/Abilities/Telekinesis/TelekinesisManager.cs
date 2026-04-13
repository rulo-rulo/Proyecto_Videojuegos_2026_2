using UnityEngine;
using System.Collections.Generic;

namespace Telekinesis
{
    public class TelekinesisManager : MonoBehaviour
    {
        [SerializeField] private TelekinesisConfig            config;
        [SerializeField] private Transform                    playerTransform;
        [SerializeField] private Camara                       camara;
        [SerializeField] private TelekinesisOutlineController outlineController;
        [SerializeField] private PlayerMovimiento playerMovimiento;

        private TelekinesisInputHandler    inputHandler;
        private TelekinesisState           currentState = TelekinesisState.Idle;
        private MovableObject              currentTarget;
        private List<MovableObject>        nearbyObjects  = new List<MovableObject>();
        private float                      scanRefreshTimer;
        private Transform                  originalCameraTarget;

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
            currentState  = TelekinesisState.Scanning;

            if (nearbyObjects.Count > 0)
                outlineController.ShowOutlines(nearbyObjects, currentTarget);

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

            Debug.Log($"[Telekinesis] Apuntando a: {currentTarget.gameObject.name}");
        }

        private void ApplyForce()
        {
            if (currentTarget == null) return;

            Vector3 direction = GetWorldDirection(inputHandler.LastDirection);
            currentTarget.ApplyForce(direction, config.pushForce);

            Cancel();
        }

        private void Cancel()
        {
            AbilityManager.Instance.ClearAbility(this);
            
            if (currentState == TelekinesisState.Idle) return;

            outlineController.HideOutlines();
            camara.SetTarget(originalCameraTarget);

            playerMovimiento.enabled = true;

            currentTarget = null;
            currentState  = TelekinesisState.Idle;

            Debug.Log("[Telekinesis] Habilidad cancelada.");
        }

        // -------------------------------------------------- Dirección

        private Vector3 GetWorldDirection(Vector3 localDir)
        {
            // Convierte la dirección local (flechas) a dirección relativa a la cámara
            Transform cam = Camera.main.transform;

            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

            if (localDir == Vector3.forward) return camForward;
            if (localDir == Vector3.back)    return -camForward;
            if (localDir == Vector3.right)   return camRight;
            if (localDir == Vector3.left)    return -camRight;

            return cam.up; // Vector3.up por defecto → arriba relativo a la cámara
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
            MovableObject nearest  = null;
            float         bestDist = float.MaxValue;

            foreach (MovableObject obj in objects)
            {
                float dist = Vector3.Distance(playerTransform.position, obj.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest  = obj;
                }
            }

            return nearest;
        }
    }
}
