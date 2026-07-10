using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class WebStartRequestPoller : MonoBehaviour
{
    [Header("Web API")]
    [SerializeField] private string apiBaseUrl = "http://127.0.0.1:3000";
    [SerializeField] private string startRequestPath = "/api/unity/start-request";
    [SerializeField] private string consumePath = "/api/unity/start-request/consume";
    [SerializeField] private float pollingIntervalSeconds = 3f;

    [Header("Training Data")]
    [SerializeField] private string workerId = "W2025-0001";
    [SerializeField] private string sessionId = "S-CHEMI-001";
    [SerializeField] private string courseName = "建設現場VR危険予知訓練";

    [Header("Start Action")]
    [SerializeField] private bool pollOnEnable = true;
    [SerializeField] private bool autoFindTrainingStarter = true;
    [SerializeField] private MonoBehaviour trainingStarter;
    [SerializeField] private string startMethodName = "StartLesson";
    [SerializeField] private UnityEvent onStartRequestReceived;

    private Coroutine pollingCoroutine;
    private bool requestInProgress;
    private int processingRequestId = -1;

    public string WorkerId
    {
        get { return workerId; }
        set { workerId = value; }
    }

    public string SessionId
    {
        get { return sessionId; }
        set { sessionId = value; }
    }

    private void OnEnable()
    {
        if (pollOnEnable)
        {
            StartPolling();
        }
    }

    private void OnDisable()
    {
        StopPolling();
    }

    [ContextMenu("Start Polling")]
    public void StartPolling()
    {
        if (pollingCoroutine != null)
        {
            return;
        }

        pollingCoroutine = StartCoroutine(PollStartRequests());
        Debug.Log("Web開始指示ポーリング開始: worker_id=" + workerId + ", session_id=" + sessionId);
    }

    [ContextMenu("Stop Polling")]
    public void StopPolling()
    {
        if (pollingCoroutine == null)
        {
            return;
        }

        StopCoroutine(pollingCoroutine);
        pollingCoroutine = null;
        requestInProgress = false;
        Debug.Log("Web開始指示ポーリング停止");
    }

    [ContextMenu("Check Start Request Once")]
    public void CheckStartRequestOnce()
    {
        if (!requestInProgress)
        {
            StartCoroutine(CheckStartRequest());
        }
    }

    private IEnumerator PollStartRequests()
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(1f, pollingIntervalSeconds));

        while (enabled)
        {
            if (!requestInProgress)
            {
                yield return CheckStartRequest();
            }

            yield return wait;
        }
    }

    private IEnumerator CheckStartRequest()
    {
        requestInProgress = true;

        string url = BuildStartRequestUrl();

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Web開始指示確認API失敗: " + request.error + "\n" + request.downloadHandler.text);
            requestInProgress = false;
            yield break;
        }

        WebStartRequestCheckResponse response = JsonUtility.FromJson<WebStartRequestCheckResponse>(request.downloadHandler.text);
        if (response == null || !response.success)
        {
            Debug.LogWarning("Web開始指示確認API応答エラー: " + request.downloadHandler.text);
            requestInProgress = false;
            yield break;
        }

        if (!response.has_request || response.start_request == null)
        {
            requestInProgress = false;
            yield break;
        }

        if (processingRequestId == response.start_request.id)
        {
            requestInProgress = false;
            yield break;
        }

        processingRequestId = response.start_request.id;
        Debug.Log("Web開始指示を受信: id=" + response.start_request.id + ", worker_id=" + response.start_request.worker_id + ", session_id=" + response.start_request.session_id);

        InvokeTrainingStart(response.start_request);
        yield return ConsumeStartRequest(response.start_request.id);

        requestInProgress = false;
    }

    private void InvokeTrainingStart(WebStartRequestData startRequest)
    {
        onStartRequestReceived?.Invoke();

        if (trainingStarter == null && autoFindTrainingStarter)
        {
            FindTrainingStarter();
        }

        if (trainingStarter == null)
        {
            Debug.LogWarning("開始指示は受信しましたが、研修開始先が未設定です。Inspectorの On Start Request Received か Training Starter を設定してください。");
            return;
        }

        trainingStarter.SendMessage(startMethodName, SendMessageOptions.DontRequireReceiver);
        Debug.Log("研修開始処理を呼び出しました: " + trainingStarter.name + "." + startMethodName + "()");
    }

    private void FindTrainingStarter()
    {
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            Type type = behaviour.GetType();
            if (type.Name == "LessonManager")
            {
                trainingStarter = behaviour;
                startMethodName = "StartLesson";
                return;
            }

            if (type.FullName == "SafetyTraining.SafetyGameManager")
            {
                trainingStarter = behaviour;
                startMethodName = "StartGame";
                return;
            }
        }
    }

    private IEnumerator ConsumeStartRequest(int startRequestId)
    {
        string url = BuildUrl(consumePath);
        WebStartRequestConsumeRequest payload = new WebStartRequestConsumeRequest
        {
            start_request_id = startRequestId,
            worker_id = workerId,
            session_id = sessionId
        };

        string json = JsonUtility.ToJson(payload);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Web開始指示消化API失敗: " + request.error + "\n" + request.downloadHandler.text);
            yield break;
        }

        WebStartRequestConsumeResponse response = JsonUtility.FromJson<WebStartRequestConsumeResponse>(request.downloadHandler.text);
        if (response != null && response.success)
        {
            Debug.Log("Web開始指示消化成功: id=" + startRequestId + ", status=" + response.status);
        }
        else
        {
            Debug.LogWarning("Web開始指示消化API応答: " + request.downloadHandler.text);
        }
    }

    private string BuildStartRequestUrl()
    {
        string query = "?worker_id=" + UnityWebRequest.EscapeURL(workerId) + "&session_id=" + UnityWebRequest.EscapeURL(sessionId);
        return BuildUrl(startRequestPath) + query;
    }

    private string BuildUrl(string path)
    {
        return apiBaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }
}

[Serializable]
public class WebStartRequestCheckResponse
{
    public bool success;
    public bool has_request;
    public WebStartRequestData start_request;
}

[Serializable]
public class WebStartRequestData
{
    public int id;
    public string worker_id;
    public string session_id;
    public string course_name;
    public string status;
    public string created_at;
}

[Serializable]
public class WebStartRequestConsumeRequest
{
    public int start_request_id;
    public string worker_id;
    public string session_id;
}

[Serializable]
public class WebStartRequestConsumeResponse
{
    public bool success;
    public string status;
    public string error;
}
