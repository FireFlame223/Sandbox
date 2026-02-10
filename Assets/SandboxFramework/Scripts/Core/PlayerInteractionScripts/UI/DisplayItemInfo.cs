using UnityEngine;
using TMPro;

/// <summary>
/// Displays information about the currently selected item, 
/// but only when no drag operation is active.
/// </summary>
[DisallowMultipleComponent]
public class DisplayItemInfo : MonoBehaviour
{
    [Tooltip("Reference to the SelectionHandler managing current selection")]
    public SelectionHandler selectionHandler;

    [Tooltip("Reference to the DragHandler managing drag state")]
    public DragHandler dragHandler;

    [Tooltip("UI Text element displaying the item name")]
    public TextMeshProUGUI itemName;

    [Tooltip("UI Text element displaying the item description")]
    public TextMeshProUGUI itemDescription;

    private void Update()
    {
        // Only update displayed info if no drag is active
        GameObject selected = null;
        if (dragHandler == null || dragHandler.CurrentState == DragState.Idle)
        {
            selected = selectionHandler != null ? selectionHandler.currentSelection : null;
        }

        UpdateItemInfo(selected);
    }

    /// <summary>
    /// Updates the UI text elements based on the provided item.
    /// Disables UI text if the item is null or UI references are missing.
    /// </summary>
    /// <param name="item">The GameObject to display info for</param>
    private void UpdateItemInfo(GameObject item)
    {
        if (itemName == null || itemDescription == null)
            return;

        if (item != null)
        {
            // Get name and description, if description is unavailable fallback to empty string
            string name = item.name;
            string description = item.GetComponent<Selectable>()?.ObjectDescription ?? string.Empty;

            itemName.enabled = true;
            itemDescription.enabled = true;

            itemName.SetText(name);
            itemDescription.SetText(description);
        }
        else
        {
            // Hide text if no item is selected
            itemName.enabled = false;
            itemDescription.enabled = false;
        }
    }
}
