using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class TrainingResult
{
    public List<string> correctlyFound = new List<string>(); // 発見できた危険
    public List<string> missed = new List<string>();         // 見逃した危険
    public int foundCount;                                   // 合計発見数
    public int missedCount;                                  // 合計見逃し数
    public float totalTimeSeconds;                           // 全体の所要時間
    public float avgReactionTime;                            // 平均発見時間
    public float avgGazeTime;                                // 平均視線移動時間
    public int dangerZoneIntrusions;                         // 危険エリア侵入回数
    public int fallCount;                                    // 高所落下回数
}

public class ResultManager : MonoBehaviour
{
    public TrainingResult result = new TrainingResult();
    private List<float> allReactionTimes = new List<float>();
    private List<float> allGazeTimes = new List<float>();

    // 危険を発見したときに呼ばれる
    public void RegisterDiscovery(string dangerName, float reactionTime, float gazeTime)
    {
        if (!result.correctlyFound.Contains(dangerName))
        {
            result.correctlyFound.Add(dangerName);
            result.foundCount++;
            allReactionTimes.Add(reactionTime);
            allGazeTimes.Add(gazeTime);
            UpdateAverages();
            Debug.Log($"【記録】{dangerName}を発見！ 合計: {result.foundCount}");
            SaveDataToPC();
        }
    }

    // 見逃したときに呼ばれる
    public void RegisterMiss(string dangerName)
    {
        if (!result.correctlyFound.Contains(dangerName) && !result.missed.Contains(dangerName))
        {
            result.missed.Add(dangerName);
            result.missedCount++;
            Debug.Log($"【記録】{dangerName}を見逃し。合計: {result.missedCount}");
            SaveDataToPC();
        }
    }

    // 危険エリア侵入時に呼ばれる
    public void RegisterIntrusion()
    {
        result.dangerZoneIntrusions++;
        Debug.Log($"【記録】危険エリア侵入！ 合計: {result.dangerZoneIntrusions}回");
        SaveDataToPC();
    }

    // 落下時に呼ばれる
    public void RegisterFall()
    {
        result.fallCount++;
        Debug.Log($"【記録】落下を検知！ 合計: {result.fallCount}回");
        SaveDataToPC();
    }

    private void UpdateAverages()
    {
        if (allReactionTimes.Count > 0)
        {
            float totalReaction = 0;
            foreach (float t in allReactionTimes) totalReaction += t;
            result.avgReactionTime = totalReaction / allReactionTimes.Count;
        }
        if (allGazeTimes.Count > 0)
        {
            float totalGaze = 0;
            foreach (float t in allGazeTimes) totalGaze += t;
            result.avgGazeTime = totalGaze / allGazeTimes.Count;
        }
    }

    public void SaveDataToPC()
    {
        string jsonText = JsonUtility.ToJson(result, true);
        string savePath = Path.Combine(Application.persistentDataPath, "TrainingResult.json");
        File.WriteAllText(savePath, jsonText);
    }
}