using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// SwitchListener responds to activation and deactivation events.
/// It groups switches by color and triggers UnityEvents on state change.
/// </summary>
public class SwitchListener : MonoBehaviour, IActivatable
{
    [Header("Switch Events")]

    /// <summary>
    /// Group identifier for this switch (using Color as ID).
    /// </summary>
    public Color switchGroup = Color.white;

    /// <summary>
    /// Event invoked when the switch turns on (activated).
    /// </summary>
    public UnityEvent onTurnOn;

    /// <summary>
    /// Event invoked when the switch turns off (deactivated).
    /// </summary>
    public UnityEvent onTurnOff;

    // Internal flag to track active state
    private bool isActive = false;

    /// <summary>
    /// Checks if the given group color matches this switch's group.
    /// </summary>
    /// <param name="group">Color to match.</param>
    /// <returns>True if matches, false otherwise.</returns>
    public bool MatchActivationGroup(Color group) => switchGroup == group;

    /// <summary>
    /// Activates the switch and invokes the onTurnOn event if not already active.
    /// </summary>
    public void OnActivate()
    {
        if (!enabled || isActive) return;

        isActive = true;
        onTurnOn?.Invoke();
    }

    /// <summary>
    /// Deactivates the switch and invokes the onTurnOff event if currently active.
    /// </summary>
    public void OnDeactivate()
    {
        if (!enabled || !isActive) return;

        isActive = false;
        onTurnOff?.Invoke();
    }

    /// <summary>
    /// Returns whether the switch is currently active.
    /// </summary>
    public bool IsActive() => isActive;
}
