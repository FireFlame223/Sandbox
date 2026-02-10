using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum RigidbodyStateChange
{
    Unchanged,
    SetKinematic,
    SetNonKinematic
}

[RequireComponent(typeof(Selectable))]
[DisallowMultipleComponent]
public class Draggable : MonoBehaviour
{
    public bool shouldPropagateDragEvents = true;
    public bool shouldIgnoreRigidbodySettingFromDragger = false;
    public bool removeFromParentAtAwake = false;
    private bool isBeingDragged = false;
    private Rigidbody rigidBody;
    private Vector3 throwVelocity = Vector3.zero;
    void Awake()
    {
        if (removeFromParentAtAwake)
        {
            transform.SetParent(null, true);
        }
    }

    /// <summary>
    /// Called when dragging starts. Optionally modifies Rigidbody settings.
    /// </summary>
    public void StartDrag(RigidbodyStateChange stateChange)
    {
        if (!enabled) return;

        throwVelocity = Vector3.zero;

        rigidBody = GetComponent<Rigidbody>();
        OnGrab();
        isBeingDragged = true;

        ApplyRigidbodyStateChange(stateChange);
    }

    /// <summary>
    /// Changes the Rigidbody's kinematic state, if required, for all connected rigidbodies.
    /// </summary>
    private void ApplyRigidbodyStateChange(RigidbodyStateChange stateChange)
    {
        if (shouldIgnoreRigidbodySettingFromDragger || stateChange == RigidbodyStateChange.Unchanged)
            return;

        Weldable weldable = GetComponent<Weldable>();

        IReadOnlyList<Rigidbody> rigidbodies;
        if (weldable && weldable.weldType == WeldType.HierarchyBased)
        {
            // Find all rigidbodies in the connected hierarchy/weld
            rigidbodies = Utils.FindAllInHierarchyAndConnections<Rigidbody>(weldable);
        }
        else
        {
            rigidbodies = new List<Rigidbody> { rigidBody }.AsReadOnly();
        }

        foreach (var rb in rigidbodies)
            {
                if (rb == null) continue;
                rb.isKinematic = (stateChange == RigidbodyStateChange.SetKinematic);
            }
    }    
    
    /// <summary>
    /// Updates the object's position and rotation during dragging.
    /// </summary>
    public void UpdateDrag(Vector3 position, Quaternion rotation)
    {
        if (!enabled || !isBeingDragged) return;

        if (rigidBody != null)
        {
            throwVelocity = throwVelocity * 0.9f + ((position - rigidBody.position) / Time.deltaTime) * 0.1f;
        }

        ApplyTransformation(position, rotation);
        CustomFixedJoint.UpdateJoint(transform);
    }

    /// <summary>
    /// Applies the transformation using Rigidbody or Transform.
    /// </summary>
    private void ApplyTransformation(Vector3 position, Quaternion rotation)
    {
        if (rigidBody != null)
        {
            MoveRigidbody(position, rotation);
        }
        else
        {
            MoveTransform(position, rotation);
        }
    }

    /// <summary>
    /// Moves the object using Transform. Handles parented objects correctly.
    /// </summary>
    private void MoveTransform(Vector3 position, Quaternion rotation)
    {
        if (transform.parent == null)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        else
        {
            Transform root = transform.root;

            Matrix4x4 currentLocalMatrix = root.worldToLocalMatrix * transform.localToWorldMatrix;
            Matrix4x4 desiredWorldMatrix = Matrix4x4.TRS(position, rotation, transform.lossyScale);
            Matrix4x4 newRootWorldMatrix = desiredWorldMatrix * currentLocalMatrix.inverse;

            root.position = newRootWorldMatrix.GetColumn(3);
            root.rotation = Quaternion.LookRotation(
                newRootWorldMatrix.GetColumn(2),
                newRootWorldMatrix.GetColumn(1)
            );
        }
    }

    /// <summary>
    /// Moves the object using Rigidbody methods.
    /// </summary>
    private void MoveRigidbody(Vector3 position, Quaternion rotation)
    {
        rigidBody.MoveRotation(rotation);
        rigidBody.MovePosition(position);
    }

    /// <summary>
    /// Ends the dragging operation and restores Rigidbody settings if needed.
    /// </summary>
    public void EndDrag(RigidbodyStateChange stateChange, float throwMultiplier, float maxThrowVelocity)
    {
        if (!enabled) return;

        OnRelease();
        ApplyRigidbodyStateChange(stateChange);
        isBeingDragged = false;

        if (rigidBody != null && !rigidBody.isKinematic)
        {
            Vector3 newThrowVelocity = throwVelocity * throwMultiplier;
            if (newThrowVelocity.magnitude > maxThrowVelocity)
            {
                newThrowVelocity = newThrowVelocity.normalized * maxThrowVelocity;
            }
            rigidBody.linearVelocity = newThrowVelocity;
        }
    }

    /// <summary>
    /// Finds all drag listeners affected by this object.
    /// </summary>
    private IDragListener[] GetConnectedDragListeners()
    {
        if (shouldPropagateDragEvents)
        {
            Weldable weldable = GetComponentInParent<Weldable>();
            if (weldable != null)
            {
                return Utils.FindAllInHierarchyAndConnections<IDragListener>(weldable).ToArray();
            }

            Transform root = transform.root;
            return root.GetComponentsInChildren<IDragListener>();
        }
        else
        {
            var result = new List<IDragListener>();
            var stack = new Stack<Transform>();
            stack.Push(transform);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current != transform && current.GetComponent<Weldable>() != null)
                    continue;

                foreach (var listener in current.GetComponents<IDragListener>())
                {
                    if (listener != null)
                        result.Add(listener);
                }

                foreach (Transform child in current)
                {
                    stack.Push(child);
                }
            }

            return result.ToArray();
        }
    }

    /// <summary>
    /// Notifies all listeners that the object was grabbed.
    /// </summary>
    private void OnGrab()
    {
        if (!enabled) return;

        foreach (DragListener dragListener in GetConnectedDragListeners())
        {
            dragListener.OnGrab();
        }
    }

    /// <summary>
    /// Notifies all listeners that the object was released.
    /// </summary>
    private void OnRelease()
    {
        if (!enabled) return;

        foreach (DragListener dragListener in GetConnectedDragListeners())
        {
            dragListener.OnRelease();
        }
    }
}
