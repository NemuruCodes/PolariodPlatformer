using UnityEngine;

public class PLRD_Hit : MonoBehaviour
{
    public PLRD_Manager PLRD;
    void OnTriggerEnter2D(Collider2D other)
    {
        PLRD.Lock();
    }
}
