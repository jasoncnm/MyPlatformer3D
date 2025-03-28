using Unity.Cinemachine;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public Transform cam;
    public Transform player;
    public Transform reciever;
    
    
    bool _Overlapping = false;
    Vector3 forward;

    private void Start()
    {
        forward = transform.parent.forward;
    }


    private void Update()
    {
        float rotationDiff = -Quaternion.Angle(transform.rotation, reciever.rotation);

        if (_Overlapping)
        {
            Vector3 positionOffset = player.transform.position - transform.position;
            float dot = Vector3.Dot(forward, positionOffset);
            
            if (dot < 0f)
            {
                rotationDiff += 180f;
                // TODO: Refactor these code
                // Telport!!
                CinemachinePanTilt panTilt = cam.GetComponent<CinemachinePanTilt>();
                panTilt.PanAxis.Value += rotationDiff;
                panTilt.PanAxis.Value %= 360f;

                // Change Velocity (Need to Be refactored)
                Rigidbody rb = player.GetComponent<Rigidbody>();
                Vector3 velocity = rb.linearVelocity;
                Vector3 Dir = Quaternion.Euler(0.0f, rotationDiff, 0.0f) * velocity.normalized;
                rb.linearVelocity = Dir * velocity.magnitude;


                positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * positionOffset;
                player.position = reciever.position + positionOffset;

                _Overlapping = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Make sure does not enter in the back
            Vector3 positionOffset = player.transform.position - transform.position;
            float dot = Vector3.Dot(forward, positionOffset);
            
            _Overlapping = dot > 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _Overlapping = false;
        }
    }
}
