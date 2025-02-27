using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputRotation : MonoBehaviour
{
    Vector2 turn;
    [SerializeField] float sensitivity = 0.5f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        transform.localRotation = Quaternion.Euler(0, turn.x, 0);
    }

}
