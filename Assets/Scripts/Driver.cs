using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Driver : MonoBehaviour
{
    [SerializeField] float currentSpeed = 5f;
    [SerializeField] float steerSpeed = 200f;
    [SerializeField] float boostSpeed = 10f;
    [SerializeField] float normalSpeed = 5f;
    [SerializeField] TMP_Text boostText;

    void Start()
    {
        MobileControlsUI.EnsureExists();
        boostText.gameObject.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Boost"))
        {
            currentSpeed = boostSpeed;
            boostText.gameObject.SetActive(true);
            Destroy(collision.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("WorldCollision"))
        {
            currentSpeed = normalSpeed;
            boostText.gameObject.SetActive(false);
        }

    }
    


    void Update()
    {
        float move = 0f;
        float steer = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.wKey.isPressed)
        {
            move = 1f;
        }
        if (keyboard != null && keyboard.sKey.isPressed)
        {
            move = -1f;
        }


        if (keyboard != null && keyboard.aKey.isPressed)
        {
            steer = 1f;
        }
        if (keyboard != null && keyboard.dKey.isPressed)
        {
            steer = -1f;
        }

        // Touch and keyboard input share the existing movement code below.
        move = Mathf.Clamp(move + MobileControlsUI.MoveInput, -1f, 1f);
        steer = Mathf.Clamp(steer + MobileControlsUI.SteerInput, -1f, 1f);

        float moveAmount=  move*currentSpeed*Time.deltaTime; 
        float steerAmount=steer*steerSpeed*Time.deltaTime;

        transform.Translate(0,moveAmount,0);
        transform.Rotate(0,0,steerAmount);

    }
}
