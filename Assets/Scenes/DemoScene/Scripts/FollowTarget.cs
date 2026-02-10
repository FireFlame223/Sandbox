using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           // The GameObject to follow
    public float followSpeed = 5f;     // How fast the object moves
    public float stopDistance = 2f;    // Minimum distance to the target

    [Header("Follow Toggle")]
    public bool isFollowing = true;   // Enabled by default

    void Update()
    {
        if (!isFollowing || target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        // Direction to the target
        Vector3 direction = (target.position - transform.position).normalized;

        if (distance > stopDistance)
        {
            // --- Move forward ---
            transform.position += transform.forward * followSpeed * Time.deltaTime;
        }

        // --- Add rotation ---
        transform.rotation = Quaternion.LookRotation(direction);
    }

    // Optional: Public method to enable/disable following
    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
    }
}
