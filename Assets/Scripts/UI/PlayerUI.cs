using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public HPUI hpUI; // �v���C���[��HP UI
    public BulbUI bulbUI; // �v���C���[��Bulb UI
    public BatteryUI batteryUI; // �v���C���[��Battery UI

    
    public void TestUpdateHP()
    {
        // �e�X�g�p��HP�X�V
        HPInfo hpInfo = new HPInfo(100f, 100f, false, 100f);
        hpUI.UpdateHPBar(hpInfo);
        HPInfo hpInfo2 = new HPInfo(0f, 80f);
        hpUI.UpdateHPBar(hpInfo2);
    }
    public void TestUpdateHP2()
    {
        // �e�X�g�p��HP�X�V�i�_���[�W�j
        HPInfo hpInfo = new HPInfo(100f, 100f, false, 100f);
        hpUI.UpdateHPBar(hpInfo);
    }
    public void TestUpdateBulb()
    {
        // �e�X�g�p��Bulb�X�V
        bulbUI.SetBulbState(true);
        bulbUI.SetBulbState(false);
    }
    public void TestUpdateBattery()
    {
        // �e�X�g�p��Battery�X�V
        batteryUI.UpdateBattery(100f,false,100f);
        batteryUI.UpdateBattery(5f,false);
    }
    public void TestUpdateBattery2()
    {
        // �e�X�g�p��Battery�X�V�i�[�d�j
        batteryUI.UpdateBattery(50f, true);
    }
    
}
