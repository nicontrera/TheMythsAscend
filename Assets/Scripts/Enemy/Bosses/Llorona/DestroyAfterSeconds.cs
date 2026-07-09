using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 5f);
    }
}
