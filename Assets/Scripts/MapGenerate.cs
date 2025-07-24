using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public List<GameObject> MapPrefabs,TilePrefabs;

    private int[,] mapData;
    private int[,] tileData;

    // マップのスタート位置（左上の基準点など）
    public Vector3 mapOrigin = new Vector3(-25f, 0.5f, 0f);
    public GameObject mapParent; // 生成物の親オブジェクト

    public TextAsset mapTextFile,tileTextFile;

    private void Start()
    {
        if (mapTextFile == null || tileTextFile == null)
        {
            Debug.LogError("マップテキストファイルが指定されていません。");
            return;
        }
        mapData = LoadMapDataFromTextAsset(mapTextFile); 
        tileData = LoadMapDataFromTextAsset(tileTextFile);

        GenerateObjectMap();
        GenerateTileMap();
    }

    int[,] LoadMapDataFromTextAsset(TextAsset textAsset)
    {
        string[] lines = textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        int height = lines.Length;
        int width = lines[0].Trim().Length;

        int[,] result = new int[height, width];

        for (int z = 0; z < height; z++)
        {
            string line = lines[z].Trim();
            for (int x = 0; x < width; x++)
            {
                char c = line[x];
                result[z, x] = c - '0';
            }
        }

        return result;
    }

    void GenerateTileMap()
    {
        for (int z = 0; z < tileData.GetLength(0); z++)
        {
            for (int x = 0; x < tileData.GetLength(1); x++)
            {
                int tile = tileData[z, x];
                if (tile == 0) continue;

                if (tile - 1 >= TilePrefabs.Count)
                {
                    Debug.LogWarning($"TilePrefabsの数を超えたID：{tile}");
                    continue;
                }

                GameObject prefab = TilePrefabs[tile - 1];
                Vector3 pos = new Vector3(
                    mapOrigin.x + x - 10f,
                    mapOrigin.y - 0.7f, // オブジェクトよりちょっと下
                    mapOrigin.z - z
                );
                GameObject obj = Instantiate(prefab, pos, prefab.transform.rotation);
                obj.transform.parent = mapParent.transform;
            }
        }
    }

    void GenerateObjectMap()
    {
        for (int z = 0; z < mapData.GetLength(0); z++)
        {
            for (int x = 0; x < mapData.GetLength(1); x++)
            {
                int tile = mapData[z, x];
                if (tile == 0) continue;

                if (tile - 1 >= MapPrefabs.Count)
                {
                    Debug.LogWarning($"MapPrefabsの数を超えたID：{tile}");
                    continue;
                }

                GameObject prefab = MapPrefabs[tile - 1];
                Vector3 basePos = new Vector3(
                    mapOrigin.x + x,
                    mapOrigin.y,
                    mapOrigin.z - z
                );

                Vector3 offset = Vector3.zero;
                if (tile == 3) offset = new Vector3(0.4f, 0f, 0f);
                if (tile == 4) offset = new Vector3(-0.2f, 0f, 0f);
                if (tile == 5) offset = new Vector3(-0.2f, 0f, 0.2f);
                if (tile == 6) offset = new Vector3(0.2f, 0f, 0.4f);

                GameObject obj = Instantiate(prefab, basePos + offset, prefab.transform.rotation);
                obj.transform.parent = mapParent.transform;
            }
        }
    }
}