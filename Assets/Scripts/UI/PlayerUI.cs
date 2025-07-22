using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public HPUI hpUI; // プレイヤーのHP UI
    public BulbUI bulbUI; // プレイヤーのBulb UI
    public BatteryUI batteryUI; // プレイヤーのBattery UI

    
    public void TestUpdateHP()
    {
        // テスト用のHP更新
        HPInfo hpInfo = new HPInfo(100f, 100f, false, 100f);
        hpUI.UpdateHPBar(hpInfo);
        HPInfo hpInfo2 = new HPInfo(0f, 80f);
        hpUI.UpdateHPBar(hpInfo2);
    }
    public void TestUpdateHP2()
    {
        // テスト用のHP更新（ダメージ）
        HPInfo hpInfo = new HPInfo(100f, 100f, false, 100f);
        hpUI.UpdateHPBar(hpInfo);
    }
    public void TestUpdateBulb()
    {
        // テスト用のBulb更新
        bulbUI.SetBulbState(true);
        bulbUI.SetBulbState(false);
    }
    public void TestUpdateBattery()
    {
        // テスト用のBattery更新
        batteryUI.UpdateBattery(100f,false,100f);
        batteryUI.UpdateBattery(5f,false);
    }
    public void TestUpdateBattery2()
    {
        // テスト用のBattery更新（充電）
        batteryUI.UpdateBattery(50f, true);
    }
    
}
