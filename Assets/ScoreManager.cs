using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // 現在のスコアを保存する変数
    private int score = 0;

    void Start()
    {
        // ゲーム開始時にスコアをリセット
        ResetScore();
    }

    // 点数を増やす命令（外から呼び出せるようにpublicにする）
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("現在のスコア: " + score);
    }

    // 点数をリセットする命令
    public void ResetScore()
    {
        score = 0;
        Debug.Log("スコアがリセットされました。");
    }

    // 現在の点数を他のプログラムに教えてあげる命令
    public int GetScore()
    {
        return score;
    }
}