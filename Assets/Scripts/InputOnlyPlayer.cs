using UnityEngine.InputSystem;
using UnityEngine;
using static GameEvents;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
public class InputOnlyPlayer : MonoBehaviour
{
    public PlayerData playerData = new PlayerData();

    private void Awake()
    {
        var input = GetComponent<PlayerInput>();

        playerData.input = input;
        playerData.playerIndex = -1;
        playerData.devices = new List<InputDevice>(input.devices);
        playerData.controlScheme = input.currentControlScheme;

        DontDestroyOnLoad(this.gameObject);

        PlayerEvents.OnPlayerRegistered?.Invoke(this);
    }
    public void Delete()
    {
        Destroy(gameObject);
    }
}