using UnityEngine;

public class CheckOutOfBounds : MonoBehaviour
{
    public Transform target;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.ToString());
        if (other.gameObject.CompareTag(target.tag))
        {
            Debug.Log("OUT OF BOUNDS");
            target.GetComponent<PlayerMovement>().ResetState();
        }
    }
}
