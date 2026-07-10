using UnityEngine;

public class DangerZoneTask : MonoBehaviour
{
    private ResultManager rm;
    private bool hasTriggered = false;

    void Start()
    {
        rm = FindAnyObjectByType<ResultManager>();
    }

    // 危険エリアに入った瞬間
    void OnTriggerEnter(Collider other)
    {
        // ぶつかった物体自身、またはその「親」や「子」にカメラが含まれているか、Playerタグの場合
        bool isPlayer = other.CompareTag("MainCamera") || 
                        other.CompareTag("Player") ||
                        other.GetComponent<Camera>() != null || 
                        other.GetComponentInParent<Camera>() != null || 
                        other.GetComponentInChildren<Camera>() != null;

        if (!hasTriggered && isPlayer)
        {
            hasTriggered = true;
            Debug.Log($"【システム】危険エリア（{gameObject.name}）に侵入しました！");

            if (rm != null)
            {
                rm.RegisterIntrusion();
            }
        }
    }

    // エリアから外に出たとき
    void OnTriggerExit(Collider other)
    {
        bool isPlayer = other.CompareTag("MainCamera") || 
                        other.CompareTag("Player") ||
                        other.GetComponent<Camera>() != null || 
                        other.GetComponentInParent<Camera>() != null || 
                        other.GetComponentInChildren<Camera>() != null;

        if (isPlayer)
        {
            hasTriggered = false;
            Debug.Log($"【システム】危険エリア（{gameObject.name}）から脱出しました。");
        }
    }
}