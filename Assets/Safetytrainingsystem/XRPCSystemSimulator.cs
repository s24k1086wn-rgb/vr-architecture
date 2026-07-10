using UnityEngine;

public class XRPCSystemSimulator : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // 本番の重機レイヤーをすり抜けて、すべてのコライダーを一斉取得
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            GeneralDangerTask dangerTask = hit.collider.GetComponent<GeneralDangerTask>();
            
            if (dangerTask != null)
            {
                dangerTask.OnGazeEnter();

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log($"【PCテスト】安全にセンサーを検知: {dangerTask.dangerName}");
                    dangerTask.OnDangerFound();
                }
                break; 
            }
        }
    }
}