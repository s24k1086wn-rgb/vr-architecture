# Web AI Final Integration Guide

## Final Policy

Unity does not call Gemini, OpenAI, or Ollama directly.

Unity sends the existing `TrainingResult` to the Web system.
The Web system saves the training result to the database, calls the local Ollama AI, and returns an advice message.

```text
Unity
↓
POST /api/unity/training-result
↓
Web system database
↓
POST /api/ai/feedback
↓
Web system
↓
Ollama local AI
↓
Web system
↓
Unity
```

## Required Unity Files

Keep these files:

```text
Assets/Safetytrainingsystem/AI/WebAIFeedbackClient.cs
Assets/Safetytrainingsystem/AI/WebAIFeedbackResultBridge.cs
Assets/Safetytrainingsystem/AI/WebStartRequestPoller.cs
Assets/Safetytrainingsystem/AI/WebAI_FinalIntegrationGuide.md
```

## Existing VR Files

Do not change these existing VR files:

```text
Assets/Safetytrainingsystem/ResultManager.cs
Assets/Safetytrainingsystem/GeneralDangerTask.cs
Assets/Safetytrainingsystem/DangerZoneTask.cs
Assets/Safetytrainingsystem/FallDetector.cs
Assets/Safetytrainingsystem/XRPCSystemSimulator.cs
```

## Unity Endpoint

Local Web server:

```text
POST http://localhost:3000/api/unity/training-result
POST http://localhost:3000/api/ai/feedback
```

Meta Quest or another device on the same network:

```text
POST http://<web-pc-ip-address>:3000/api/unity/training-result
POST http://<web-pc-ip-address>:3000/api/ai/feedback
```

Production:

```text
POST https://<public-url>/api/unity/training-result
POST https://<public-url>/api/ai/feedback
```

## Unity Save Request JSON

Unity saves:

```json
{
  "source": "unity_vr",
  "session_id": "S-CHEMI-001",
  "worker_id": "W2025-0001",
  "safety_score": 82,
  "recommendation": "見逃した危険箇所を中心に、視線移動と確認順序を復習してください。",
  "risk_tendency": "hazard_miss,danger_zone_intrusion",
  "sentiment_score": 0.64,
  "analysis_type": "primary",
  "issues_detected": ["見逃し: 足場端部"],
  "ai_comment": "安全スコア 82。発見 2件、見逃し 1件、危険エリア侵入 1回、高所落下 0回。",
  "logs": [
    {
      "event_type": "start",
      "gaze_target": null,
      "collided_object_id": null,
      "position_x": 0,
      "position_y": 1.6,
      "position_z": 0,
      "timestamp": "2026-07-07T10:00:00+09:00"
    }
  ]
}
```

## Unity AI Request JSON

Unity sends:

```json
{
  "source": "unity_vr",
  "prompt_version": "ollama_web_v1",
  "training_result": {
    "correctlyFound": ["開口部", "重機接近"],
    "missed": ["足場端部"],
    "foundCount": 2,
    "missedCount": 1,
    "totalTimeSeconds": 118,
    "avgReactionTime": 6.8,
    "avgGazeTime": 4.2,
    "dangerZoneIntrusions": 1,
    "fallCount": 0
  }
}
```

`TrainingResult` is the existing class in:

```text
Assets/Safetytrainingsystem/ResultManager.cs
```

Do not define another `TrainingResult` class.

## Web Response JSON

Success:

```json
{
  "success": true,
  "advice": "AI講評本文"
}
```

Error:

```json
{
  "success": false,
  "error": "エラー内容"
}
```

Unity reads response fields in this order:

```text
1. advice
2. ai_comment
3. error
4. raw response JSON
```

## Unity Scene Setup

1. Add an empty GameObject named `WebAIFeedbackClient`.
2. Add the `WebAIFeedbackClient` component.
3. Set `apiBaseUrl`.
   - Same PC: `http://localhost:3000`
   - Quest or another device: `http://10.96.171.189:3000`
4. Keep `trainingResultPath` as `/api/unity/training-result`.
5. Keep `feedbackPath` as `/api/ai/feedback`.
6. Add another empty GameObject named `WebAIFeedbackResultBridge`.
7. Add the `WebAIFeedbackResultBridge` component.
8. Assign `ResultManager` and `WebAIFeedbackClient` in the Inspector, or leave auto-find enabled.
9. Set `sessionId` to `S-CHEMI-001`.
10. Set `workerId` to one of `W2025-0001` through `W2025-0006`.
11. For the first test, use `Context Menu > Send Current Result To Web Save And AI`.

## Unity Start Request Polling Setup

Add this when the Web worker screen should start the VR training.

1. Add an empty GameObject named `WebStartRequestPoller`.
2. Add the `WebStartRequestPoller` component.
3. Set `apiBaseUrl` to `http://127.0.0.1:3000`.
4. Keep `startRequestPath` as `/api/unity/start-request`.
5. Keep `consumePath` as `/api/unity/start-request/consume`.
6. Set `workerId` to the current worker, for example `W2025-0001`.
7. Set `sessionId` to `S-CHEMI-001`.
8. Keep `pollOnEnable` enabled.
9. If the scene uses `LessonManager`, leave `autoFindTrainingStarter` enabled.
10. If automatic detection is not enough, assign the training start component to `trainingStarter` and set `startMethodName`.

Common start method names:

```text
LessonManager: StartLesson
SafetyTraining.SafetyGameManager: StartGame
```

Polling flow:

```text
GET /api/unity/start-request?worker_id=W2025-0001&session_id=S-CHEMI-001
↓
has_request true
↓
call StartLesson or StartGame
↓
POST /api/unity/start-request/consume
```

## Fixed Web Data

Current course:

```text
session_id: S-CHEMI-001
courseName: 建設現場VR危険予知訓練
```

Available workers:

```text
W2025-0001
W2025-0002
W2025-0003
W2025-0004
W2025-0005
W2025-0006
```

The AI feedback endpoint returns advice only.
The database update depends on `/api/unity/training-result`, so Unity sends both endpoints.

## Important Notes

- No Gemini API key is needed.
- No OpenAI API key is needed.
- `Assets/Resources/apikey.txt` is not used.
- Unity should not call `http://localhost:11434` directly.
- Ollama is managed by the Web side.

## Known Naming Notes

Current Unity field names:

```text
avgGazeTime
fallCount
```

In the existing Unity code, `avgGazeTime` behaves like average time until first gaze, not total gaze duration.
`fallCount` is currently treated as high-place fall count.

If the Web display text says "注視時間" or "転倒回数", align the wording later if needed.
