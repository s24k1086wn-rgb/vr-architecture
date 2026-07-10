using UnityEngine;
using System.Collections.Generic; // 削除機能のために必要

public class SiteGenerator : MonoBehaviour
{
    [Header("現場の設定")]
    public float siteSize = 20f;
    public int obstacleCount = 15;

    // 生成したオブジェクトを覚えておくリスト
    private List<GameObject> spawnedObjects = new List<GameObject>();

    [ContextMenu("Generate Site")] // これが右クリックメニューに出る魔法
    public void GenerateSite()
    {
        ClearSite(); // 生成前に一度きれいに消す

        // 床の生成
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Ground";
        floor.transform.localScale = new Vector3(siteSize / 10, 1, siteSize / 10);
        spawnedObjects.Add(floor);

        // 障害物の生成
        for (int i = 0; i < obstacleCount; i++)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Barrier_" + i;
            float x = Random.Range(-siteSize / 2, siteSize / 2);
            float z = Random.Range(-siteSize / 2, siteSize / 2);
            obstacle.transform.position = new Vector3(x, 1f, z);
            obstacle.transform.localScale = new Vector3(2f, 2f, 2f);
            spawnedObjects.Add(obstacle); // リストに追加
        }
    }

    [ContextMenu("Clear Site")] // これも右クリックメニューに出る
    public void ClearSite()
    {
        // リストの中身をすべて消す
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        spawnedObjects.Clear();
    }
}