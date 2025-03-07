using System.Collections;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;


    [SerializeField, Range(1f, 100f)]
    float gravityScale = 10f;

    [SerializeField, Range(1f, 2f)]
    float sprintFactor = 1.5f;
    
    [SerializeField]
    float normalSpeed, maxSpeed, acceleration, jump, groundDecceleration, airDecceleration, slopeDecceleration, frictionAmount;
    [SerializeField]
    Transform Cam;

    [SerializeField, Range(0f, .1f)]
    float turnSmoothTime;

    [SerializeField, Range(0f, 1f)] float jumpBufferTime = 0.2f, coyoteTime = 0.2f;
    [SerializeField, Range(0f, 1f)] float releaseBufferTime = 0.2f;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;


    [SerializeField]
    bool ToggleSlide = true;

    [SerializeField]
    LayerMask groundedMask;

    [SerializeField]
    SphereCollider sphereCollider;

    BetterJump fallComponent;

    float speed;
    
    float velPower;
    float turnSmoothVelocity;
    float jumpBufferCounter;
    float minGroundDotProduct;
    float coyoteTimeCounter;
    float slopeDeceleration;

    bool _Grounded = true;
    bool _SlopeStop = false;
    bool _Jump = false;
    bool _JumpReleased = false;
    bool _NoInput = false;
    bool _OnSlope = false;
    

    Vector3 forwardMovement, backwardMovement;
    Vector3 contactNormal;

    Vector3 originalposition = new Vector3(0f, 1.55999994f, 0f);
    Quaternion originalrotation = Quaternion.identity;
    Vector3 originalscale = Vector3.one;
    Vector3 originalGravity = Physics.gravity;
    

    private void OnValidate()
    {
        Physics.gravity = Vector3.down * gravityScale;
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        fallComponent = transform.GetComponent<BetterJump>();
        OnValidate();
        speed = normalSpeed;
        rb = gameObject.GetComponent<Rigidbody>();
        velPower = 0.7f;
        turnSmoothTime = 0.0473f;
        _NoInput = true;
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
        InputAction SprintAction = InputSystem.actions.FindAction("Sprint");
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        Vector2 MoveValue = moveAction.ReadValue<Vector2>();
        bool JumpButtonDown = Input.GetButtonDown("Jump");
        bool JumpButtonUp = Input.GetButtonUp("Jump");

        _NoInput |= !Input.anyKeyDown;

        Vector3 direction = new Vector3(MoveValue.x, 0.0f, MoveValue.y).normalized;

        SimulateMovement(direction.x, direction.z, SprintAction.IsPressed());
        SimulateJump(JumpButtonDown, JumpButtonUp);
    }


    private void FixedUpdate()
    {
        if (_NoInput && _OnSlope)
        {
            
        }
        else
        {
            //fallComponent.SetFall(true);
        }
        Jump();
        move();

        _NoInput = false;
        _Grounded = false;
        _SlopeStop = false;
        _OnSlope = false;
    }

    void SimulateJump(bool buttonDown, bool buttonUp)
    {
        
        if (buttonDown)
        {
            jumpBufferCounter = releaseBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (_Grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if ((jumpBufferCounter > 0f) && (coyoteTimeCounter > 0f))
        {
            _Jump = true;
        }
        else if (buttonUp && rb.linearVelocity.y > 0f)
        {
            _JumpReleased = true;
        }

        if (!_Grounded) contactNormal = Vector3.up;

    }
    void Jump()
    {
        
        if (_Jump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            rb.AddForce(Vector3.up * jump, ForceMode.Impulse);
            _Jump = false;
        }

        if (_JumpReleased)
        {
            _JumpReleased = false;  
            rb.linearVelocity = Vector3.Scale(rb.linearVelocity, new Vector3(1f, 0.5f, 1f));
            // rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
        }

    }

    Vector3 GetPlayerVelocity(float dirX, float dirZ)
    {
        if (Mathf.Abs(dirX) > 0f || Mathf.Abs(dirZ) > 0f)
        {
            float targetAngle = Mathf.Atan2(dirX, dirZ) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            targetAngle = Mathf.Abs(targetAngle) < 0.0001f ? 0 : targetAngle;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            angle = Mathf.Abs(angle) < 0.0001f ? 0 : angle;

            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            Vector3 Dir = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;

            Vector3 targetVelocity = Dir.normalized * speed;

            Vector3 VelocityDiff = targetVelocity - rb.linearVelocity;

            float accelRate = acceleration;

            return new Vector3(Mathf.Pow(Mathf.Abs(VelocityDiff.x) * accelRate, velPower) * Mathf.Sign(VelocityDiff.x), 0.0f,
                Mathf.Pow(Mathf.Abs(VelocityDiff.z) * accelRate, velPower) * Mathf.Sign(VelocityDiff.z));
        }
        else
        {
            return Vector3.zero;
        }
    }

    void SimulateMovement(float dirX, float dirZ, bool sprint)
    {
        if (sprint)
        {
            speed = normalSpeed * sprintFactor;

        }
        else
        {
            speed = normalSpeed;
        }

        // Debug.Log("DIRX: " + dirX.ToString() + " DIRZ: " + dirZ.ToString());
        forwardMovement = Vector3.ProjectOnPlane(GetPlayerVelocity(dirX, dirZ), contactNormal);
    }


    void move()
    {

        rb.AddForce(forwardMovement);


        float decceleration;

        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
        {
            hVel = hVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(hVel.x, rb.linearVelocity.y, hVel.z);
        }
        Debug.Log("Forward Direction = " + forwardDir.ToShortString() + " Reverse Direction = " + reverseDir.ToShortString());
        if (forwardMovement.magnitude < 0.01f && rb.linearVelocity.magnitude > 1f)
        {
            forwardDir = rb.linearVelocity.normalized;
            reverseDir = -forwardDir;

            if (_OnSlope)
            {
                decceleration = slopeDecceleration;
            }
            else if (_Grounded)
            {
                decceleration = groundDecceleration;
            }
            else if (!_SlopeStop)
            {
                reverseDir.y = 0;
                decceleration = airDecceleration;
            }
            else
            {
                decceleration = 0f;
            }

            rb.AddForce(reverseDir * decceleration);

            //rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, new Vector3(0f, rb.linearVelocity.y, 0f),
                //decceleration * Time.fixedDeltaTime);

        }
    }
    public Vector3 forwardDir, reverseDir;
    void DebugPause()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int i = 0;
        }
    }

    public void ResetState()
    {
        transform.position = originalposition;
        transform.rotation = originalrotation;
        transform.localScale = originalscale;
        rb.linearVelocity = Vector3.zero;
    }

    void EvaluateColllision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            // Debug.Log(normal.y);
            if (normal.y >= minGroundDotProduct)
            {
                _Grounded = true;
                contactNormal = normal;
                if (normal.y < 0.9f)
                {
                    _OnSlope = true;
                }
            }
            else if (normal.y > 0f)
            {
                _SlopeStop = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateColllision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateColllision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        _Grounded = false;
    }

    public bool IsGround()
    {
        return coyoteTimeCounter > 0f;
    }

}
