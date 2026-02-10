using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Invokes UnityEvents when this GameObject receives mouse input events.
/// Requires a Collider component to register mouse events from Unity.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MouseListener : MonoBehaviour
{
    [Header("Mouse Events")]
    [Tooltip("Invoked when the mouse button is pressed down on this GameObject.")]
    public UnityEvent onMouseDownEvent;

    [Tooltip("Invoked when the mouse button is released over this GameObject.")]
    public UnityEvent onMouseUpEvent;

    /// <summary>
    /// Called automatically by Unity when the user presses the mouse button
    /// while the cursor is over this GameObject's collider.
    /// </summary>
    public void OnMouseDown()
    {
        if (!enabled) return;
        onMouseDownEvent?.Invoke();
    }

    /// <summary>
    /// Called automatically by Unity when the user releases the mouse button
    /// while the cursor is over this GameObject's collider.
    /// </summary>
    public void OnMouseUp()
    {
        if (!enabled) return;
        onMouseUpEvent?.Invoke();
    }
}
