using UnityEngine;
using TMPro;
using System.Collections;

namespace SafetyTraining
{
    /// <summary>
    /// Specific trigger script that shows a large Japanese warning in the center of the screen
    /// when the player enters a zone, and smoothly fades it out.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class JapaneseZoneWarning : MonoBehaviour
    {
        [Header("Warning Text Settings")]
        [SerializeField] [TextArea(2, 4)] private string warningMessage = "⚠️ 立入禁止！\n関係者以外はこれより先への立入を禁じます。";
        [SerializeField] private Color textColor = Color.red;
        [SerializeField] private float displayDuration = 2.5f;
        [SerializeField] private float fadeDuration = 1.0f;

        [Header("UI Reference")]
        [Tooltip("The central large TextMeshProUGUI component that will display this warning.")]
        [SerializeField] private TextMeshProUGUI warningTextComponent;

        private Coroutine fadeCoroutine;
        private Collider zoneCollider;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null)
            {
                zoneCollider.isTrigger = true;
            }
        }

        private void Start()
        {
            // Ensure text is empty at start
            if (warningTextComponent != null)
            {
                warningTextComponent.text = "";
                // Set alpha to 0 initially
                Color col = warningTextComponent.color;
                col.a = 0f;
                warningTextComponent.color = col;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (warningTextComponent != null)
                {
                    if (fadeCoroutine != null)
                    {
                        StopCoroutine(fadeCoroutine);
                    }
                    fadeCoroutine = StartCoroutine(CoShowAndFadeText());
                }
            }
        }

        private IEnumerator CoShowAndFadeText()
        {
            // 1. Set text and color
            warningTextComponent.text = warningMessage;
            warningTextComponent.color = new Color(textColor.r, textColor.g, textColor.b, 1f);

            // 2. Display fully visible
            yield return new WaitForSeconds(displayDuration);

            // 3. Fade out smoothly
            float elapsed = 0f;
            Color startColor = warningTextComponent.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float norm = Mathf.Clamp01(elapsed / fadeDuration);
                
                // Linear fade
                warningTextComponent.color = new Color(startColor.r, startColor.g, startColor.b, 1f - norm);
                yield return null;
            }

            // 4. Ensure completely clear and text reset
            warningTextComponent.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            warningTextComponent.text = "";
            fadeCoroutine = null;
        }

        private void OnDisable()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            if (warningTextComponent != null)
            {
                warningTextComponent.text = "";
            }
        }
    }
}
