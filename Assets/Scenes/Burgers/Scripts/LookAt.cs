using UnityEngine;

// Prevents multiple instances of this component on a single GameObject
[DisallowMultipleComponent]
public class LookAt : MonoBehaviour
{
    // The transform this object should look at
    public Transform target;

    // Property wrapper around the target field
    [SerializeField]
    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }

    void Update()
    {
        // If no target is assigned, do nothing
        if (target == null) return;

        // Calculate the direction from this object to the target
        Vector3 direction = target.position - transform.position;

        // Ignore vertical difference; we only want to rotate horizontally
        direction.y = 0f;

        // If the direction is not (almost) zero
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            // Create a rotation that looks in the direction of the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Extract the Y-axis rotation (yaw) from the quaternion
            Vector3 euler = targetRotation.eulerAngles;

            // Apply only the Y rotation to this object, keeping X and Z unchanged
            transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        }
    }
}
