using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public LayerMask obstacleMask;//��Q���̃��C���[�}�X�N

    private List<Bulb> bulbs = new List<Bulb>();
    private List<Flashlight> flashlights = new List<Flashlight>();

    public int bulbMaxCount = 16; // �ő�d����

    private void OnEnable()
    {
        GameEvents.Light.OnPointLightCreated += AddLight;
        GameEvents.Light.OnPointLightDestroyed += RemoveLight;
        GameEvents.Light.OnFlashlightCreated += AddFlashlight;
        GameEvents.Light.OnFlashlightDestroyed += RemoveFlashlight;
    }

    private void OnDisable()
    {
        GameEvents.Light.OnPointLightCreated -= AddLight;
        GameEvents.Light.OnPointLightDestroyed -= RemoveLight;
        GameEvents.Light.OnFlashlightCreated -= AddFlashlight;
        GameEvents.Light.OnFlashlightDestroyed -= RemoveFlashlight;
    }

    private void Start()
    {
        bulbMaxCount = GameManager.Instance?.GetMaxBulbCount() ?? bulbMaxCount; // GameManager����ő�d�������擾
    }
    private void AddLight(Bulb light)
    {
        if (!bulbs.Contains(light))
        {
            bulbs.Add(light);
            // �d�����쐬���ꂽ���Ƃ�ʒm
            Debug.Log("Spotlight created: " + light.name);
            // �d���̐����ő吔�𒴂����ꍇ�A�ł��Â��d�����폜
            if (bulbs.Count > bulbMaxCount)
            {
                int count = bulbs.Count - bulbMaxCount;
                for (int i = 0; i < count; i++)
                {
                    if(bulbs.Count <= i) break; // ���S�`�F�b�N
                    bulbs[i].Extinguish(); // �d��������
                    Debug.Log("Removed oldest spotlight: " + bulbs[i].name);
                }
            }
        }
    }

    private void RemoveLight(Bulb light)
    {
        bulbs.Remove(light);
        // �d�����j�󂳂ꂽ���Ƃ�ʒm
        Debug.Log("Spotlight destroyed: " + light.name);
    }

    private void AddFlashlight(Flashlight flashlight)
    {
        if (!flashlights.Contains(flashlight))
        {
            flashlights.Add(flashlight);
            // �����d�����쐬���ꂽ���Ƃ�ʒm
            Debug.Log("Flashlight created: " + flashlight.name);
        }
    }
    private void RemoveFlashlight(Flashlight flashlight)
    {
        flashlights.Remove(flashlight);
        // �����d�����j�󂳂ꂽ���Ƃ�ʒm
        Debug.Log("Flashlight destroyed: " + flashlight.name);
    }


    private void Update()
    {
        var players = GameEvents.PlayerEvents.OnQueryAllPlayers?.Invoke();
        if (players == null) return;

        // Dictionary �� List �ɕϊ����ė񋓂����S�ɂ���
        var playerList = new List<KeyValuePair<GameObject, PlayerData>>(players);

        foreach (var light in bulbs)
        {
            foreach (var player in playerList)
            {
                if (player.Key == null) continue;
                light.CheckPlayer(player.Key, obstacleMask);
            }
        }

        foreach (var flashlight in flashlights)
        {
            foreach (var player in playerList)
            {
                if (player.Key == null) continue;
                flashlight.CheckPlayer(player.Key, obstacleMask);
            }
        }
    }
}
