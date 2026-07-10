using UnityEngine;
using System.Collections.Generic;

public class AssetScatter : MonoBehaviour
{
    [Header("配置の設定")]
    public float areaSize = 30f;      // 現場の広さ
    public int count = 50;            // 配置したい個数

    [Header("使う素材 (Prefabをここにドラッグ)")]
    public GameObject[] prefabs;      // 素材リスト

    private List<GameObject> spawned = new List<GameObject>();

    [ContextMenu("Scatter Assets")] // 右クリックで実行できるようにする
    public void Scatter()
    {
        Clear();
        if (prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            // ランダムな位置と回転
            Vector3 pos = new Vector3(Random.Range(-areaSize / 2, areaSize / 2), 0, Random.Range(-areaSize / 2, areaSize / 2));
            Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

            // ランダムに素材を選んで配置
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject obj = Instantiate(prefab, pos, rot, this.transform);

            // サイズを少しバラつかせてリアルにする
            float randomScale = Random.Range(0.8f, 1.2f);
            obj.transform.localScale = Vector3.one * randomScale;

            spawned.Add(obj);
        }
    }

    [ContextMenu("Clear Site")]
    public void Clear()
    {
        foreach (var obj in spawned) if (obj != null) DestroyImmediate(obj);
        spawned.Clear();
    }
}