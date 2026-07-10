using UnityEngine;
using System;

namespace SafetyTraining
{
    /// <summary>
    /// Base class for all construction site hazards.
    /// Event-driven, optimized, and designed to prevent Update polling.
    /// </summary>
    public abstract class HazardBase : MonoBehaviour
    {
        [Header("Hazard Information")]
        [SerializeField] private string hazardID = "HZ_001";
        [SerializeField] private string hazardName = "General Hazard";
        [SerializeField, TextArea(3, 5)] private string description = "A dangerous situation on the construction site.";
        [SerializeField] private int dangerScore = 10;

        [Header("State")]
        [SerializeField] private bool triggerOnce = true;
        
        private bool isTriggered = false;
        private bool isResolved = false;

        // Events for UI, managers, or logging to subscribe to
        public event Action<HazardBase> OnHazardTriggered;
        public event Action<HazardBase> OnHazardResolved;
        public event Action<HazardBase> OnHazardReset;

        public string HazardID => hazardID;
        public string HazardName => hazardName;
        public string Description => description;
        public int DangerScore => dangerScore;
        public bool IsTriggered => isTriggered;
        public bool IsResolved => isResolved;

        /// <summary>
        /// Triggers the hazard. Override to add custom visual/audio/physics effects.
        /// </summary>
        public virtual void TriggerHazard()
        {
            if (triggerOnce && isTriggered) return;

            isTriggered = true;
            isResolved = false;

            OnTrigger();
            OnHazardTriggered?.Invoke(this);
            
            Debug.Log($"[Hazard] Triggered: {hazardName} ({hazardID})");
        }

        /// <summary>
        /// Resolves the hazard (e.g., player takes safety action, wears harness, stops machine).
        /// </summary>
        public virtual void ResolveHazard()
        {
            if (!isTriggered || isResolved) return;

            isResolved = true;
            
            OnResolve();
            OnHazardResolved?.Invoke(this);

            Debug.Log($"[Hazard] Resolved: {hazardName} ({hazardID})");
        }

        /// <summary>
        /// Resets the hazard to its initial state.
        /// </summary>
        public virtual void ResetHazard()
        {
            isTriggered = false;
            isResolved = false;
            
            OnReset();
            OnHazardReset?.Invoke(this);

            Debug.Log($"[Hazard] Reset: {hazardName} ({hazardID})");
        }

        // Subclasses implement these to define specific visual/audio/behavioral responses
        protected abstract void OnTrigger();
        protected virtual void OnResolve() { }
        protected virtual void OnReset() { }
    }
}
