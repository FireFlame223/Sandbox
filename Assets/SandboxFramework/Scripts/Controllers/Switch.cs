using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;

/// <summary>
/// Represents a switch that can activate, deactivate or toggle IActivatable components
/// within the welded structure based on an activation group color.
/// </summary>
[DisallowMultipleComponent]
public class Switch : MonoBehaviour
{
    [Tooltip("Only targets with a matching activationGroup color will respond.")]
    public Color activationGroup = Color.white;

    [Tooltip("Will find any listener in the scene, even ones that are not connected.")]
    public bool shouldBroadcast = false;

    /// <summary>
    /// Activates all matching and currently inactive IActivatable components within the welded structure.
    /// </summary>
    public void TurnOn()
    {
        if (!enabled) return;

        foreach (var target in FindAllActivatables())
        {
            // Activate only if inactive and matching the activation group
            if (!target.IsActive() && target.MatchActivationGroup(activationGroup))
            {
                target.OnActivate();
            }
        }
    }

    /// <summary>
    /// Deactivates all currently active IActivatable components within the welded structure.
    /// </summary>
    public void TurnOff()
    {
        if (!enabled) return;

        foreach (var target in FindAllActivatables())
        {
            if (target.IsActive())
            {
                target.OnDeactivate();
            }
        }
    }

    /// <summary>
    /// Toggles the activation state of all IActivatable components within the welded structure.
    /// Only activates those matching the activation group when toggling on.
    /// </summary>
    public void Toggle()
    {
        if (!enabled) return;

        foreach (var target in FindAllActivatables())
        {
            if (target.IsActive())
            {
                target.OnDeactivate();
            }
            else if (target.MatchActivationGroup(activationGroup))
            {
                target.OnActivate();
            }
        }
    }

    /// <summary>
    /// Forces a reactivation cycle: first turns off all active components,
    /// then turns on all matching inactive components.
    /// </summary>
    public void Reactivate()
    {
        if (!enabled) return;

        TurnOff();
        TurnOn();
    }

    /// <summary>
    /// Retrieves all IActivatable components in this object's root welded hierarchy,
    /// including inactive ones.
    /// </summary>
    /// <returns>List of all IActivatable components found</returns>
    private List<IActivatable> FindAllActivatables()
    {
        var result = new List<IActivatable>();

        if (shouldBroadcast)
        {
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var script in allMonoBehaviours)
            {
                if (script is IActivatable activatable)
                {
                    result.Add(activatable);
                }
            }
            return result;
        }

        var scannedRoots = new HashSet<Transform>();
        var addedComponents = new HashSet<IActivatable>();

        void ScanRoot(Transform root)
        {
            if (root == null || !scannedRoots.Add(root))
                return;

            foreach (var comp in root.GetComponentsInChildren<IActivatable>(true))
            {
                if (comp != null && addedComponents.Add(comp))
                {
                    result.Add(comp);
                }
            }
        }

        ScanRoot(transform.root);

        var weldable = GetComponent<Weldable>();
        if (weldable != null)
        {
            foreach (var other in weldable.GetAllConnectedRecursive())
            {
                ScanRoot(other.transform.root);
            }
        }

        return result;
    }
}
