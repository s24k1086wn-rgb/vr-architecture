using UnityEngine;
using System;
using System.Collections.Generic;

namespace SafetyTraining
{
    public enum GameState
    {
        Setup,
        Playing,
        GameOver
    }

    /// <summary>
    /// Coordinates the safety training game loop, scoring, and hazard state tracking.
    /// Event-driven, low overhead, and VR-optimized.
    /// </summary>
    public class SafetyGameManager : MonoBehaviour
    {
        public static SafetyGameManager Instance { get; private set; }

        [Header("Game Flow Settings")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float timeLimit = 300f; // 5 minutes

        [Header("Scoring")]
        [SerializeField] private int baseScore = 100;
        private int currentScore;
        private float timeRemaining;
        private GameState state = GameState.Setup;

        private List<HazardBase> allHazards = new List<HazardBase>();
        private HashSet<string> triggeredHazardIDs = new HashSet<string>();

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<float> OnTimerUpdated;
        public event Action<HazardBase> OnHazardEncountered;

        public int CurrentScore => currentScore;
        public float TimeRemaining => timeRemaining;
        public GameState State => state;
        public IReadOnlyCollection<HazardBase> AllHazards => allHazards;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            currentScore = baseScore;
        }

        private void Start()
        {
            // Gather all hazards dynamically present in the scene
            FindAndRegisterHazards();

            if (autoStart)
            {
                StartGame();
            }
        }

        private void Update()
        {
            if (state == GameState.Playing)
            {
                timeRemaining -= Time.deltaTime;
                OnTimerUpdated?.Invoke(timeRemaining);

                if (timeRemaining <= 0)
                {
                    EndGame();
                }
            }
        }

        public void StartGame()
        {
            timeRemaining = timeLimit;
            currentScore = baseScore;
            triggeredHazardIDs.Clear();
            state = GameState.Playing;

            // Reset all registered hazards to their initial state for replayability
            foreach (var hazard in allHazards)
            {
                if (hazard != null)
                {
                    hazard.ResetHazard();
                }
            }

            OnStateChanged?.Invoke(state);
            OnScoreChanged?.Invoke(currentScore);
            Debug.Log("[SafetyGameManager] Game Started.");
        }

        public void EndGame()
        {
            state = GameState.GameOver;
            OnStateChanged?.Invoke(state);
            Debug.Log($"[SafetyGameManager] Game Over. Final Score: {currentScore}");
        }

        private void FindAndRegisterHazards()
        {
            allHazards.Clear();
            var found = FindObjectsByType<HazardBase>(FindObjectsInactive.Include);
            foreach (var hazard in found)
            {
                RegisterHazard(hazard);
            }
            Debug.Log($"[SafetyGameManager] Automatically registered {allHazards.Count} hazards.");
        }

        public void RegisterHazard(HazardBase hazard)
        {
            if (hazard == null || allHazards.Contains(hazard)) return;

            allHazards.Add(hazard);
            hazard.OnHazardTriggered += HandleHazardTriggered;
            hazard.OnHazardResolved += HandleHazardResolved;
        }

        private void HandleHazardTriggered(HazardBase hazard)
        {
            if (state != GameState.Playing) return;

            // Prevent double penalty for the same hazard type if configured
            if (!triggeredHazardIDs.Contains(hazard.HazardID))
            {
                triggeredHazardIDs.Add(hazard.HazardID);
                // In safety training, detecting a hazard correctly ADDs points, 
                // while stepping into/triggering a dangerous area subtracts safety points.
                currentScore -= hazard.DangerScore; 
                OnScoreChanged?.Invoke(currentScore);
                OnHazardEncountered?.Invoke(hazard);
            }
        }

        private void HandleHazardResolved(HazardBase hazard)
        {
            if (state != GameState.Playing) return;

            // Rewarding safety-conscious behavior
            currentScore += hazard.DangerScore;
            OnScoreChanged?.Invoke(currentScore);

            // Check if all registered hazards have been successfully resolved
            bool allResolved = true;
            foreach (var h in allHazards)
            {
                if (h != null && !h.IsResolved)
                {
                    allResolved = false;
                    break;
                }
            }

            if (allResolved && allHazards.Count > 0)
            {
                Debug.Log("[SafetyGameManager] All construction site hazards resolved successfully! Triggering evaluation.");
                EndGame();
            }
        }

        private void OnDestroy()
        {
            foreach (var hazard in allHazards)
            {
                if (hazard != null)
                {
                    hazard.OnHazardTriggered -= HandleHazardTriggered;
                    hazard.OnHazardResolved -= HandleHazardResolved;
                }
            }
        }
    }
}
