
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] LayerMask probeMask = -1;

    [SerializeField] InputReader input;

    [SerializeField] Transform Cam;

    [SerializeField] Renderer debugPlayerRenderer;

    [SerializeField, Range(1f, 100f)]
    float gravityScale = 10f;

    [SerializeField, Range(1f, 5f)]
    float sprintFactor = 1.5f;
    
    [SerializeField]
    float normalSpeed, maxSpeed, acceleration, jump, groundFriction, airFriction, slopeFriction;

    [SerializeField, Min(0f)] float probeDistance = 1f;
    
    [SerializeField, Range(0f, 1f)] float turnSmoothTime;

    [SerializeField, Range(0f, 1f)] float jumpBufferTime = 0.2f, coyoteTime = 0.2f;
 
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;

    [SerializeField, Range(0f, 100f)] float maxSnapSpeed = 100f;


    public float fallTime;
    
    float velPower;
    float turnSmoothVelocity;
    float jumpBufferCounter;
    float minGroundDotProduct;
    float coyoteTimeCounter;
    float speed;

    public bool _Jumping { get; private set; } = false;
    public bool _AniGrounded { get; private set; } = true;
    public bool _Moving { get; private set; } = false;
    public bool _Sprint { get; private set; } = false;
    public bool _Walk { get; private set; } = false;
    public bool _Stop { get; private set; } = false;

    bool _JumpButtonDown = false;
    bool _JumpButtonCancelled = false;
    bool _SlopeStop = false;
    bool _JumpReleased = false;
    bool _OnSlope = false;
    bool _Jump = false;
    bool _Grounded;
    bool _HoldJump = false;

    public Vector3 velocity{ get; private set; }

    Vector3 forwardMovement;
    Vector3 contactNormal;
    Vector3 originalposition = new Vector3(0f, 1.55999994f, 0f);
    Vector3 originalscale = Vector3.one;
    Vector3 originalGravity = Physics.gravity;
    
    Quaternion originalrotation = Quaternion.identity;

    Vector2 moveValue;

    Rigidbody rb;

    int timeStepsSinceLastGrounded = 0;
    int timeStepsSinceLastJump = 0;
    int groundContactCount = 0;
  

    private void OnEnable()
    {
        input.moveEvent += OnMove;
        input.jumpEvent += OnJump;
        input.jumpCancelledEvent += OnJumpCancelled;
        input.sprintEvent += OnSprint;
        input.sprintCancelledEvent += OnSprintCancelled;

    }
    
    private void OnDisable()
    {
        input.moveEvent -= OnMove;
        input.jumpEvent -= OnJump;
        input.jumpCancelledEvent -= OnJumpCancelled;
        input.sprintEvent -= OnSprint;
        input.sprintCancelledEvent -= OnSprintCancelled;
    }

    void OnMove(Vector2 movement)
    {
        // Debug.Log("SET MOVE");
        moveValue = movement;
    }

    void OnJump()
    {
        _JumpButtonDown = true;
        _JumpButtonCancelled = false;
    }

    void OnJumpCancelled()
    {
        _JumpButtonCancelled = true;
        _JumpButtonDown = false;
    }

    void OnSprint()
    {
        _Sprint = true;
    }

    void OnSprintCancelled()
    {
        _Sprint = false;
    }


    private void OnValidate()
    {
        Physics.gravity = Vector3.down * gravityScale;
        originalGravity = Physics.gravity;
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        moveValue = Vector2.zero;
        OnValidate();
        speed = normalSpeed;
        rb = gameObject.GetComponent<Rigidbody>();
        velPower = 0.7f;
        turnSmoothTime = 0.0473f;
    }

    private void OnDrawGizmos()
    {
        if (rb != null) Gizmos.DrawLine(rb.position, Vector3.down * probeDistance);
    }


    Vector3 GetPlayerVelocity(float dirX, float dirZ)
    {
        Vector3 result;
        if (Mathf.Abs(dirX) > 0f || Mathf.Abs(dirZ) > 0f)
        {
            _Moving = true;
            Vector3 rbDir = rb.linearVelocity.normalized;
            float velDot = Vector2.Dot(new Vector2(dirX, dirZ), new Vector2(rbDir.x, rbDir.z));
            // Debug.Log(velDot);

            float targetAngle = Mathf.Atan2(dirX, dirZ) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            targetAngle = Mathf.Abs(targetAngle) < 0.0001f ? 0 : targetAngle;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            angle = Mathf.Abs(angle) < 0.0001f ? 0 : angle;

            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            Vector3 Dir = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;

            Vector3 targetVelocity = Dir.normalized * speed;

            Vector3 VelocityDiff = targetVelocity - rb.linearVelocity;

            float accelRate = acceleration;

            result = new Vector3(Mathf.Pow(Mathf.Abs(VelocityDiff.x) * accelRate, velPower) * Mathf.Sign(VelocityDiff.x), 0.0f,
                        Mathf.Pow(Mathf.Abs(VelocityDiff.z) * accelRate, velPower) * Mathf.Sign(VelocityDiff.z));
        }
        else
        {
            _Moving = false;
            result = Vector3.zero;
        }
        return result;
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
        // Debug.Log("Input Button Down Value: " + _JumpButtonDown);
        Vector3 direction = new Vector3(moveValue.x, 0.0f, moveValue.y).normalized;
        SimulateMovement(direction.x, direction.z, _Sprint, moveValue.magnitude);
        SimulateJump(_JumpButtonDown, _JumpButtonCancelled);
        
        _Jumping = _Jump || !_AniGrounded;
        _Stop = rb.linearVelocity.magnitude < 0.001f;
        velocity = rb.linearVelocity;
        DebugCode();
        Debug.Log("HoldJump: " +_HoldJump);
        _JumpButtonCancelled = false;
        _JumpButtonDown = false;
    }


    void SimulateMovement(float dirX, float dirZ, bool sprint, float ratio)
    {

        Debug.Assert(ratio >= 0f && ratio <= 1.001f, "Assertion Faile (ratio : " + ratio.ToString() + ")");
        if (sprint)
        {
            speed = normalSpeed * sprintFactor;
            _Walk = false;

        }
        else
        {
            _Walk = true;
            speed = normalSpeed * ratio;
        }

        // Debug.Log("DIRX: " + dirX.ToString() + " DIRZ: " + dirZ.ToString());
        forwardMovement = GetPlayerVelocity(dirX, dirZ);
    }

    void SimulateJump(bool buttonDown, bool buttonUp)
    {
        
        if (buttonDown)
        {
            jumpBufferCounter = jumpBufferTime;
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

    bool SnapToGround()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (timeStepsSinceLastGrounded > 1 || timeStepsSinceLastJump <= 2)
        {
            return false;
        }

        if (!Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        if (hit.normal.y < minGroundDotProduct)
        {
            return false;
        }
        groundContactCount = 1;
        contactNormal = hit.normal;

        float dot = Vector3.Dot(rb.linearVelocity, hit.normal);
        if (dot > 0f) rb.linearVelocity = (rb.linearVelocity - hit.normal * dot).normalized * speed;
        return true;
    }

    void UpdateState()
    {
        timeStepsSinceLastGrounded++;
        timeStepsSinceLastJump++;


        _AniGrounded = _Grounded || SnapToGround() || (timeStepsSinceLastGrounded < 8);
        Debug.Log(_AniGrounded);
        if (_Grounded || SnapToGround())
        {
            // Debug.Log(timeStepsSinceLastGrounded);
            timeStepsSinceLastGrounded = 0;
            if (groundContactCount > 1)
            {
                contactNormal = contactNormal.normalized;
            }
        }
       
        if (_OnSlope)
        {
            // Debug.Log("ONSLOPE");
            Physics.gravity = Vector3.zero;
        }
        else
        {
            Physics.gravity = originalGravity;
        }

        // Record time of falling
        if (rb.linearVelocity.y < -1f && !_AniGrounded)
        {
            //Debug.Log("FALL");
            fallTime += Time.fixedDeltaTime;
        }
    }

    void ClearState()
    {
        groundContactCount = 0;
        _Grounded = false;
        _SlopeStop = false;
        _OnSlope = false;
        _Jump = false;
        _JumpReleased = false;
    }

    private void FixedUpdate()
    {
        UpdateState();
        
        Jump();
        Move();

        ClearState();
    }

    void Jump()
    {
        
        if (_Jump)
        {
            timeStepsSinceLastJump = 0;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            rb.AddForce(Vector3.up * jump, ForceMode.Impulse);
        }

        if (_JumpReleased && _HoldJump)
        {
            rb.linearVelocity = Vector3.Scale(rb.linearVelocity, new Vector3(1f, 0.5f, 1f));
            // rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
        }

    }

 

    void Move()
    {
        forwardMovement = Vector3.ProjectOnPlane(forwardMovement, contactNormal);
        if (!_SlopeStop)
        {
            rb.AddForce(forwardMovement);
        }
        float friction;

        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
        {
            hVel = hVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(hVel.x, rb.linearVelocity.y, hVel.z);
        }
        // Debug.Log("Forward Direction = " + forwardDir.ToShortString() + " Reverse Direction = " + reverseDir.ToShortString());
        if (forwardMovement.magnitude < 0.01f)
        {
            forwardDir = rb.linearVelocity.normalized;
            reverseDir = -forwardDir;

            if (_OnSlope)
            {
                friction = slopeFriction;
            }
            else if (_Grounded)
            {
                friction = groundFriction;
            }
            else if (!_SlopeStop)
            {
                reverseDir.y = 0;
                friction = airFriction;
            }
            else
            {
                friction = 0f;
            }

            float amount = Mathf.Min(rb.linearVelocity.magnitude, Mathf.Abs(friction));

            rb.AddForce(reverseDir * amount, ForceMode.Impulse);

            //rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, new Vector3(0f, rb.linearVelocity.y, 0f),
                //decceleration * Time.fixedDeltaTime);

        }
    }
    public Vector3 forwardDir, reverseDir;
    void DebugCode()
    {
        debugPlayerRenderer.material.SetColor("_BaseColor", _AniGrounded ? Color.white : Color.black);

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
                groundContactCount += 1;
                _Grounded = true;
                contactNormal += normal;
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
        _SlopeStop &= !_Grounded;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateColllision(collision);

        if (_OnSlope && !_Moving)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateColllision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        fallTime = 0f;
        _Grounded = false;
    }

    public bool IsGround()
    {
        return coyoteTimeCounter > 0f;
    }

    public void SetHoldJump(Toggle toggle)
    {
        _HoldJump = toggle.isOn;
        // Debug.Log(_HoldJump);
    }

}
