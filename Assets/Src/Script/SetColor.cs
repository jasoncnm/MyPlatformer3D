using UnityEngine;

public class SetColor : MonoBehaviour
{
    public Color color;

    public Renderer[] renderers;

    bool start = false;

    private void Start()
    {
        if (start)
        {
            foreach (Renderer renderer in renderers)
                renderer.material.color = color;
        }
    }

}
