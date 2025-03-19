using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Input Reader", fileName = "Input Reader")]
public class InputReader : ScriptableObject
{
    [SerializeField] public InputActionAsset asset;

    public event UnityAction<Vector2> moveEvent;
    public event UnityAction jumpEvent;
    public event UnityAction jumpCancelledEvent;

    public event UnityAction sprintEvent;
    public event UnityAction sprintCancelledEvent;

    InputAction moveAction;
    InputAction sprintAction;
    InputAction jumpAction;

    private void OnEnable()
    {
        moveAction = asset.FindAction("Move", true);
        sprintAction = asset.FindAction("Sprint", true);
        jumpAction = asset.FindAction("Jump", true);

        moveAction.started   += OnMove;
        moveAction.performed += OnMove;
        moveAction.canceled  += OnMove;

        sprintAction.started   += OnSprint;
        sprintAction.performed += OnSprint;
        sprintAction.canceled  += OnSprint;

        jumpAction.started   += OnJump;
        jumpAction.performed += OnJump;
        jumpAction.canceled  += OnJump;

        moveAction.Enable();
        sprintAction.Enable();
        jumpAction.Enable();

    }

    private void OnDisable()
    {
        moveAction.started -= OnMove;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        sprintAction.started -= OnSprint;
        sprintAction.performed -= OnSprint;
        sprintAction.canceled -= OnSprint;

        jumpAction.started -= OnJump;
        jumpAction.performed -= OnJump;
        jumpAction.canceled -= OnJump;
        

        moveAction.Disable();
        sprintAction.Disable();
        jumpAction.Disable();
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveEvent?.Invoke(context.ReadValue<Vector2>());
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
