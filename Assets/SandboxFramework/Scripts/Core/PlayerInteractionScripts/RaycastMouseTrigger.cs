using UnityEngine;

/// <summary>
/// Casts a ray from the center of the screen and triggers mouse events
/// on objects that implement MouseEventInvoker when the left mouse button is used.
/// </summary>
[DisallowMultipleComponent]
public class RaycastMouseTrigger : MonoBehaviour
{
    [Tooltip("Maximum raycast distance.")]
    [SerializeField] private float rayDistance = 5f;

    /// <summary>
    /// Performs a raycast from the center of the screen and returns the MouseEventInvoker component if found.
    /// </summary>
    private MouseListener GetMouseEventTarget()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            return hit.collider.GetComponent<MouseListener>();
        }
        return null;
    }

    private void Update()
    {
        // Check for left mouse button press
        if (InputSystem.GetPointerDown())
        {
            GetMouseEventTarget()?.OnMouseDown();
        }

        // Check for left mouse button release
        if (InputSystem.GetPointerUp())
        {
            GetMouseEventTarget()?.OnMouseUp();
        }
    }
}
