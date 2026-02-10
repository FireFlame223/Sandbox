using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AddOutlineCamera : MonoBehaviour
{
    public GameObject outlineCameraPrefab;

    void Start()
    {
        if (outlineCameraPrefab == null)
        {
            Debug.LogError("Outline camera prefab not set");
            return;
        }

        Camera camera = FindAnyObjectByType<Camera>();

        if (camera == null) 
        {
            Debug.LogError("No camera found");
            return;
        }


        UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
        if (cameraData != null)
        {
            cameraData.renderPostProcessing = true;
            cameraData.SetRenderer(1); //1=PC_Renderer
        }

        Instantiate(outlineCameraPrefab, camera.transform);
    }

}
