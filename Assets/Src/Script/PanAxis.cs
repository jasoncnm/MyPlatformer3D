using Unity.Cinemachine;
using UnityEditor.PackageManager;
using UnityEngine;

public class PanAxis : MonoBehaviour
{

    public float pan, tilt;
    public CinemachineCamera fpscam;

    CinemachinePanTilt panTilt;

    private void Start()
    {
        panTilt = fpscam.GetComponent<CinemachinePanTilt>();
    }
    // Update is called once per frame
    void Update()
    {
        if (panTilt != null)
        {
            Debug.Log("PANNNN");

        } 
    }



}
