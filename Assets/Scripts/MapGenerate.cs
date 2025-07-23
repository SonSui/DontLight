using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public List<GameObject> MapPrefabs;

    private int[,] mapData;

    // マップのスタート位置（左上の基準点など）
    public Vector3 mapOrigin = new Vector3(-25f, 0.5f, 0f);
    public GameObject mapParent; // 生成物の親オブジェクト

    public TextAsset mapTextFile;

    private void Start()
    {
        if (mapTextFile == null)
        {
            Debug.LogError("マップテキストファイルが指定されていません。");
            return;
        }
        LoadMapDataFromTextAsset(mapTextFile);
        GenerateMap();
    }

    void LoadMapDataFromTextAsset(TextAsset textAsset)
    {
        string[] lines = textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        int height = lines.Length;
        int width = lines[0].Trim().Length;

        mapData = new int[height, width];

        for (int z = 0; z < height; z++)
        {
            string line = lines[z].Trim();
            for (int x = 0; x < width; x++)
            {
                char c = line[x];
                mapData[z, x] = c - '0';
            }
        }
    }

    void GenerateMap()
    {
        for (int z = 0; z < mapData.GetLength(0); z++)
        {
            for (int x = 0; x < mapData.GetLength(1); x++)
            {
                int tile = mapData[z, x];
                if (tile == 0) continue;

                GameObject prefab = MapPrefabs[tile - 1];
                Vector3 basePos = new Vector3(
                    mapOrigin.x + x,
                    mapOrigin.y,
                    mapOrigin.z - z
                );

                Vector3 offset = Vector3.zero;

                if (tile == 3)
                {
                    offset = new Vector3(0.4f, 0f, 0f);
                }
                if(tile == 4)
                {
                    offset = new Vector3(-0.2f, 0f, 0f);
                }
                if (tile == 5)
                {
                    offset = new Vector3(-0.2f, 0f, 0.2f);
                }
                if (tile == 6)
                {
                    offset = new Vector3(0.2f, 0f, 0.4f);
                }
                Quaternion rotation = prefab.transform.rotation;
                GameObject obj = Instantiate(prefab, basePos + offset, rotation);
                obj.transform.parent = mapParent.transform;
            }
        }
    }
}