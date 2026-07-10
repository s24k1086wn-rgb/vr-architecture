using UnityEngine;

public class GeneralDangerTask : MonoBehaviour
{
    [Header("危険の設定")]
    public string dangerName = "危険オブジェクト";
    public float startDelay = 10.0f;  // 発動する秒数
    public float timeLimit = 10.0f;   // 見逃しまでの制限時間

    private ResultManager resultManager;
    private AudioSource audioSource;
    private Animator animator;
    private bool isTaskActive = false;
    private float eventStartTime = 0f;
    private float firstGazeTime = 0f;
    private bool hasGazed = false;

    void Awake()
    {
        resultManager = FindAnyObjectByType<ResultManager>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // ★本番のLessonManagerによってこのオブジェクトが「有効（ON）」にされた瞬間にタイマーが始まります
    void OnEnable()
    {
        Invoke("StartDangerEvent", startDelay);
    }

    void StartDangerEvent()
    {
        isTaskActive = true;
        eventStartTime = Time.time;

        if (audioSource != null) audioSource.Play();
        if (animator != null) animator.SetTrigger("IsActive");

        Debug.Log($"【イベント開始】{dangerName} が発動しました！");
        Invoke("TimeUp", timeLimit);
    }

    void TimeUp()
    {
        if (isTaskActive)
        {
            isTaskActive = false;
            if (audioSource != null) audioSource.Stop();
            if (resultManager != null) resultManager.RegisterMiss(dangerName);
        }
    }

    public void OnGazeEnter()
    {
        if (!isTaskActive || hasGazed) return;
        firstGazeTime = Time.time;
        hasGazed = true;
        Debug.Log($"【注視検知】プレイヤーが {dangerName} を見ました！");
    }

    public void OnDangerFound()
    {
        if (!isTaskActive) return;
        isTaskActive = false;
        CancelInvoke("TimeUp");

        if (audioSource != null) audioSource.Stop();

        float reactionTime = Time.time - eventStartTime;
        float gazeTime = hasGazed ? (firstGazeTime - eventStartTime) : reactionTime;

        if (resultManager != null)
        {
            resultManager.RegisterDiscovery(dangerName, reactionTime, gazeTime);
        }
    }
}
