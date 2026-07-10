using UnityEngine;
using TMPro;

public class LessonManager : MonoBehaviour
{
    [Header("教習時間")]
    [SerializeField] private float duration = 120f;

    [Header("画面")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("残り時間の表示")]
    [SerializeField] private TMP_Text timerText;

    [Header("開始・終了時に制御するスクリプト")]
    [SerializeField] private MonoBehaviour[] lessonScripts;

    [Header("開始・終了時に制御するアニメーション")]
    [SerializeField] private Animator[] lessonAnimators;

    [Header("木箱落下イベント")]
    [SerializeField] private WoodenBoxFallEvent[] woodenBoxFallEvents;

    private float remainingTime;
    private bool isLessonRunning;

    private void Start()
    {
        remainingTime = duration;
        isLessonRunning = false;

        // 開始画面を表示
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        // 通常終了画面を非表示
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // 開始前は時間表示を非表示
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        // スタートを押すまで動きを停止
        SetLessonScripts(false);
        SetLessonAnimators(false);

        UpdateTimerDisplay();

        Debug.Log("開始ボタンを押してください。");
    }

    private void Update()
    {
        if (!isLessonRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        UpdateTimerDisplay();

        if (remainingTime <= 0f)
        {
            EndLesson();
        }
    }

    public void StartLesson()
    {
        remainingTime = duration;
        isLessonRunning = true;

        // 開始画面を消す
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        // 終了画面を隠す
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // 残り時間を表示
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        // オブジェクトやプレイヤー操作を開始
        SetLessonScripts(true);
        SetLessonAnimators(true);

        // 木箱落下イベントの待機を開始
        foreach (WoodenBoxFallEvent boxEvent in woodenBoxFallEvents)
        {
            if (boxEvent != null)
            {
                boxEvent.StartWoodenBoxFallEvent();
            }
        }

        UpdateTimerDisplay();

        Debug.Log("教習開始");
    }

    private void EndLesson()
    {
        isLessonRunning = false;
        remainingTime = 0f;

        // オブジェクトやプレイヤー操作を停止
        SetLessonScripts(false);
        SetLessonAnimators(false);

        // 時間表示を消す
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        // 通常終了画面を表示
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        Debug.Log("教習終了");
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void SetLessonScripts(bool active)
    {
        foreach (MonoBehaviour lessonScript in lessonScripts)
        {
            if (lessonScript != null)
            {
                lessonScript.enabled = active;
            }
        }
    }

    private void SetLessonAnimators(bool active)
    {
        foreach (Animator lessonAnimator in lessonAnimators)
        {
            if (lessonAnimator != null)
            {
                lessonAnimator.enabled = active;
            }
        }
    }
}
