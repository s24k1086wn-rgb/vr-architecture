using UnityEngine;

public class ShovelTask : MonoBehaviour
{
    private ScoreManager scoreManager;
    private ResultManager resultManager; 
    private AudioSource audioSource;
    private bool isTaskActive = false;

    private float eventStartTime = 0f;  
    private float firstGazeTime = 0f;   
    private bool hasGazed = false;      

    void Start()
    {
        scoreManager = FindAnyObjectByType<ScoreManager>();
        resultManager = FindAnyObjectByType<ResultManager>(); 
        audioSource = GetComponent<AudioSource>();

        Invoke("StartShovelEvent", 3.0f);
    }

    void StartShovelEvent()
    {
        if (audioSource != null)
        {
            audioSource.Play();
            isTaskActive = true;
            eventStartTime = Time.time; 
            
            Debug.Log("【イベント開始】ショベルカーの音が鳴りました！");

            // ★【新機能】10秒後に自動で「見逃し判定」を行うタイマーをスタート
            Invoke("TimeUp", 10.0f);
        }
    }

    // ★【新機能】クリックされずに10秒経ったら呼び出される
    void TimeUp()
    {
        if (isTaskActive)
        {
            isTaskActive = false;
            if (audioSource != null) audioSource.Stop();

            if (resultManager != null)
            {
                resultManager.RegisterMiss("ショベルカーの急始動");
            }
        }
    }

    public void OnGazeEnter()
    {
        if (!isTaskActive || hasGazed) return;
        firstGazeTime = Time.time;
        hasGazed = true;
        Debug.Log("【注視検知】プレイヤーがショベルカーを見ました！");
    }

    public void OnDangerFound()
    {
        if (!isTaskActive) return;

        isTaskActive = false;
        CancelInvoke("TimeUp"); // ★発見できたので、見逃しタイマーを解除する

        if (audioSource != null) audioSource.Stop(); 

        float reactionTime = Time.time - eventStartTime; 
        float gazeTime = hasGazed ? (firstGazeTime - eventStartTime) : reactionTime;

        if (scoreManager != null) scoreManager.AddScore(1);

        if (resultManager != null)
        {
            resultManager.RegisterDiscovery("ショベルカーの急始動", reactionTime, gazeTime);
            resultManager.SaveDataToPC();
        }
    }
}