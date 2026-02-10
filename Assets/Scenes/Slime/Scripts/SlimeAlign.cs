using UnityEngine;

public class SlimeAlign : MonoBehaviour
{
    // The parent object (the agent) that controls the forward direction
    private Transform agentTransform;

    public float rayLength = 2f;
    public LayerMask groundMask;
    public float rotationSpeed = 10f;

    void Awake()
    {
        // Get the parent's Transform to determine the forward direction
        agentTransform = transform.parent;
    }

    void Update()
    {
        // Start the raycast from the slime's position, with a small upward offset
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // Shoot a raycast straight down to detect the surface
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundMask))
        {
            // The desired "up" direction is the normal of the hit surface
            Vector3 newUp = hit.normal;

            // The desired "forward" direction is the parent's forward direction.
            // We project it onto the plane of the ground to keep it aligned with the slope.
            Vector3 newForward = Vector3.ProjectOnPlane(agentTransform.forward, newUp).normalized;

            // If the projected forward direction is very small, use a default forward vector to avoid issues
            if (newForward.sqrMagnitude < 0.01f)
            {
                newForward = transform.forward;
            }

            // Calculate the final target rotation using the new up and forward vectors
            Quaternion targetRotation = Quaternion.LookRotation(newForward, newUp);

            // Smoothly apply the rotation using Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}