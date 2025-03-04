using UnityEngine;

public class CircleSync : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");
    public static int tintID = Shader.PropertyToID("_tint");

    public Camera cam;
    public Material[] WallMaterials;
    public LayerMask Mask;
    public float lerpSpeed = 2f;

    float duration = 0f;

    enum MaterialTag { Platforms = 0, Walls = 1 }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 dir = cam.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);

        RaycastHit info;
        bool hit = Physics.Raycast(ray, out info, dir.magnitude, Mask);
        Material mat = null;



        if (hit)
        {
            if (info.collider.CompareTag("Platforms"))
            {
                mat = WallMaterials[(int)MaterialTag.Platforms];
            }
            else if (info.collider.CompareTag("Walls"))
            {
                mat = WallMaterials[(int)MaterialTag.Walls];
            }
        }

        if (mat != null)
        { 
            Debug.Log("!!!!!");
            float size = Mathf.Lerp(0f, 0.5f, duration);
            mat.SetFloat(SizeID, size);
            duration += Time.deltaTime * lerpSpeed;
            Vector3 view = cam.WorldToViewportPoint(transform.position);
            mat.SetVector(PosID, view);
        }
        else
        {
            foreach (Material m in WallMaterials)
            {
                float size = Mathf.Lerp(0f, 0.5f, duration);
                m.SetFloat(SizeID, size);
                duration -= Time.deltaTime * lerpSpeed;
            }
        }

        duration = Mathf.Clamp(duration, 0f, 1f);

#if false
        foreach (Material WallMaterial in WallMaterials)
        {
            if (hit)
            {
                Debug.Log("!!!!!");
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
#endif
    }

}
