using UnityEngine;

public class DangerZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("危険区域に侵入しました！");
    }
}

