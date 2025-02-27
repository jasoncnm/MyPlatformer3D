using UnityEngine;

public class CircleSync : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Camera cam;
    public Material[] WallMaterials;
    public LayerMask Mask;
    public float lerpSpeed = 2f;

    float duration = 0f;

    // Update is called once per frame
    void Update()
    {
        
        Vector3 dir = cam.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);
        foreach (Material WallMaterial in WallMaterials)
        {
            if (Physics.Raycast(ray, 3000, Mask))
            {
                Debug.Log("!!!!!!!!!!");
                float size = Mathf.Lerp(0f, 0.5f, duration);
                WallMaterial.SetFloat(SizeID, size);
                duration += Time.deltaTime * lerpSpeed;
            }
            else
            {

                float size = Mathf.Lerp(0f, 0.5f, duration);
                WallMaterial.SetFloat(SizeID, size);
                duration -= Time.deltaTime * lerpSpeed;

            }
            duration = Mathf.Clamp(duration, 0f, 1f);
            Vector3 view = cam.WorldToViewportPoint(transform.position);
            WallMaterial.SetVector(PosID, view);
        }
    }
}
