using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles selection of objects using raycasting on components with a Selectable component.
/// Selected objects are temporarily placed on a separate layer for highlighting,
/// and their original layers are restored when deselected.
/// </summary>
[DisallowMultipleComponent]
public class SelectionHandler : MonoBehaviour
{
    [Header("Selection Settings")]
    [Tooltip("Layer used to visually highlight selected objects.")]
    public string selectionLayerName = "Selection";

    public float raycastDistance = 8f;

    public GameObject currentSelection { get; private set; }

    private Camera cam;
    private bool selectionLocked = false;
    private int selectionLayer;
    private readonly Dictionary<GameObject, int> originalLayers = new();

    private float selectionLockedTimer = 0f;

    private void Start()
    {
        cam = Camera.main;
        selectionLayer = LayerMask.NameToLayer(selectionLayerName);
    }

    private void Update()
    {
        if (selectionLocked)
            return;

        if (selectionLockedTimer > 0f)
        {
            selectionLockedTimer -= Time.deltaTime;
            return;
        }

        GameObject hoveredObject = GetSelectableUnderCursor();

        if (hoveredObject != null)
            SetSelection(hoveredObject);
        else
            ClearSelection();
    }

    /// <summary>
    /// Prevents changes to the current selection.
    /// </summary>
    public void LockSelection() => selectionLocked = true;

    /// <summary>
    /// Allows the selection to be updated again.
    /// </summary>
    public void UnlockSelection() => selectionLocked = false;

    /// <summary>
    /// Clears the current selection and removes visual highlights.
    /// </summary>
    public void ClearSelection() => SetSelection(null);

    /// <summary>
    /// Sets the current selection and applies the selection layer for visual feedback.
    /// If ShowHierarchy is active, applies the highlight to the root object instead.
    /// </summary>
    /// <param name="newSelection">The newly selected object.</param>
    private void SetSelection(GameObject newSelection)
    {
        currentSelection = newSelection;
        bool showHierarchy = InputSystem.GetButton(InputButton.ShowHierarchy);

        GameObject target = newSelection;
        if (showHierarchy && newSelection != null)
            target = newSelection.transform.root.gameObject;

        ApplySelectionLayer(target, showHierarchy);
    }

    /// <summary>
    /// Performs a raycast from the mouse position and returns the closest selectable object.
    /// </summary>
    /// <returns>The closest GameObject with a Selectable component, or null if none found.</returns>
    private GameObject GetSelectableUnderCursor()
    {
        Ray ray = cam.ScreenPointToRay(InputSystem.GetPointerPosition());
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance);

        GameObject closest = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            var selectable = hit.collider.GetComponentInParent<Selectable>();
            if (selectable == null || !selectable.enabled)
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closest = selectable.gameObject;
            }
        }

        return closest;
    }

    /// <summary>
    /// Highlights the target object by assigning it to the selection layer.
    /// Optionally includes all child objects.
    /// </summary>
    /// <param name="target">The GameObject to highlight.</param>
    /// <param name="recursive">Whether to apply the layer to all children.</param>
    /// <param name="timeToHighlight">Time in seconds to keep selection locked after highlighting.</param>
    public void HighlightObject(GameObject target, bool recursive = false, float timeToHighlight = 0.5f)
    {
        ApplySelectionLayer(target, recursive);
        selectionLockedTimer = timeToHighlight;
    }

    /// <summary>
    /// Applies the selection layer to the target object and (optionally) its children.
    /// Remembers original layers so they can be restored.
    /// </summary>
    /// <param name="target">The GameObject to highlight.</param>
    /// <param name="recursive">Whether to apply the layer recursively to all children.</param>
    private void ApplySelectionLayer(GameObject target, bool recursive)
    {
        RestoreOriginalLayers();
        if (target == null) return;

        var stack = new Stack<GameObject>();
        stack.Push(target);

        while (stack.Count > 0)
        {
            var obj = stack.Pop();

            // If not recursive and this is a child with a Selectable, skip it
            if (!recursive && obj != target && obj.GetComponent<Selectable>())
                continue;

            if (!originalLayers.ContainsKey(obj))
                originalLayers[obj] = obj.layer;

            obj.layer = selectionLayer;

            foreach (Transform child in obj.transform)
                stack.Push(child.gameObject);
        }
    }

    /// <summary>
    /// Restores the original layers of all objects that were previously highlighted.
    /// </summary>
    private void RestoreOriginalLayers()
    {
        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null)
                kvp.Key.layer = kvp.Value;
        }

        originalLayers.Clear();
    }
}
