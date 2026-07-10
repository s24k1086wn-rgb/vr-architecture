using UnityEngine;

public class StaticDangerTask : MonoBehaviour
{
    [SerializeField] private string m_DangerName = "放置された工具"; // 変数名を少し変更
    [SerializeField] private float timeLimit = 15.0f;

    // ★外部から名前を安全に読み取るための窓口（これがないとシミュレーターが読めません）
    public string dangerName => m_DangerName; 

    private ResultManager resultManager;
    private bool isTaskActive = true;

    void Start()
    {
        resultManager = FindAnyObjectByType<ResultManager>();
        Invoke("TimeUp", timeLimit);
    }

    void TimeUp()
    {
        if (isTaskActive && resultManager != null)
        {
            isTaskActive = false;
            resultManager.RegisterMiss(dangerName);
        }
    }

    public void OnDangerFound()
    {
        if (!isTaskActive) return;

        isTaskActive = false;
        CancelInvoke("TimeUp");

        if (resultManager != null)
        {
            resultManager.RegisterDiscovery(dangerName, 0f, 0f);
            resultManager.SaveDataToPC();
        }
    }
}