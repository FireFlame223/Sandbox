using UnityEngine;

public class ScaleController : MonoBehaviour
{
    private float scale = 1.0f;
    private float lastScale;

    void Start()
    {
        lastScale = scale;
    }

    public void SetScale(float scale)
    {
        if (scale != lastScale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            lastScale = scale;
        }
    }
}