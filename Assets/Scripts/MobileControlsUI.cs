using System.Collections.Generic;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Creates a small, responsive touch-control overlay on the scene's existing Canvas.
/// The overlay is visible on touch/mobile devices and in the Editor for easy testing.
/// </summary>
public class MobileControlsUI : MonoBehaviour
{
    public static float MoveInput => GetAxis(ControlAction.Reverse, ControlAction.Forward);
    public static float SteerInput => GetAxis(ControlAction.Right, ControlAction.Left);

    static MobileControlsUI instance;

    readonly Dictionary<ControlAction, HashSet<int>> pressedPointers = new();
    RectTransform safeAreaTransform;
    Rect lastSafeArea;
    Vector2Int lastScreenSize;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern int IsMobileBrowser();
#endif

    public static void EnsureExists()
    {
        if (instance != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "Game UI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        instance = canvas.gameObject.AddComponent<MobileControlsUI>();
    }

    static float GetAxis(ControlAction negative, ControlAction positive)
    {
        if (instance == null)
        {
            return 0f;
        }

        bool negativePressed = instance.IsPressed(negative);
        bool positivePressed = instance.IsPressed(positive);
        return (positivePressed ? 1f : 0f) - (negativePressed ? 1f : 0f);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        ConfigureCanvas();
        CreateControls();
        UpdateSafeArea();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height || lastSafeArea != Screen.safeArea)
        {
            UpdateSafeArea();
        }

        // A touch device can appear after startup in WebGL, so update visibility lazily.
        safeAreaTransform.gameObject.SetActive(ShouldShowControls());
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ReleaseAll();
        }
    }

    void OnDisable()
    {
        ReleaseAll();
    }

    void ConfigureCanvas()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    void CreateControls()
    {
        safeAreaTransform = CreateRect("Mobile Controls (Safe Area)", transform);
        safeAreaTransform.offsetMin = Vector2.zero;
        safeAreaTransform.offsetMax = Vector2.zero;

        RectTransform steering = CreateCornerGroup("Steering", safeAreaTransform, false);
        CreateButton(steering, "Left", "\u25c0", ControlAction.Left, new Vector2(0f, 0f));
        CreateButton(steering, "Right", "\u25b6", ControlAction.Right, new Vector2(180f, 0f));

        RectTransform pedals = CreateCornerGroup("Pedals", safeAreaTransform, true);
        CreateButton(pedals, "Reverse / Brake", "REV", ControlAction.Reverse, new Vector2(-180f, 0f));
        CreateButton(pedals, "Forward", "GO", ControlAction.Forward, new Vector2(0f, 0f));
    }

    RectTransform CreateCornerGroup(string objectName, Transform parent, bool rightAligned)
    {
        RectTransform rect = CreateRect(objectName, parent);
        Vector2 anchor = new Vector2(rightAligned ? 1f : 0f, 0f);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = new Vector2(rightAligned ? -48f : 48f, 48f);
        rect.sizeDelta = new Vector2(340f, 160f);
        return rect;
    }

    void CreateButton(Transform parent, string objectName, string label, ControlAction action, Vector2 position)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TouchHoldButton));
        buttonObject.layer = LayerMask.NameToLayer("UI");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(160f, 160f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.08f, 0.12f, 0.72f);

        TouchHoldButton holdButton = buttonObject.GetComponent<TouchHoldButton>();
        holdButton.Initialize(this, action, image);

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.layer = LayerMask.NameToLayer("UI");
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = label.Length > 1 ? 38f : 64f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform));
        child.layer = LayerMask.NameToLayer("UI");
        child.transform.SetParent(parent, false);
        return child.GetComponent<RectTransform>();
    }

    void UpdateSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        safeAreaTransform.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        safeAreaTransform.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
    }

    bool ShouldShowControls()
    {
#if UNITY_EDITOR
        return true;
#elif UNITY_WEBGL
        return IsMobileBrowser() != 0;
#else
        return Application.isMobilePlatform || Touchscreen.current != null;
#endif
    }

    internal void SetPressed(ControlAction action, int pointerId, bool pressed)
    {
        if (!pressedPointers.TryGetValue(action, out HashSet<int> pointers))
        {
            pointers = new HashSet<int>();
            pressedPointers.Add(action, pointers);
        }

        if (pressed)
        {
            pointers.Add(pointerId);
        }
        else
        {
            pointers.Remove(pointerId);
        }
    }

    bool IsPressed(ControlAction action)
    {
        return pressedPointers.TryGetValue(action, out HashSet<int> pointers) && pointers.Count > 0;
    }

    void ReleaseAll()
    {
        pressedPointers.Clear();
    }

    internal enum ControlAction
    {
        Left,
        Right,
        Forward,
        Reverse
    }
}

public class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    MobileControlsUI controls;
    MobileControlsUI.ControlAction action;
    Image image;
    Color releasedColor;
    readonly HashSet<int> activePointers = new();

    internal void Initialize(MobileControlsUI owner, MobileControlsUI.ControlAction controlAction, Image targetImage)
    {
        controls = owner;
        action = controlAction;
        image = targetImage;
        releasedColor = image.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        activePointers.Add(eventData.pointerId);
        controls.SetPressed(action, eventData.pointerId, true);
        image.color = new Color(0.15f, 0.65f, 1f, 0.9f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release(eventData.pointerId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Release(eventData.pointerId);
    }

    void OnDisable()
    {
        foreach (int pointerId in activePointers)
        {
            controls?.SetPressed(action, pointerId, false);
        }

        activePointers.Clear();
        if (image != null)
        {
            image.color = releasedColor;
        }
    }

    void Release(int pointerId)
    {
        activePointers.Remove(pointerId);
        controls.SetPressed(action, pointerId, false);
        if (activePointers.Count == 0)
        {
            image.color = releasedColor;
        }
    }
}
