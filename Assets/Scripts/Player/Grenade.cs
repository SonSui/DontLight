using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float lifeTime = 15f;
    void Start()
    {
        StartCoroutine(DestroyAfterTime());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
