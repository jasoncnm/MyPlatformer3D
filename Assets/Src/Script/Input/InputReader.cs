using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Input Reader", fileName = "Input Reader")]
public class InputReader : ScriptableObject
{
    
    [SerializeField] public InputActionAsset asset;

    public event UnityAction<Vector2> moveEvent;
    public event UnityAction<Vector2> lookEvent;

    public event UnityAction jumpEvent;
    public event UnityAction jumpCancelledEvent;

    public event UnityAction sprintEvent;
    public event UnityAction sprintCancelledEvent;

    public event UnityAction pauseEvent;
    public event UnityAction unPauseEvent;

    InputAction lookAction;
    InputAction moveAction;
    InputAction sprintAction;
    InputAction jumpAction;
    InputAction pauseAction;
    InputAction unPauseAction;

    private void OnEnable()
    {
        lookAction = asset.FindAction("Look", true);
        moveAction = asset.FindAction("Move", true);
        sprintAction = asset.FindAction("Sprint", true);
        jumpAction = asset.FindAction("Jump", true);
        pauseAction = asset.FindAction("Pause", true);
        unPauseAction = asset.FindAction("UnPause", true);

        lookAction.started += OnLook;
        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;

        moveAction.started   += OnMove;
        moveAction.performed += OnMove;
        moveAction.canceled  += OnMove;

        sprintAction.started   += OnSprint;
        sprintAction.performed += OnSprint;
        sprintAction.canceled  += OnSprint;

        jumpAction.started   += OnJump;
        jumpAction.performed += OnJump;
        jumpAction.canceled  += OnJump;

        pauseAction.started   += OnPause;
        pauseAction.performed += OnPause;
        pauseAction.canceled  += OnPause;

        unPauseAction.started += OnUnPause;
        unPauseAction.performed += OnUnPause;
        unPauseAction.canceled += OnUnPause;

        lookAction.Enable();
        moveAction.Enable();
        sprintAction.Enable();
        jumpAction.Enable();
        pauseAction.Enable();
        unPauseAction.Enable();

    }

    private void OnDisable()
    {
        lookAction.started -= OnLook;
        lookAction.performed -= OnLook;
        lookAction.canceled -= OnLook;


        moveAction.started -= OnMove;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        sprintAction.started -= OnSprint;
        sprintAction.performed -= OnSprint;
        sprintAction.canceled -= OnSprint;

        jumpAction.started -= OnJump;
        jumpAction.performed -= OnJump;
        jumpAction.canceled -= OnJump;

        pauseAction.started -= OnPause;
        pauseAction.performed -= OnPause;
        pauseAction.canceled -= OnPause;

        unPauseAction.started -= OnUnPause;
        unPauseAction.performed -= OnUnPause;
        unPauseAction.canceled -= OnUnPause;

        lookAction.Disable();
        moveAction.Disable();
        sprintAction.Disable();
        jumpAction.Disable();
        pauseAction.Disable();
        unPauseAction.Disable();
    }

    void OnUnPause(InputAction.CallbackContext context)
    {
        if (unPauseEvent != null && context.started)
        {
            unPauseEvent.Invoke();
        }
    }

    void OnPause(InputAction.CallbackContext context)
    {
        if (pauseEvent != null && context.started)
        {
            pauseEvent.Invoke();
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveEvent?.Invoke(context.ReadValue<Vector2>());

    }

    void OnLook(InputAction.CallbackContext  context)
    {
        lookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    void OnSprint(InputAction.CallbackContext context)
    {
        if (sprintEvent != null && context.started)
        {
            sprintEvent.Invoke();
        }

        if (sprintCancelledEvent != null && context.canceled)
        {
            sprintCancelledEvent.Invoke();
        }
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (jumpEvent != null && context.started)
        {
            jumpEvent.Invoke();
        }

        if (jumpCancelledEvent != null && context.canceled)
        {
            jumpCancelledEvent.Invoke();
        }
    }

}
