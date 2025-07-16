using System;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;

public class GameEvents
{
    public static class PlayerEvents
    {
        public static Action<GameObject> OnPlayerSpawned; //�v���C���[���X�|�[�������Ƃ��ɌĂ΂��C�x���g
        public static Action<GameObject> OnPlayerDied;//�v���C���[�����S�����Ƃ��ɌĂ΂��C�x���g
        public static Func<List<GameObject>> OnQueryAllPlayers;//�S�Ẵv���C���[���擾���邽�߂̃C�x���g
        public static Action<GameObject, float> OnTakeLightDamage;//�v���C���[�����C�g����_���[�W���󂯂��Ƃ��ɌĂ΂��C�x���g
    }
    public static class Light
    {
        public static Action<Bulb> OnPointLightCreated;//�d�����쐬���ꂽ�Ƃ��ɌĂ΂��C�x���g
        public static Action<Bulb> OnPointLightDestroyed;//�d�����j�󂳂ꂽ�Ƃ��ɌĂ΂��C�x���g
    }

}
