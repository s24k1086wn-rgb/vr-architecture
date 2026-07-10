using UnityEngine;

public class CraneTask : MonoBehaviour
{
    private ScoreManager scoreManager;
    private ResultManager resultManager; 
    private AudioSource audioSource;
    private Animator animator; 
    private bool isTaskActive = false;

    private float eventStartTime = 0f;  
    private float firstGazeTime = 0f;   
    private bool hasGazed = false;      

    void Start()
    {
        scoreManager = FindAnyObjectByType<ScoreManager>();
        resultManager = FindAnyObjectByType<ResultManager>(); 
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); 

        Invoke("StartCraneEvent", 7.0f);
    }

    void StartCraneEvent()
    {
        isTaskActive = true;
        eventStartTime = Time.time; 

        if (audioSource != null) audioSource.Play();
        if (animator != null) animator.SetTrigger("IsRotate"); 

        Debug.Log("【イベント開始】クレーンが旋回を始めました！");

        // ★【新機能】10秒後に自動で「見逃し判定」を行うタイマーをスタート
        Invoke("TimeUp", 10.0f);
    }

    // ★【新機能】クリックされずに10秒経ったら呼び出される
    void TimeUp()
    {
        if (isTaskActive)
        {
            isTaskActive = false;
            if (audioSource != null) audioSource.Stop();
            if (animator != null) animator.speed = 0f;

            if (resultManager != null)
            {
                resultManager.RegisterMiss("クレーンの旋回");
            }
        }
    }

    public void OnGazeEnter()
    {
        if (!isTaskActive || hasGazed) return;
        firstGazeTime = Time.time;
        hasGazed = true;
        Debug.Log("【注視検知】プレイヤーがクレーンを見ました！");
    }

    public void OnDangerFound()
    {
        if (!isTaskActive) return;

        isTaskActive = false;
        CancelInvoke("TimeUp"); // ★発見できたので、見逃しタイマーを解除する

        if (audioSource != null) audioSource.Stop();
        if (animator != null) animator.speed = 0f; 

        float reactionTime = Time.time - eventStartTime; 
        float gazeTime = hasGazed ? (firstGazeTime - eventStartTime) : reactionTime;

        if (scoreManager != null) scoreManager.AddScore(1);

        if (resultManager != null)
        {
            resultManager.RegisterDiscovery("クレーンの旋回", reactionTime, gazeTime);
            resultManager.SaveDataToPC();
        }
    }
}