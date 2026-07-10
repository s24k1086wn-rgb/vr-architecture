using UnityEngine;

namespace SafetyTraining
{
    /// <summary>
    /// A concrete implementation of a hazard that triggers when the player enters a physical area.
    /// Highly optimized: utilizes layer/tag caching and avoids Update checks.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerHazard : HazardBase
    {
        [Header("Trigger Settings")]
        [Tooltip("The tag of the player object. Usually 'Player'.")]
        [SerializeField] private string targetTag = "Player";
        
        [Tooltip("If true, checks if the collider has an XR Origin or CharacterController component.")]
        [SerializeField] private bool requirePlayerComponent = false;

        [Header("Visual/Audio feedback (Optional preview)")]
        [SerializeField] private ParticleSystem triggerVFX;
        [SerializeField] private AudioClip triggerSFX;
        [SerializeField] private AudioSource audioSource;

        private Collider triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            // Ensure the collider is set as a trigger
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            // Attempt to auto-grab AudioSource if not assigned but SFX is present
            if (audioSource == null && triggerSFX != null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 1. Tag check (fastest, most common)
            bool isPlayer = other.CompareTag(targetTag);

            // 2. Component check (fallback or additional security)
            if (!isPlayer && requirePlayerComponent)
            {
                // Checks for typical XR Origin/Rig components or standard player components
                // We check component names by string to avoid compile-time dependencies on uninstalled packages.
                if (other.GetComponent<CharacterController>() != null)
                {
                    isPlayer = true;
                }
                else
                {
                    Component[] components = other.GetComponentsInParent<Component>();
                    foreach (var c in components)
                    {
                        if (c != null && (c.GetType().Name == "XROrigin" || c.GetType().Name == "XRRig"))
                        {
                            isPlayer = true;
                            break;
                        }
                    }
                }
            }

            if (isPlayer)
            {
                TriggerHazard();
            }
        }

        protected override void OnTrigger()
        {
            // Play VFX
            if (triggerVFX != null)
            {
                triggerVFX.Play();
            }

            // Play SFX
            if (audioSource != null && triggerSFX != null)
            {
                audioSource.PlayOneShot(triggerSFX);
            }
        }

        protected override void OnResolve()
        {
            // Custom cleanup or change of visual indicators when resolved
        }

        protected override void OnReset()
        {
            // Reset particle systems or audio states if needed
            if (triggerVFX != null)
            {
                triggerVFX.Stop();
                triggerVFX.Clear();
            }
        }
    }
}
