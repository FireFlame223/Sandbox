using StarterAssets;
using UnityEngine;

[DisallowMultipleComponent]
public class KeepUpright : MonoBehaviour
{
    void LateUpdate()
    {
        if (transform.parent == null)
        {
            transform.rotation = Quaternion.identity;
            float rotationY = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0, rotationY, 0);
        }
    }
}
