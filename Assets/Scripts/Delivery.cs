using UnityEngine;
using TMPro;

public class Delivery : MonoBehaviour
{
    bool hasPackage;
    bool gameIsOver;
    int completedDeliveries;
    int totalDeliveries;

    [SerializeField] float destroyDelay = 0.3f;
    [SerializeField] TMP_Text deliveryCountText;
    [SerializeField] TMP_Text gameOverText;

    void Start()
    {
        totalDeliveries = GameObject.FindGameObjectsWithTag("Customer").Length;
        CreateUIIfNeeded();
        UpdateDeliveryCount();

        gameOverText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameIsOver)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Package") && !hasPackage)
        {
            Debug.Log("Picked up package");
            hasPackage = true;
            GetComponent<ParticleSystem>().Play();
            Destroy(collision.gameObject, destroyDelay);
        }
        
        if (collision.gameObject.CompareTag("Customer") && hasPackage)
        {
            Debug.Log("Delivered package");
            hasPackage = false;
            GetComponent<ParticleSystem>().Stop();
            completedDeliveries++;
            UpdateDeliveryCount();
            Destroy(collision.gameObject, destroyDelay);

            if (completedDeliveries >= totalDeliveries)
            {
                EndGame();
            }
        } 
    }

    void UpdateDeliveryCount()
    {
        deliveryCountText.text = $"Deliveries: {completedDeliveries} / {totalDeliveries}";
    }

    void EndGame()
    {
        gameIsOver = true;
        gameOverText.text = $"GAME OVER\nAll {completedDeliveries} deliveries completed!";
        gameOverText.gameObject.SetActive(true);

        Driver driver = GetComponent<Driver>();
        if (driver != null)
        {
            driver.enabled = false;
        }
    }

    void CreateUIIfNeeded()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Game UI", typeof(Canvas));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (deliveryCountText == null)
        {
            deliveryCountText = CreateText(canvas.transform, "Delivery Count");
            RectTransform rect = deliveryCountText.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(420f, 60f);
            deliveryCountText.alignment = TextAlignmentOptions.TopRight;
            deliveryCountText.fontSize = 32f;
        }

        if (gameOverText == null)
        {
            gameOverText = CreateText(canvas.transform, "Game Over Text");
            RectTransform rect = gameOverText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800f, 220f);
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.fontSize = 52f;
            gameOverText.fontStyle = FontStyles.Bold;
            gameOverText.color = new Color(1f, 0.85f, 0.15f);
        }
    }

    TMP_Text CreateText(Transform parent, string objectName)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        textObject.layer = LayerMask.NameToLayer("UI");
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.raycastTarget = false;
        return text;
    }
}
