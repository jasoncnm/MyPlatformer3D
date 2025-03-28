using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public InputReader input;

    [SerializeField] InputActionAsset asset;

    [SerializeField] GameObject pauseMenu;

    [SerializeField] GameObject pauseMenuFirst;

        
    bool _PauseType = false;
    bool _UnPauseType = false;
    bool _Pause = false;
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        pauseMenu.SetActive(false);
        input.pauseEvent += OnPause;
        input.unPauseEvent += OnUnPause;

    }

    private void OnDisable()
    {
        input.pauseEvent -= OnPause;
        input.pauseEvent -= OnUnPause;
    }

    void OnPause()
    {
        _PauseType = true;
    }

    void OnUnPause()
    {
        _UnPauseType = true;
    }

    private void Update()
    {
        if (_PauseType)
        {
            if (!_Pause)
            {
                PauseGame();
            }
        }
        else if (_UnPauseType)
        {
            if (_Pause)
            {
                ResumeGame();
            }
        }
        _PauseType = false;
        _UnPauseType = false;
    }
    public void ResumeGame()
    {
        _Pause = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
        asset.FindActionMap("Player", true).Enable();
        asset.FindActionMap("UI", true).Disable();

        // playerInput.SwitchCurrentActionMap("Player");
    }

    public void PauseGame()
    {
        _Pause = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        EventSystem.current.SetSelectedGameObject(pauseMenuFirst);
        asset.FindActionMap("Player", true).Disable();
        asset.FindActionMap("UI", true).Enable();
        //playerInput.SwitchCurrentActionMap("UI");
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    public void SetHide(Toggle toggle)
    {
        Cursor.visible = !toggle.isOn;
    }


}
