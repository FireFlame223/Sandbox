using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the crosshair UI position by following the mouse position.
/// </summary>
[DisallowMultipleComponent]
public class CrossHairPositioning : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        // Cache RectTransform and parent Canvas components for efficiency
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Crosshair follows the mouse position regardless of cursor lock state
        Vector2 localMousePos;
        bool validPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            InputSystem.GetPointerPosition(),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localMousePos);

        if (validPosition)
        {
            rectTransform.anchoredPosition = localMousePos;
        }
    }
}
