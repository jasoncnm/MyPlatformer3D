using Unity.Cinemachine;
using UnityEngine;

public class TestPlayerRot : MonoBehaviour
{
    public float delta;
    public CinemachinePanTilt cameraPanTilt;

    private void Update()
    {

        //transform.Rotate(Vector3.up, delta * Time.deltaTime);
        cameraPanTilt.PanAxis.Value += delta * Time.deltaTime;
        cameraPanTilt.PanAxis.Value %= 360f;
        Debug.Log("Player Rotation: " + transform.rotation.eulerAngles.ToString() + " Camera pan tilt: (" + cameraPanTilt.PanAxis.Value + ", " + cameraPanTilt.TiltAxis.Value + ")");

    }
}
