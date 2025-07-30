using UnityEngine;

public class LockWorldYRotation : MonoBehaviour
{
    private float initialY;

    void Awake()
    {
        initialY = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        Vector3 worldEuler = transform.eulerAngles;
        worldEuler.y = initialY;
        transform.rotation = Quaternion.Euler(worldEuler);
    }
}