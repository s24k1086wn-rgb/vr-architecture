using UnityEngine;

public class WebAIFeedbackResultBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResultManager resultManager;
    [SerializeField] private WebAIFeedbackClient webAIFeedbackClient;

    [Header("Options")]
    [SerializeField] private bool autoFindReferences = true;

    private void Awake()
    {
        if (autoFindReferences)
        {
            FindMissingReferences();
        }
    }

    [ContextMenu("Find Missing References")]
    public void FindMissingReferences()
    {
        if (resultManager == null)
        {
            resultManager = FindAnyObjectByType<ResultManager>();
        }

        if (webAIFeedbackClient == null)
        {
            webAIFeedbackClient = FindAnyObjectByType<WebAIFeedbackClient>();
        }
    }

    [ContextMenu("Send Current Result To Web AI")]
    public void SendCurrentResultToWebAI()
    {
        SendFeedbackOnly();
    }

    [ContextMenu("Send Current Result To Web Save And AI")]
    public void SendCurrentResultToWebSaveAndAI()
    {
        if (!TryPrepareReferences())
        {
            return;
        }

        webAIFeedbackClient.SubmitTrainingResultAndFeedback(resultManager.result);
    }

    public void SendFeedbackOnly()
    {
        if (!TryPrepareReferences())
        {
            return;
        }

        webAIFeedbackClient.RequestFeedback(resultManager.result);
    }

    private bool TryPrepareReferences()
    {
        if (resultManager == null || webAIFeedbackClient == null)
        {
            FindMissingReferences();
        }

        if (resultManager == null)
        {
            Debug.LogError("ResultManager が見つかりません。シーンに ResultManager があるか確認してください。");
            return false;
        }

        if (webAIFeedbackClient == null)
        {
            Debug.LogError("WebAIFeedbackClient が見つかりません。シーンに WebAIFeedbackClient コンポーネントを追加してください。");
            return false;
        }

        return true;
    }
}
