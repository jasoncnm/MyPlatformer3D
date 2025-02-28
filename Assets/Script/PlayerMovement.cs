using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField]
    float normalSpeed, acceleration, jump, frictionAmount;
    [SerializeField]
    Transform Cam;

    [SerializeField, Range(0f, .1f)]
    float turnSmoothTime;


    [SerializeField]
    float decceleration;

    [SerializeField, Range(0f, 1f)] float jumpBufferTime = 0.2f;

    [SerializeField]
    bool ToggleSlide = true;
    
    float speed;
    float sprintSpeed;
    float targetAngle = 0.0f;
    float velPower;
    float hMovement = 0.0f;
    float vMovement = 0.0f;
    float turnSmoothVelocity;
    float jumpBufferCounter;

    bool IsGound, JumpButtonDown,  JumpButtonUp, reset;

    Vector3 direction;

    Vector3 originalposition = new Vector3(0f, 1.55999994f, 0f);
    Quaternion originalrotation = Quaternion.identity;
    Vector3 originalscale = Vector3.one;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        reset = false;
        IsGound = true;
        speed = normalSpeed;
        rb = gameObject.GetComponent<Rigidbody>();
        sprintSpeed = 2 * normalSpeed;
        velPower = 0.7f;
        turnSmoothTime = 0.0473f;
    }

    private void Start()
    {
        transform.position = originalposition;
        transform.rotation = originalrotation;
        transform.localScale = originalscale;
    }

    // Update is called once per frame
    void Update()
    {
        DebugPause();
        InputAction SprintAction = InputSystem.actions.FindAction("Sprint");
        InputAction moveAction = InputSystem.actions.FindAction("Move");

        if (Input.GetKeyDown(KeyCode.R)) reset = true;

        if (reset)
        {
            ResetState();
        }

        Vector2 MoveValue = moveAction.ReadValue<Vector2>();

        hMovement = MoveValue.x;
        vMovement = MoveValue.y;
        // Debug.Log(moveAction.ToString());
        // hMovement = Input.GetAxisRaw("Horizontal");
        // vMovement = Input.GetAxisRaw("Vertical");
        // desiredJump |= Input.GetButtonDown("Jump");
        // Input.GetButtonUp("Jump");

        JumpButtonDown = Input.GetButtonDown("Jump");
        JumpButtonUp = Input.GetButtonUp("Jump");
        direction = new Vector3(hMovement, 0.0f, vMovement).normalized;

           

        if (JumpButtonDown)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (direction.magnitude >= 0.1f)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }

        if (SprintAction.IsPressed())
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = normalSpeed;
        }
        Jump();
    }

    private void FixedUpdate()
    {
        if (!reset)
        {
            if (direction.magnitude >= 0.1f)
            {
                Vector3 Dir = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;
                // ector3 Dir = new Vector3(hMovement, 0.0f, vMovement);

                Vector3 targetVelocity = Dir.normalized * speed;

                Vector3 VelocityDiff = targetVelocity - rb.linearVelocity;

                float accelRate = acceleration;

                Vector3 movement = new Vector3(Mathf.Pow(Mathf.Abs(VelocityDiff.x) * accelRate, velPower) * Mathf.Sign(VelocityDiff.x), 0.0f,
                    Mathf.Pow(Mathf.Abs(VelocityDiff.z) * accelRate, velPower) * Mathf.Sign(VelocityDiff.z));

                rb.AddForce(movement);
            }
            else if (IsGound)
            {
                if (ToggleSlide)
                    rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, decceleration * Time.fixedDeltaTime);
                else
                    rb.linearVelocity = Vector3.zero;

            }




            /*

            if (desiredJump)    
            {
                desiredJump = false;
                Jump();
            }


            IsGound = false;
            */
        }

    }

    void Jump()
    {
        if (IsGound && jumpBufferCounter > 0f)
        {
            jumpBufferCounter = 0f;
            // Debug.Log("JUMP NOW!");
            IsGound = false;
            rb.linearVelocity += Vector3.up * jump;
        } 
        else if (JumpButtonUp && rb.linearVelocity.y > 0.0001f)
        {
            JumpButtonUp = false;
            rb.linearVelocity = Vector3.Scale(rb.linearVelocity, new Vector3(1f, 0.5f, 1f));
            // rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
        }

    }

    void EvaluateCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            Debug.Log("ContactName: " + collision.gameObject.name + " Normal: " + normal.ToString());
            if (normal.y > 0f)
            {
                IsGound = true;
                break;
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {

        EvaluateCollisionEnter(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        //TODO: Need check is it is ground
        // IsGound = false;
        // EvaluateCollisionExit(collision);
    }

    void DebugPause()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int i = 0;
        }
    }

    public void ResetState()
    {
        reset = false;
        transform.position = originalposition;
        transform.rotation = originalrotation;
        transform.localScale = originalscale;
        rb.linearVelocity = Vector3.zero;
    }
}
