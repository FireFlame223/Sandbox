using System.Collections.Generic;
using UnityEngine;

namespace UnityFundamentals
{

    // The BetterRigidBodyPush Script provides a frame rate independent version of
    // the standard Unity BasicRigidBodyPush Script.
    //
    // The issue is caused by OnControllerColliderHit being called for every Update 
    // that the character hits something, in other words the amount of times it is being
    // called is frame rate DEPENDENT. If you apply forces during that method, 
    // faster framerate incur more forces being applied.
    //
    // To fix the issue, we record objects that need to be push, only only DO the pushing
    // during FixedUpdate. This is an alternative Update method, called at a Fixed rate
    // (by default at 50 fps always).
    //
    // @author J.C. Wichman

    public class BetterRigidBodyPush : MonoBehaviour
    {
        public string pushTag;
        public bool canPush;
        [Range(0.5f, 5f)] public float strength = 1.1f;

        private Dictionary<Rigidbody, Vector3> rigidBodiesToPush = new Dictionary<Rigidbody, Vector3>();

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (canPush)
            {
                // We dont want to push objects below us
                if (hit.moveDirection.y < -0.3f) return;

                // make sure we hit a non kinematic rigidbody
                Rigidbody body = hit.collider.attachedRigidbody;
                if (body == null || body.isKinematic) return;
                if (rigidBodiesToPush.ContainsKey(body)) return;

                // make sure we only push desired layer(s)
                if (!string.IsNullOrEmpty(pushTag) && !body.gameObject.CompareTag(pushTag)) return;

                // Calculate push direction from move direction, horizontal motion only
                Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

                rigidBodiesToPush[body] = pushDirection;
            }
        }

        private void FixedUpdate()
        {
            foreach (var entry in rigidBodiesToPush)
            {
                PushRigidBody(entry.Key, entry.Value);
            }
            rigidBodiesToPush.Clear();
        }

        private void PushRigidBody(Rigidbody rb, Vector3 pushDir)
        {
            // Apply the push and take strength into account
            rb.AddForce(pushDir * strength, ForceMode.Impulse);
        }
    }
}
