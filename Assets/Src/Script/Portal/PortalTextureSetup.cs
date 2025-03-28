using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    public Camera[] portalCams;
    
    public Material[] portalCamMats;

    public static int TextureID = Shader.PropertyToID("_Texture");
    private void Start()
    {
        for (int i = 0; i < portalCams.Length; i++)
        {
            Camera cam = portalCams[i];
            Material mat = portalCamMats[i];

            if (cam.targetTexture != null)
            {
                cam.targetTexture.Release();
            }
            cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            mat.SetTexture(TextureID, cam.targetTexture);

        }
    }
}
