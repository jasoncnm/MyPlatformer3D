using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class PortalCamera : MonoBehaviour
{
    [SerializeField] Transform playerCam;
    [SerializeField] Transform portal;
    [SerializeField] Transform otherPortal;

    Transform tempCopy;

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        otherPortal.Rotate(new Vector3(0f, 180f, 0f));

        Matrix4x4 m = portal.localToWorldMatrix * otherPortal.worldToLocalMatrix * playerCam.localToWorldMatrix;
        transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        otherPortal.Rotate(new Vector3(0f, -180f, 0f));

    }
}
