using Unity.Cinemachine;
using UnityEngine;

public class FirstPersonRotate : MonoBehaviour
{
    public InputReader input;

    float panAxisDelta = 0f;
    float tiltAxisDelta = 0f;
    private void OnEnable()
    {
        input.lookEvent += OnLook;

    }

    private void OnDisable()
    {
        input.lookEvent -= OnLook;
    }

    void OnLook(Vector2 delta)
    {
        Debug.Log("Value: " + delta.ToString());
        panAxisDelta = delta.x;
        tiltAxisDelta = delta.y;
    }


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(tiltAxisDelta, panAxisDelta, 0f));
    }
}
