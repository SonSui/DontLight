using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public List<GameObject> MapPrefabs;

    private int[,] mapData;

    // �}�b�v�̃X�^�[�g�ʒu�i����̊�_�Ȃǁj
    public Vector3 mapOrigin = new Vector3(-25f, 0.5f, 0f);
    public GameObject mapParent; //�������̐e�I�u�W�F�N�g ���ꂪ�Ȃ��ƃq�G�����L�[�����Â炢
    private void Start()
    {
        LoadMapDataFromText("map"); // Resources/map.txt
        GenerateMap();
    }

    void LoadMapDataFromText(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if (textAsset == null)
        {
            Debug.LogError("�}�b�v�t�@�C����������܂���: " + fileName);
            return;
        }

        string[] lines = textAsset.text.Split('\n');
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
                if (mapData[z, x] != 0)
                {
                    GameObject prefab = MapPrefabs[mapData[z, x] - 1];
                    Vector3 pos = new Vector3(
                        mapOrigin.x - x,
                        mapOrigin.y,
                        mapOrigin.z + z
                    );
                    GameObject obj = Instantiate(prefab, pos, prefab.transform.rotation);
                    obj.transform.parent = mapParent.transform;
                }
            }
        }
    }
}