using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private ResultManager resultManager;
    private bool isFallingFromHigh = false; // 高いところから落ちてきているかフラグ
    private GameObject playerCamera;

    void Start()
    {
        resultManager = FindAnyObjectByType<ResultManager>();
        
        // シーン内から MainCamera タグのついたカメラを探しておく
        playerCamera = GameObject.FindWithTag("MainCamera");
    }

    void Update()
    {
        if (playerCamera != null)
        {
            // プレイヤーが2階の高さ（Y座標が2.5メートル以上）にいる時は、フラグをONにする
            if (playerCamera.transform.position.y > 2.5f)
            {
                isFallingFromHigh = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ぶつかったのがカメラで、かつ「高いところから落ちてきたフラグ」がONの場合だけカウント
        if (other.CompareTag("MainCamera") && isFallingFromHigh && resultManager != null)
        {
            isFallingFromHigh = false; // フラグをすぐ消して連打防止

            // ★【修正】大元の脳みそに「落下したよ！」と正しく通知する
            resultManager.RegisterFall();
        }
    }
}