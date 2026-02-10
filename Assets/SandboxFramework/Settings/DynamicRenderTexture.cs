using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class DynamicRenderTexture : MonoBehaviour
{
    private Camera cam;
    private RenderTexture renderTex;

    public Material blendMaterial;

    private int currentWidth = 0;
    private int currentHeight = 0;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = true;
        UpdateRenderTexture();
    }

    void Update()
    {
        if (Screen.width != currentWidth || Screen.height != currentHeight)
        {
            UpdateRenderTexture();
        }
    }

    void UpdateRenderTexture()
    {
        if (renderTex != null)
        {
            renderTex.Release();
            Destroy(renderTex);
        }

        currentWidth = Screen.width;
        currentHeight = Screen.height;

        renderTex = new RenderTexture(currentWidth, currentHeight, 16);
        renderTex.name = "DynamicRT_" + currentWidth + "x" + currentHeight;

        cam.targetTexture = renderTex;

        blendMaterial.SetTexture("_RenderTexture", renderTex);
    }

    void OnDestroy()
    {
        blendMaterial.SetTexture("_RenderTexture", null);
        if (renderTex != null)
        {
            renderTex.Release();
            Destroy(renderTex);
        }
    }
}
