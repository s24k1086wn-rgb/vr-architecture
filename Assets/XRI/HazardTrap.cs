using UnityEngine;

public class HazardTrap : MonoBehaviour
{
    [Header("罠の設定")]
    public string trapName = "危険な箇所";
    public string message = "注意不足です！足元を確認しましょう。";

    // プレイヤーがこのエリアに入った瞬間に呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        // "Player" という名前のタグがついたオブジェクトが触れた場合のみ反応する
        if (other.CompareTag("Player"))
        {
            Debug.LogError($"【ハザード発動！】{trapName}に引っかかりました！");
            Debug.Log($"解説：{message}");

            // ここにゲームオーバー処理や、減点ログなどの機能を追加できます
        }
    }
}