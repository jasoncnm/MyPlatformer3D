using UnityEngine;

public class Launch : MonoBehaviour
{
    Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    enum LaunchDirection { Up, Down, Left, Right, front, back};

    
    [SerializeField] float launchSpeed;
    [SerializeField] LaunchDirection dirIndex;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Triggered Player");
            Rigidbody rb = other.transform.GetComponent<Rigidbody>();
            Vector3 v = Vector3.Scale(rb.linearVelocity, directions[(int)dirIndex]);
            rb.linearVelocity -= v;
            rb.AddForce(directions[(int)dirIndex] * launchSpeed, ForceMode.Impulse);
        }
    }

}
