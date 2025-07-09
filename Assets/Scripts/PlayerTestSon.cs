using UnityEngine;

public class PlayerTestSon : MonoBehaviour
{
   
    private void Start()
    {
        
        GameEvents.PlayerEvents.OnPlayerSpawned?.Invoke(this.gameObject);
        Invoke("Die", 50f); // Simulate player death after 30 seconds
    }

    public void Die()
    {
        
        GameEvents.PlayerEvents.OnPlayerDied?.Invoke(this.gameObject);
        Destroy(gameObject);
    }

}

