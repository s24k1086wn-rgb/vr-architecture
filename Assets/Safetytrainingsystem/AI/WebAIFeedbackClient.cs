using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebAIFeedbackClient : MonoBehaviour
{
    [Header("Web API")]
    [SerializeField] private string apiBaseUrl = "http://127.0.0.1:3000";
    [SerializeField] private string trainingResultPath = "/api/unity/training-result";
    [SerializeField] private string feedbackPath = "/api/ai/feedback";
    [SerializeField] private string unityApiKey = "";

    [Header("Fixed Training Data")]
    [SerializeField] private string sessionId = "S-CHEMI-001";
    [SerializeField] private string workerId = "W2025-0001";

    public Action<string> OnAdviceReceived;
    public Action<string> OnErrorReceived;
    public Action<WebTrainingResultSaveResponse> OnTrainingResultSaved;

    public void RequestFeedback(TrainingResult result)
    {
        StartCoroutine(PostFeedback(result));
    }

    public void SubmitTrainingResultAndFeedback(TrainingResult result)
    {
        StartCoroutine(PostTrainingResultAndFeedback(result));
    }

    private IEnumerator PostTrainingResultAndFeedback(TrainingResult result)
    {
        if (result == null)
        {
            NotifyError("TrainingResult が null です。");
            yield break;
        }

        yield return PostTrainingResult(result);
        yield return PostFeedback(result);
    }

    private IEnumerator PostTrainingResult(TrainingResult result)
    {
        string url = BuildUrl(trainingResultPath);
        UnityTrainingResultSaveRequest payload = BuildTrainingResultSaveRequest(result);
        string json = JsonUtility.ToJson(payload);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        ApplyJsonHeaders(request);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            NotifyError("Web 訓練結果保存API送信失敗: " + request.error + "\n" + request.downloadHandler.text);
            yield break;
        }

        WebTrainingResultSaveResponse response = JsonUtility.FromJson<WebTrainingResultSaveResponse>(request.downloadHandler.text);
        if (response != null && response.success)
        {
            Debug.Log("Web 訓練結果保存成功: session_id=" + response.session_id + ", worker_id=" + response.worker_id + ", logs_saved=" + response.logs_saved);
        }
        else
        {
            Debug.LogWarning("Web 訓練結果保存API応答: " + request.downloadHandler.text);
        }

        OnTrainingResultSaved?.Invoke(response);
    }

    private IEnumerator PostFeedback(TrainingResult result)
    {
        if (result == null)
        {
            NotifyError("TrainingResult が null です。");
            yield break;
        }

        string url = BuildUrl(feedbackPath);
        WebAIFeedbackRequest payload = new WebAIFeedbackRequest
        {
            source = "unity_vr",
            prompt_version = "ollama_web_v1",
            training_result = result
        };

        string json = JsonUtility.ToJson(payload);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        ApplyJsonHeaders(request);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            NotifyError("Web AI分析API送信失敗: " + request.error + "\n" + request.downloadHandler.text);
            yield break;
        }

        WebAIFeedbackResponse response = JsonUtility.FromJson<WebAIFeedbackResponse>(request.downloadHandler.text);
        string advice = ExtractAdvice(response, request.downloadHandler.text);

        Debug.Log("=== Web AIアドバイス ===\n" + advice);
        OnAdviceReceived?.Invoke(advice);
    }

    private string BuildUrl(string path)
    {
        return apiBaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    private void ApplyJsonHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrWhiteSpace(unityApiKey))
        {
            request.SetRequestHeader("x-unity-api-key", unityApiKey);
        }
    }

    private UnityTrainingResultSaveRequest BuildTrainingResultSaveRequest(TrainingResult result)
    {
        int safetyScore = CalculateSafetyScore(result);
        List<string> issues = BuildIssues(result);

        return new UnityTrainingResultSaveRequest
        {
            source = "unity_vr",
            session_id = sessionId,
            worker_id = workerId,
            safety_score = safetyScore,
            recommendation = BuildRecommendation(result),
            risk_tendency = BuildRiskTendency(result),
            sentiment_score = Mathf.Clamp((safetyScore - 50f) / 50f, -1f, 1f),
            analysis_type = "primary",
            issues_detected = issues,
            ai_comment = BuildSummaryComment(result, safetyScore),
            logs = BuildLogs(result)
        };
    }

    private int CalculateSafetyScore(TrainingResult result)
    {
        float score = 100f;
        score -= result.missedCount * 15f;
        score -= result.dangerZoneIntrusions * 10f;
        score -= result.fallCount * 20f;

        if (result.avgReactionTime > 5f)
        {
            score -= Mathf.Min((result.avgReactionTime - 5f) * 2f, 15f);
        }

        return Mathf.Clamp(Mathf.RoundToInt(score), 0, 100);
    }

    private string BuildRecommendation(TrainingResult result)
    {
        if (result.fallCount > 0)
        {
            return "高所作業時の立ち位置と足元確認を優先して再訓練してください。";
        }

        if (result.dangerZoneIntrusions > 0)
        {
            return "危険エリアへの接近前に周囲確認と停止判断を徹底してください。";
        }

        if (result.missedCount > 0)
        {
            return "見逃した危険箇所を中心に、視線移動と確認順序を復習してください。";
        }

        return "危険発見は良好です。現在の確認手順を維持してください。";
    }

    private string BuildRiskTendency(TrainingResult result)
    {
        List<string> tendencies = new List<string>();

        if (result.missedCount > 0)
        {
            tendencies.Add("hazard_miss");
        }

        if (result.dangerZoneIntrusions > 0)
        {
            tendencies.Add("danger_zone_intrusion");
        }

        if (result.fallCount > 0)
        {
            tendencies.Add("fall_risk");
        }

        if (result.avgReactionTime > 5f)
        {
            tendencies.Add("slow_reaction");
        }

        return tendencies.Count == 0 ? "stable" : string.Join(",", tendencies);
    }

    private List<string> BuildIssues(TrainingResult result)
    {
        List<string> issues = new List<string>();

        foreach (string item in result.missed)
        {
            issues.Add("見逃し: " + item);
        }

        if (result.dangerZoneIntrusions > 0)
        {
            issues.Add("危険エリア侵入: " + result.dangerZoneIntrusions + "回");
        }

        if (result.fallCount > 0)
        {
            issues.Add("高所落下: " + result.fallCount + "回");
        }

        if (issues.Count == 0)
        {
            issues.Add("重大な問題は検出されませんでした。");
        }

        return issues;
    }

    private string BuildSummaryComment(TrainingResult result, int safetyScore)
    {
        return "安全スコア " + safetyScore + "。発見 " + result.foundCount + "件、見逃し " + result.missedCount + "件、危険エリア侵入 " + result.dangerZoneIntrusions + "回、高所落下 " + result.fallCount + "回。";
    }

    private List<UnityTrainingLogPayload> BuildLogs(TrainingResult result)
    {
        List<UnityTrainingLogPayload> logs = new List<UnityTrainingLogPayload>();
        DateTime startedAt = DateTime.Now.AddSeconds(-Mathf.Max(result.totalTimeSeconds, 0f));
        Vector3 position = transform.position;
        int offsetSeconds = 0;

        AddLog(logs, "start", null, null, position, startedAt.AddSeconds(offsetSeconds++));

        foreach (string item in result.correctlyFound)
        {
            AddLog(logs, "gaze", item, null, position, startedAt.AddSeconds(offsetSeconds++));
        }

        foreach (string item in result.missed)
        {
            AddLog(logs, "missed", item, null, position, startedAt.AddSeconds(offsetSeconds++));
        }

        for (int i = 0; i < result.dangerZoneIntrusions; i++)
        {
            AddLog(logs, "collision", null, "danger_zone", position, startedAt.AddSeconds(offsetSeconds++));
        }

        for (int i = 0; i < result.fallCount; i++)
        {
            AddLog(logs, "fall", null, "fall_zone", position, startedAt.AddSeconds(offsetSeconds++));
        }

        AddLog(logs, "end", null, null, position, DateTime.Now);
        return logs;
    }

    private void AddLog(List<UnityTrainingLogPayload> logs, string eventType, string gazeTarget, string collidedObjectId, Vector3 position, DateTime timestamp)
    {
        logs.Add(new UnityTrainingLogPayload
        {
            event_type = eventType,
            gaze_target = gazeTarget,
            collided_object_id = collidedObjectId,
            position_x = position.x,
            position_y = position.y,
            position_z = position.z,
            timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz")
        });
    }

    private string ExtractAdvice(WebAIFeedbackResponse response, string rawJson)
    {
        if (response == null)
        {
            return rawJson;
        }

        if (!string.IsNullOrWhiteSpace(response.advice))
        {
            return response.advice;
        }

        if (!string.IsNullOrWhiteSpace(response.ai_comment))
        {
            return response.ai_comment;
        }

        if (!string.IsNullOrWhiteSpace(response.error))
        {
            return response.error;
        }

        return rawJson;
    }

    private void NotifyError(string message)
    {
        Debug.LogError(message);
        OnErrorReceived?.Invoke(message);
    }
}

[Serializable]
public class WebAIFeedbackRequest
{
    public string source;
    public string prompt_version;
    public TrainingResult training_result;
}

[Serializable]
public class WebAIFeedbackResponse
{
    public bool success;
    public string advice;
    public string ai_comment;
    public string error;
}

[Serializable]
public class UnityTrainingResultSaveRequest
{
    public string source;
    public string session_id;
    public string worker_id;
    public int safety_score;
    public string recommendation;
    public string risk_tendency;
    public float sentiment_score;
    public string analysis_type;
    public List<string> issues_detected = new List<string>();
    public string ai_comment;
    public List<UnityTrainingLogPayload> logs = new List<UnityTrainingLogPayload>();
}

[Serializable]
public class UnityTrainingLogPayload
{
    public string event_type;
    public string gaze_target;
    public string collided_object_id;
    public float position_x;
    public float position_y;
    public float position_z;
    public string timestamp;
}

[Serializable]
public class WebTrainingResultSaveResponse
{
    public bool success;
    public string session_id;
    public string worker_id;
    public int logs_saved;
    public int score_id;
    public string error;
}
