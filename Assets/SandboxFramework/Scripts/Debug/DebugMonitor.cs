using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// DebugMonitor creates an in-game debug console overlay to display custom logs
/// and live state machine statuses. It appears as a UI panel and supports clearing logs
/// via Shift+C. Optionally shows timestamps and can be configured for position and size.
/// </summary>
[DisallowMultipleComponent]
public class DebugMonitor : MonoBehaviour
{
    [Header("Debug Window Settings")]
    public bool enableOnStart = true;                   // Whether the monitor is visible at start
    [Range(0.1f, 1f)] public float widthPercent = 0.4f;  // Width of the panel relative to screen width
    [Range(0.1f, 1f)] public float heightPercent = 0.3f; // Height of the panel relative to screen height
    public Anchor anchorPosition = Anchor.BottomLeft;   // Where to position the panel on screen

    [Header("Log Settings")]
    public bool showTimestamp = true;                   // Whether to show a timestamp on each log line

    [Header("State Machines to Watch")]
    public StateMachine stateMachine1;
    public StateMachine stateMachine2;
    public StateMachine stateMachine3;

    private TextMeshProUGUI outputText;                 // The text component displaying log lines
    private RectTransform panel;                        // The UI panel container
    private readonly Queue<string> logLines = new();    // Queue to store visible log lines
    private int maxLines = 0;                           // Max number of lines visible on screen

    public enum Anchor { TopLeft, TopRight, BottomLeft, BottomRight }

    void Start()
    {
        if (enableOnStart)
        {
            Initialize();  // Create UI on start if enabled
        }
    }

    // Sets up the UI panel and initial message
    void Initialize()
    {
        CreateUI();
        LogInfo("Press SHIFT+C to clear the console.");
    }

    void Update()
    {
        if (outputText == null) return;

        // Clear log on Shift + C
        if (Keyboard.current.cKey.wasPressedThisFrame && Keyboard.current.leftShiftKey.isPressed)
        {
            Clear();
        }

        // Gather state machine info if assigned
        string stateInfo = "";
        if (stateMachine1 != null) stateInfo += $"[1] <b>{stateMachine1.name}</b>: {stateMachine1.CurrentState}\n";
        if (stateMachine2 != null) stateInfo += $"[2] <b>{stateMachine2.name}</b>: {stateMachine2.CurrentState}\n";
        if (stateMachine3 != null) stateInfo += $"[3] <b>{stateMachine3.name}</b>: {stateMachine3.CurrentState}\n";

        // Fill lines if fewer than max
        while (logLines.Count < maxLines)
        {
            logLines.Enqueue("");
        }

        // Update text display
        outputText.text = string.Join("\n", logLines) + $"\n<size=80%><i>States</i>\n{stateInfo}</size>";
    }

    // Public logging methods
    public void Log(string message) => AddLine(Wrap("INFO", message, "white"));
    public void Log(float number) => Log(number.ToString("F2"));
    public void Log(int number) => Log(number.ToString());
    public void Log(bool value) => Log(value.ToString());
    public void LogInfo(string message) => AddLine(Wrap("INFO", message, "white"));
    public void LogWarning(string message) => AddLine(Wrap("WARN", message, "yellow"));
    public void LogError(string message) => AddLine(Wrap("ERROR", message, "red"));
    public void Clear() => logLines.Clear();

    // Adds a new line to the log queue
    private void AddLine(string line)
    {
        if (outputText == null) Initialize();
        if (logLines.Count >= maxLines)
            logLines.Dequeue(); // Remove oldest line
        logLines.Enqueue(line);
    }

    // Wraps a log message in formatting (timestamp, tag, color)
    private string Wrap(string tag, string message, string color)
    {
        string time = showTimestamp ? $"[{Time.time:0.00}] " : "";
        return $"<color={color}>{time}[{tag}] {message}</color>";
    }

    // Dynamically creates the debug panel and text UI
    private void CreateUI()
    {
        // Try to find an existing canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            // If none found, create a new overlay canvas
            GameObject canvasGO = new GameObject("DebugCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create panel object
        GameObject panelGO = new GameObject("DebugPanel", typeof(Image));
        panel = panelGO.GetComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);
        panel.anchorMin = GetAnchorMin();
        panel.anchorMax = GetAnchorMax();
        panel.pivot = panel.anchorMin;
        panel.sizeDelta = new Vector2(Screen.width * widthPercent, Screen.height * heightPercent);
        panel.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f); // semi-transparent background

        // Create text object inside the panel
        GameObject textGO = new GameObject("DebugText", typeof(TextMeshProUGUI));
        outputText = textGO.GetComponent<TextMeshProUGUI>();
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textGO.transform.SetParent(panel, false);
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(10, 10);
        textRT.offsetMax = new Vector2(-10, -10);
        outputText.textWrappingMode = TextWrappingModes.Normal;
        outputText.richText = true;
        outputText.fontSize = 12;
        outputText.text = "Debug Output...";

        // Disable raycast blocking so this overlay doesn't interfere with UI
        panel.GetComponent<Image>().raycastTarget = false;
        outputText.raycastTarget = false;

        // Calculate max visible lines based on panel height
        float lineHeight = outputText.fontSize * 1.2f;
        float availableHeight = panel.sizeDelta.y;
        maxLines = Mathf.FloorToInt(availableHeight / lineHeight) - 2;
    }

    // Returns anchor position as min vector
    private Vector2 GetAnchorMin()
    {
        return anchorPosition switch
        {
            Anchor.TopLeft => new Vector2(0, 1),
            Anchor.TopRight => new Vector2(1, 1),
            Anchor.BottomRight => new Vector2(1, 0),
            _ => new Vector2(0, 0),
        };
    }

    // Returns anchor position as max vector (same as min for fixed corner anchoring)
    private Vector2 GetAnchorMax() => GetAnchorMin();
}
