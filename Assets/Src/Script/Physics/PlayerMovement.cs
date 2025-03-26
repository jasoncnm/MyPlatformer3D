
using MoreMountains.Tools;
using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;


public class PlayerMovement : MonoBehaviour
{
    #region Variables
    enum Mode { ThirdPerson = 0, FirstPerson = 1 }

    public InputReader input;

    IPlayerMode playerMode;

    [SerializeField] LayerMask probeMask = -1;
   
    [SerializeField] CinemachineCamera[] Cameras;

    [SerializeField] Transform PlayerModel;

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
    float rotateAngle = 0;

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
    public Vector3 forwardDir, reverseDir;

    Vector3 forwardMovement;
    Vector3 contactNormal;
    Vector3 originalposition = new Vector3(605.580017f, 1.5f, -352.73999f);
    Vector3 originalscale = Vector3.one;
    Vector3 originalGravity = Physics.gravity;
    
    Quaternion originalrotation = Quaternion.identity;

    Vector2 moveValue;

    Rigidbody rb;

    int cameraIndex = -1;
    int timeStepsSinceLastGrounded = 0;
    int timeStepsSinceLastJump = 0;
    int groundContactCount = 0;

    #endregion

    #region InputEvents
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

    #endregion

    #region Debug
    private void OnValidate()
    {
        Physics.gravity = Vector3.down * gravityScale;
        originalGravity = Physics.gravity;
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void DebugCode()
    {
        debugPlayerRenderer.material.SetColor("_BaseColor", _AniGrounded ? Color.white : Color.black);
        // Debug.Log("HoldJump: " + _HoldJump);
        // Debug.Log(_AniGrounded);
        // Debug.Log(_Jumping);
    }


    private void OnDrawGizmos()
    {
        if (rb != null) Gizmos.DrawLine(rb.position, Vector3.down * probeDistance);
    }
    #endregion

    #region Init Code
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
    private void Start()
    {
        originalposition = transform.position;
        transform.rotation = originalrotation;
        transform.localScale = originalscale;
        // TODO: TEMPORARY! need to implement switch mode
        SwitchPlayerModeTo(Mode.ThirdPerson);
    }
    #endregion

    #region UpdateMethods
    void SwitchPlayerModeTo(Mode to)
    {
        if (cameraIndex != (int)to)
        {
            bool _ThirdPerson = to == Mode.ThirdPerson;
            playerMode = _ThirdPerson ? new ThirdPersonMode() : new FirstPersonMode();
            PlayerModel.gameObject.SetActive(_ThirdPerson);
            Cameras[(int)to].Priority.Value = 2;
            Cameras[(int)(to + 1) % 2].Priority.Value = 1;
            
            if (to == Mode.FirstPerson)
            {
                CinemachinePanTilt panTilt = Cameras[(int)Mode.FirstPerson].GetComponent<CinemachinePanTilt>();
                InputAxis A = new() { Value = transform.rotation.eulerAngles.y, Range = new Vector2(-180, 180), Wrap = true, Center = 0, Recentering = InputAxis.RecenteringSettings.Default };
                panTilt.PanAxis = A;

                InputAxis B = new() { Value = transform.rotation.eulerAngles.x, Range = new Vector2(-180, 180), Wrap = true, Center = 0, Recentering = InputAxis.RecenteringSettings.Default };
                panTilt.TiltAxis = B;
            }
            
            cameraIndex = (int)to;
        }
    }

    void MoveAndTurn()
    {
        // Debug.Log(moveValue.magnitude);
        _Moving = moveValue.magnitude > 0.001f;
        Vector3 direction = new Vector3(moveValue.x, 0.0f, moveValue.y).normalized;
        float ratio = moveValue.magnitude;
        Debug.Assert(ratio >= 0f && ratio <= 1.001f, "Assertion Faile (ratio : " + ratio.ToString() + ")");
        
        if (_Sprint)
        {
            speed = normalSpeed * sprintFactor;
            _Walk = false;

        }
        else
        {
            _Walk = true;
            speed = normalSpeed * ratio;
        }

        // TODO: FixThis
        forwardMovement = playerMode.SimulateMovement(direction.x, direction.z, speed, acceleration, velPower, Cameras[cameraIndex].transform.eulerAngles.y, transform.eulerAngles.y,
                                           turnSmoothTime, ref turnSmoothVelocity, ref rotateAngle, rb.linearVelocity);
        transform.rotation = Quaternion.Euler(0.0f, rotateAngle, 0.0f);
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
            _Jumping = true;
        }
        else if (buttonUp && rb.linearVelocity.y > 0f)
        {
            _JumpReleased = true;
        }

        if (!_Grounded) contactNormal = Vector3.up;

    }

    #endregion

    #region Update Code
    // Update is called once per frame
    void Update()
    {
        // NOTE: Test Code
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchPlayerModeTo(Mode.FirstPerson);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchPlayerModeTo(Mode.ThirdPerson);
        }

        MoveAndTurn();
        SimulateJump(_JumpButtonDown, _JumpButtonCancelled);
        _Stop = rb.linearVelocity.magnitude < 0.01f;
        velocity = rb.linearVelocity;
        DebugCode();
        _JumpButtonCancelled = false;
        _JumpButtonDown = false;
    }

    private void FixedUpdate()
    {
        UpdateState();

        Jump();
        Move();

        ClearState();
    }

    #endregion

    #region FixUpdateMethods
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


    public void ResetState()
    {
        transform.position = originalposition;
        transform.rotation = originalrotation;
        transform.localScale = originalscale;
        rb.linearVelocity = Vector3.zero;
    }

    #endregion

    #region Collisions
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

        if (_Grounded)
        {
            _Jumping = false;
        }

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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Inside"))
        {
            SwitchPlayerModeTo(Mode.FirstPerson);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Inside"))
        {
            SwitchPlayerModeTo(Mode.ThirdPerson);
        }
    }

    #endregion

    #region Public Methods

    public bool IsGround()
    {
        return coyoteTimeCounter > 0f;
    }

    public void SetHoldJump(Toggle toggle)
    {
        _HoldJump = toggle.isOn;
        // Debug.Log(_HoldJump);
    }

    #endregion

}
