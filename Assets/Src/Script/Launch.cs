using UnityEngine;

public class Launch : MonoBehaviour
{
    Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
    enum LaunchDirection { Up, Down, Left, Right};

    
    [SerializeField] float launchSpeed;
    [SerializeField] LaunchDirection dirIndex;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Triggered Player");
            Rigidbody rb = other.transform.GetComponent<Rigidbody>();
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(directions[(int)dirIndex] * launchSpeed, ForceMode.Impulse);
        }
    }

}
