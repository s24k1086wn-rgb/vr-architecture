using UnityEngine;
using TMPro;
using System.Collections;

namespace SafetyTraining
{
    /// <summary>
    /// Handles visual warning panels (World Space Canvas) and warning sounds in VR
    /// when the player encounters or triggers a hazard.
    /// </summary>
    public class SafetyAlertUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private float displayDuration = 4.0f;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip warningSiren;

        private Coroutine alertCoroutine;

        private void Start()
        {
            // Initial State
            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // Subscribe to Manager events
            if (SafetyGameManager.Instance != null)
            {
                SafetyGameManager.Instance.OnHazardEncountered += ShowWarning;
            }
            else
            {
                Debug.LogWarning("[SafetyAlertUI] SafetyGameManager instance not found in scene. Waiting...");
                StartCoroutine(DelayedSubscribe());
            }
        }

        private IEnumerator DelayedSubscribe()
        {
            yield return new WaitUntil(() => SafetyGameManager.Instance != null);
            SafetyGameManager.Instance.OnHazardEncountered += ShowWarning;
        }

        private void OnDestroy()
        {
            if (SafetyGameManager.Instance != null)
            {
                SafetyGameManager.Instance.OnHazardEncountered -= ShowWarning;
            }
        }

        public void ShowWarning(HazardBase hazard)
        {
            if (hazard == null) return;

            if (alertCoroutine != null)
            {
                StopCoroutine(alertCoroutine);
            }

            alertCoroutine = StartCoroutine(CoShowWarning(hazard.HazardName, hazard.Description));
        }

        private IEnumerator CoShowWarning(string title, string description)
        {
            // Set text
            if (titleText != null) titleText.text = "⚠️ " + title;
            if (descriptionText != null) descriptionText.text = description;

            // Enable Panel
            if (warningPanel != null)
            {
                warningPanel.SetActive(true);
            }

            // Play Alert Sound
            if (audioSource != null && warningSiren != null)
            {
                audioSource.PlayOneShot(warningSiren);
            }

            // Wait
            yield return new WaitForSeconds(displayDuration);

            // Disable Panel
            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }

            alertCoroutine = null;
        }
    }
}
