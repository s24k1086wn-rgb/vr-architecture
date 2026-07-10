using UnityEngine;
using UnityEngine.InputSystem;

namespace SafetyTraining.Player
{
    /// <summary>
    /// Handles FPS raycast interactions, allowing players to look at a hazard and click
    /// (e.g. Left Click or Interact button) to resolve/remedy it.
    /// </summary>
    public class FPSInteractor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactRange = 5.0f;
        [SerializeField] private LayerMask interactableLayers;

        [Header("Crosshair UI (Optional)")]
        [SerializeField] private GameObject crosshairActiveVisual;

        private InputAction interactAction;
        private bool isLookingAtHazard = false;
        private HazardBase currentTargetHazard = null;

        private void Start()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    playerCamera = Camera.main;
                }
            }

            // Cache the Interact action from the project-wide Input System
            if (InputSystem.actions != null)
            {
                interactAction = InputSystem.actions.FindAction("Interact");
                if (interactAction == null)
                {
                    // Fallback to "Attack" if "Interact" is not assigned (usually Left Click)
                    interactAction = InputSystem.actions.FindAction("Attack");
                }
            }

            if (interactAction != null)
            {
                interactAction.performed += OnInteractPressed;
            }
            else
            {
                Debug.LogWarning("[FPSInteractor] No 'Interact' or 'Attack' action found in project-wide actions.");
            }
        }

        private void Update()
        {
            PerformRaycastCheck();
        }

        private void PerformRaycastCheck()
        {
            if (playerCamera == null) return;

            // Raycast from the center of the screen
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, interactRange, interactableLayers);

            if (hitSomething)
            {
                HazardBase hazard = hit.collider.GetComponentInParent<HazardBase>();
                if (hazard != null && hazard.IsTriggered && !hazard.IsResolved)
                {
                    // Player is looking at an active, unresolved hazard
                    if (!isLookingAtHazard || currentTargetHazard != hazard)
                    {
                        isLookingAtHazard = true;
                        currentTargetHazard = hazard;
                        OnHoverEnter(hazard);
                    }
                    return;
                }
            }

            // If we didn't hit a valid hazard, clear state
            if (isLookingAtHazard)
            {
                OnHoverExit();
            }
        }

        private void OnHoverEnter(HazardBase hazard)
        {
            // Activate crosshair visual indicator (e.g. changes color or size to show interaction is possible)
            if (crosshairActiveVisual != null)
            {
                crosshairActiveVisual.SetActive(true);
            }
            // Optional: Show prompt UI (e.g. "Press Left Click to Fix")
        }

        private void OnHoverExit()
        {
            isLookingAtHazard = false;
            currentTargetHazard = null;

            if (crosshairActiveVisual != null)
            {
                crosshairActiveVisual.SetActive(false);
            }
        }

        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (isLookingAtHazard && currentTargetHazard != null)
            {
                Debug.Log($"[FPSInteractor] Corrective action taken on: {currentTargetHazard.HazardName}");
                
                // Resolve the hazard! This triggers events, plays resolve effects, and adds scores.
                currentTargetHazard.ResolveHazard();
                
                // Refresh hover state since it is now resolved
                OnHoverExit();
            }
        }

        private void OnDestroy()
        {
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPressed;
            }
        }
    }
}
