using UnityEngine;

public class Delivery : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Package"))
        {
            Debug.Log("Picked up package");
        }
        
        if (collision.gameObject.CompareTag("Customer"))
        {
            Debug.Log("Delivered package");
        }

    }
}
