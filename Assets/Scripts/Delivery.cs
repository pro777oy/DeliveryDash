using UnityEngine;

public class Delivery : MonoBehaviour
{
    bool hasPackage;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Package"))
        {
            Debug.Log("Picked up package");
            hasPackage = true;
            Destroy(collision.gameObject, 0.5f);
        }
        
        if (collision.gameObject.CompareTag("Customer") && hasPackage)
        {
            Debug.Log("Delivered package");
            hasPackage = false;
        }

    }
}
