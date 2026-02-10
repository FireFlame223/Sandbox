using System.Collections.Generic;
using UnityEngine;

public class CustomFixedJoint : MonoBehaviour
{
    [Tooltip("The Transform this object is attached to.")]
    public Transform targetTransform; 

    // Initial local offset between this object and the target
    private Vector3 initialLocalPosition; 
    private Quaternion initialLocalRotation;

    private FixedJoint fixedJoint;

    public WeldType weldType = WeldType.Undefined;

    public enum WeldType
    {
        Undefined,
        HierarchyBased,
        PhysicsBased
    }

    void Start()
    {
        if (targetTransform == null)
        {
            Debug.LogError("CustomFixedJoint: Target Transform is not assigned on " + gameObject.name + "! Disabling the script.");
            enabled = false;
            return;
        }

        // Calculate initial offset relative to target
        initialLocalPosition = transform.InverseTransformPoint(targetTransform.position);
        initialLocalRotation = Quaternion.Inverse(transform.rotation) * targetTransform.rotation;

        // If not explicitly set to hierarchy-based, try to create a physics-based FixedJoint
        if (weldType != WeldType.HierarchyBased)
        {
            Rigidbody targetRigidbody = targetTransform.GetComponent<Rigidbody>();
            if (targetRigidbody)
            {
                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.connectedBody = targetRigidbody;
                weldType = WeldType.PhysicsBased;
            }
            else
            {
                weldType = WeldType.HierarchyBased;
            }
        }
    }

    void OnDestroy()
    {
        if (fixedJoint != null)
        {
            Destroy(fixedJoint);
        }        
    }

    /// <summary>
    /// Traverses all CustomFixedJoints starting from a given Transform and updates their target transforms.
    /// Call this from LateUpdate or another central place to keep all joints in sync.
    /// </summary>
    public static void UpdateJoint(Transform start)
    {
        Stack<Transform> stack = new();
        stack.Push(start);

        HashSet<Transform> visited = new();

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            foreach (CustomFixedJoint cj in current.GetComponents<CustomFixedJoint>())
            {
                if (!visited.Contains(cj.targetTransform))
                {
                    cj.UpdateSingleTransform();
                    stack.Push(cj.targetTransform);
                }
            }
        }

        Physics.SyncTransforms();
    }

    /// <summary>
    /// Updates the targetTransform to follow this transform based on the initial local offset.
    /// Used when operating in hierarchy-based mode.
    /// </summary>
    public void UpdateSingleTransform()
    {
        if (targetTransform == null) return;

        Vector3 desiredPosition = transform.TransformPoint(initialLocalPosition);
        Quaternion desiredRotation = transform.rotation * initialLocalRotation;

        targetTransform.position = desiredPosition;
        targetTransform.rotation = desiredRotation;

        Rigidbody rb = targetTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = desiredPosition;
            rb.rotation = desiredRotation;
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
