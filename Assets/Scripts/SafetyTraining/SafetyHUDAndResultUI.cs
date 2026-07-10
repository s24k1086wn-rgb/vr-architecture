using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SafetyTraining
{
    /// <summary>
    /// Manages the Game HUD (Score, Timer) and the final evaluation/result screen in uGUI.
    /// Handles mouse cursor unlocking on Game Over so players can click buttons easily.
    /// </summary>
    public class SafetyHUDAndResultUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject hudPanel;

        [Header("Result Panel Elements")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text evaluationText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            // Set up button listeners
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Initially hide the result panel and show HUD
            if (resultPanel != null) resultPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(true);

            // Subscribe to Game Manager events
            if (SafetyGameManager.Instance != null)
            {
                SafetyGameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
                SafetyGameManager.Instance.OnTimerUpdated += UpdateTimerDisplay;
                SafetyGameManager.Instance.OnStateChanged += HandleStateChanged;

                // Set initial displays
                UpdateScoreDisplay(SafetyGameManager.Instance.CurrentScore);
                UpdateTimerDisplay(SafetyGameManager.Instance.TimeRemaining);
            }
        }

        private void OnDestroy()
        {
            if (SafetyGameManager.Instance != null)
            {
                SafetyGameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                SafetyGameManager.Instance.OnTimerUpdated -= UpdateTimerDisplay;
                SafetyGameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void UpdateScoreDisplay(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Safety Score: {newScore}";
            }
        }

        private void UpdateTimerDisplay(float timeRemaining)
        {
            if (timerText != null)
            {
                if (timeRemaining < 0) timeRemaining = 0;
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            if (newState == GameState.GameOver)
            {
                ShowResultScreen();
            }
            else if (newState == GameState.Playing)
            {
                // Re-enable HUD and hide result screen on play
                if (resultPanel != null) resultPanel.SetActive(false);
                if (hudPanel != null) hudPanel.SetActive(true);

                // Lock the cursor back for FPS mode
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void ShowResultScreen()
        {
            if (hudPanel != null) hudPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(true);

            // Unlock cursor so player can click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            int score = SafetyGameManager.Instance.CurrentScore;
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Safety Score: {score} / 100";
            }

            if (evaluationText != null)
            {
                evaluationText.text = GetEvaluationFeedback(score);
            }
        }

        private string GetEvaluationFeedback(int score)
        {
            if (score >= 100)
            {
                return "<color=#00FF00>RANK S (Perfect Safety Awareness)</color>\n\nExcellent work! You detected and corrected all hazards without stepping into danger. You are fully qualified for the job site!";
            }
            else if (score >= 80)
            {
                return "<color=#40FF40>RANK A (High Safety Awareness)</color>\n\nGreat job! You showed great caution and resolved most hazards. Keep practicing to maintain zero incidents.";
            }
            else if (score >= 60)
            {
                return "<color=#FFFF00>RANK B (Standard Awareness)</color>\n\nYou passed, but with minor infractions. Ensure you keep a safer distance from open ledges and dangerous zones.";
            }
            else if (score >= 40)
            {
                return "<color=#FF8000>RANK C (Needs Improvement)</color>\n\nYou triggered multiple warning events. Construction sites require extreme vigilance. Please review the site safety manual.";
            }
            else
            {
                return "<color=#FF0000>RANK D (Critical Failure)</color>\n\nToo many safety violations! Stepping into active hazard zones can lead to fatal accidents. Retake the training immediately.";
            }
        }

        private void OnRestartClicked()
        {
            if (SafetyGameManager.Instance != null)
            {
                SafetyGameManager.Instance.StartGame();
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
