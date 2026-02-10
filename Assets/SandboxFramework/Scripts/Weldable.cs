using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the type of welding behavior between objects.
/// </summary>
public enum WeldType
{
    Undefined,
    HierarchyBased,
    PhysicsBased
}

[DisallowMultipleComponent]
public class Weldable : MonoBehaviour
{
    private WeldType currentWeldType = WeldType.Undefined;
    private readonly HashSet<Weldable> connections = new();

    public WeldType weldType => currentWeldType;

    private IEnumerator Start()
    {
        yield return null;
        TryAutoHierarchyWeldWithAncestor();
    }

    /// <summary>
    /// Attempts to automatically weld this object to the first parent Weldable found in the hierarchy.
    /// </summary>
    private void TryAutoHierarchyWeldWithAncestor()
    {
        Transform current = transform.parent;

        while (current != null)
        {
            var parentWeldable = current.GetComponent<Weldable>();
            if (parentWeldable != null && !IsConnected(parentWeldable))
            {
                WeldTo(parentWeldable, WeldType.HierarchyBased, true);
                break;
            }

            current = current.parent;
        }
    }

    /// <summary>
    /// Welds this object to a target using the specified weld type.
    /// </summary>
    internal void WeldTo(Weldable target, WeldType weldType, bool isAutoWeld = false, Transform overlappingTransform = null)
    {
        if (!enabled || target == null || target == this)
            return;

        bool wasIsolated = connections.Count == 0;
        bool targetWasIsolated = target.connections.Count == 0;

        if (!TrySetWeldType(weldType) || !target.TrySetWeldType(weldType))
        {
            Debug.LogWarning($"Weld failed: type mismatch ({name} ↔ {target.name})");
            return;
        }

        if (IsConnected(target))
        {
            Debug.LogWarning("Already connected");
            return;
        }

        if (target.IsConnected(this))
        {
            Debug.LogError("One-sided connection detected");
        }

        AddConnection(target);
        target.AddConnection(this);

        if (weldType == WeldType.HierarchyBased && !isAutoWeld)
        {
            ApplyHierarchyWeld(target, overlappingTransform);
        }
        else if (weldType == WeldType.PhysicsBased)
        {
            ApplyPhysicsWeld(target);
        }

        NotifyOnWeld(wasIsolated);
        target.NotifyOnWeld(targetWasIsolated);
    }

    /// <summary>
    /// Unwelds this object from all connected weldables.
    /// </summary>
    internal void Unweld()
    {
        if (!enabled) return;

        bool wasGrouped = connections.Count > 0;
        NotifyOnUnweld(wasGrouped);

        List<Weldable> connectionsToRemove = new();

        foreach (var connection in connections)
        {
            connectionsToRemove.Add(connection);

            if (connection.connections.Remove(this))
            {
                bool connectionIsIsolatedAfterUnweld = connection.connections.Count == 0;
                connection.NotifyOnUnweld(connectionIsIsolatedAfterUnweld);
            }
        }

        if (currentWeldType == WeldType.HierarchyBased)
            RemoveHierarchyWelds(connections);
        else if (currentWeldType == WeldType.PhysicsBased)
            RemovePhysicsWelds(connections);

        connections.Clear();
        if (connections.Count < 1) currentWeldType = WeldType.Undefined;
    }

    /// <summary>
    /// Applies hierarchy-based welding by reparenting to the target.
    /// </summary>
    private void ApplyHierarchyWeld(Weldable target, Transform overlappingTransform)
    {
        Transform targetTransform = overlappingTransform ?? target.transform;

        if (transform.parent != null)
        {
            ReparentWeldableAncestors();
        }

        transform.SetParent(targetTransform, true);
    }

    /// <summary>
    /// Applies physics-based welding using custom joints.
    /// </summary>
    private void ApplyPhysicsWeld(Weldable target)
    {
        Rigidbody thisRb = GetOrAddRigidbody(gameObject);
        Rigidbody targetRb = GetOrAddRigidbody(target.gameObject);

        CustomFixedJoint joint = gameObject.AddComponent<CustomFixedJoint>();
        joint.targetTransform = target.transform;

        CustomFixedJoint joint2 = target.gameObject.AddComponent<CustomFixedJoint>();
        joint2.targetTransform = transform;
    }

    /// <summary>
    /// Removes hierarchy-based welds by unparenting connected weldables.
    /// </summary>
    private void RemoveHierarchyWelds(IEnumerable<Weldable> connected)
    {
        List<Weldable> children = GetChildWeldables();
        transform.SetParent(null, true);

        foreach (Weldable weldable in children)
        {
            weldable.transform.SetParent(null, true);
        }
    }

    /// <summary>
    /// Removes physics-based welds by destroying joint components.
    /// </summary>
    private void RemovePhysicsWelds(IEnumerable<Weldable> connected)
    {
        foreach (var other in connected)
        {
            foreach (var joint in other.GetComponents<CustomFixedJoint>())
            {
                if (joint.targetTransform == transform)
                    Destroy(joint);
            }
        }

        foreach (var joint in GetComponents<CustomFixedJoint>())
        {
            Destroy(joint);
        }
    }

    /// <summary>
    /// Reparents all weldable ancestors to create a clean hierarchy chain.
    /// </summary>
    private void ReparentWeldableAncestors()
    {
        Weldable root = this;
        if (root == null) return;

        List<Transform> weldableAncestors = new();
        Transform current = root.transform;

        while (current != null)
        {
            Weldable weldable = current.GetComponent<Weldable>();
            if (weldable)
            {
                weldableAncestors.Add(current);
            }
            current = current.parent;
        }

        foreach (Transform transform in weldableAncestors)
            transform.SetParent(null, true);

        for (int i = weldableAncestors.Count - 1; i > 0; i--)
            weldableAncestors[i].SetParent(weldableAncestors[i - 1], true);
    }

    /// <summary>
    /// Gets or adds a Rigidbody to a GameObject.
    /// </summary>
    private static Rigidbody GetOrAddRigidbody(GameObject obj)
    {
        var rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        return rb;
    }

    /// <summary>
    /// Returns the topmost Weldable in the parent hierarchy.
    /// </summary>
    private Weldable GetRootWeldable()
    {
        Transform current = transform;
        Weldable lastFound = null;

        while (current != null)
        {
            var weldable = current.GetComponent<Weldable>();
            if (weldable != null)
                lastFound = weldable;

            current = current.parent;
        }

        return lastFound;
    }

    /// <summary>
    /// Returns all Weldable components in the child hierarchy.
    /// </summary>
    private List<Weldable> GetChildWeldables()
    {
        var result = new List<Weldable>();
        var stack = new Stack<Transform>();

        foreach (Transform child in transform)
        {
            stack.Push(child);
        }

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            Weldable weldable = current.GetComponent<Weldable>();

            if (weldable)
            {
                result.Add(weldable);
            }
            else
            {
                foreach (Transform child in current)
                {
                    stack.Push(child);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all directly connected weldables.
    /// </summary>
    internal IReadOnlyCollection<Weldable> GetDirectConnections() => connections;

    /// <summary>
    /// Gets all connected weldables recursively (excluding self).
    /// </summary>
    internal HashSet<Weldable> GetAllConnectedRecursive()
    {
        var result = new HashSet<Weldable>();
        var stack = new Stack<Weldable>();
        stack.Push(this);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var conn in current.connections)
            {
                if (conn != this && result.Add(conn))
                {
                    stack.Push(conn);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Returns whether this Weldable is connected to another.
    /// </summary>
    internal bool IsConnected(Weldable other)
    {
        return connections.Contains(other);
    }

    /// <summary>
    /// Adds a connection to another Weldable.
    /// </summary>
    private void AddConnection(Weldable other)
    {
        if (other != null && other != this)
        {
            connections.Add(other);
        }
    }

    /// <summary>
    /// Attempts to set the weld type for this object.
    /// </summary>
    private bool TrySetWeldType(WeldType newType)
    {
        if (currentWeldType == WeldType.Undefined)
        {
            currentWeldType = newType;
            return true;
        }

        return currentWeldType == newType;
    }

    /// <summary>
    /// Retrieves all IWeldListener components in this object and its descendants,
    /// skipping children that have their own Weldable component.
    /// </summary>
    private IEnumerable<IWeldListener> GetDescendantWeldListeners()
    {
        foreach (var listener in GetComponents<IWeldListener>())
            yield return listener;

        var stack = new Stack<Transform>();
        stack.Push(transform);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            foreach (Transform child in current)
            {
                if (child.GetComponent<Weldable>() != null)
                    continue;

                foreach (var listener in child.GetComponents<IWeldListener>())
                    yield return listener;

                stack.Push(child);
            }
        }
    }

    /// <summary>
    /// Notifies listeners that this object has been welded.
    /// </summary>
    private void NotifyOnWeld(bool joinedWeldGroup)
    {
        foreach (var listener in GetDescendantWeldListeners())
        {
            if (joinedWeldGroup) listener.OnWeld();
            listener.OnAdded();
        }
    }

    /// <summary>
    /// Notifies listeners that this object has been unwelded.
    /// </summary>
    private void NotifyOnUnweld(bool leavedWeldGroup)
    {
        foreach (var listener in GetDescendantWeldListeners())
        {
            listener.OnRemoved();
            if (leavedWeldGroup) listener.OnUnweld();
        }
    }
}
