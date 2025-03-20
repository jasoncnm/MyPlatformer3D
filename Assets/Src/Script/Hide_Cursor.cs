using UnityEngine;
using UnityEngine.UI;

public class Hide_Cursor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void SetHide(Toggle toggle)
    {
        Cursor.visible = !toggle.isOn;
    }

}
