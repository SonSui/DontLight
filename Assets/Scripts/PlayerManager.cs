using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<GameObject> allPlayers = new List<GameObject>();

    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnPlayerSpawned += AddPlayer;
        GameEvents.PlayerEvents.OnPlayerDied += RemovePlayer;
        GameEvents.PlayerEvents.OnTakeLightDamage += TakeDamage;
        GameEvents.PlayerEvents.OnQueryAllPlayers = GetAllPlayers;

    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnPlayerSpawned -= AddPlayer;
        GameEvents.PlayerEvents.OnPlayerDied -= RemovePlayer;
        GameEvents.PlayerEvents.OnTakeLightDamage -= TakeDamage;
        GameEvents.PlayerEvents.OnQueryAllPlayers = null; 
    }

    private void AddPlayer(GameObject player)
    {
        if (!allPlayers.Contains(player))
        {
            Debug.Log("Player added: " + player.name);
            allPlayers.Add(player);
        }
    }

    private void RemovePlayer(GameObject player)
    {
        Debug.Log("Player removed: " + player.name);
        allPlayers.Remove(player);
    }

    public List<GameObject> GetAllPlayers()
    {
        return allPlayers;
    }
    public void TakeDamage(GameObject player, float damage)
    {
        if (allPlayers.Contains(player))
        {
            
            Debug.Log($"Player {player.name} took {damage} damage.");
        }
        else
        {
            Debug.LogWarning($"Player {player.name} not found in the list.");
        }
    }
}