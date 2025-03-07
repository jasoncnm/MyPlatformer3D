using System.Collections;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField, Range(1f, 2f)]
    float sprintFactor = 1.5f;
    
    [SerializeField]
    float normalSpeed, maxSpeed, acceleration, jump, groundDecceleration, airDecceleration, slopeDecceleration;
    [SerializeField]
    Transform Cam;

    [SerializeField, Range(0f, .1f)]
    float turnSmoothTime;

    [SerializeField, Range(0f, 1f)] float jumpBufferTime = 0.2f;
    [SerializeField, Range(0f, 1f)] float releaseBufferTime = 0.2f;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;


    [SerializeField]
    bool ToggleSlide = true;

    [SerializeField]
    LayerMask groundedMask;

    [SerializeField]
    SphereCollider sphereCollider;

    float speed;
    
    float velPower;
    float turnSmoothVelocity;
    float jumpBufferCounter;
    float extraHeightTest = .2f;
    float offset = 0.1f;
    float minGroundDotProduct;

    bool _Grounded = false;
    bool _SlopeStop = false;
    bool _Jump = false;
    bool _JumpReleased = false;
    Vector3 forwardMovement, backwardMovement;
    Vector3 contactNormal;

    Vector3 originalposition = new Vector3(0f, 1.55999994f, 0f);
    Quaternion originalrotation = Quaternion.identity;
    Vector3 originalscale = Vector3.one;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        OnValidate();
        speed = normalSpeed;
        rb = gameObject.GetComponent<Rigidbody>();
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
        InputAction SprintAction = InputSystem.actions.FindAction("Sprint");
        InputAction moveAction = InputSystem.actions.FindAction("Move");

        Vector2 MoveValue = moveAction.ReadValue<Vector2>();

        

        bool JumpButtonDown = Input.GetButtonDown("Jump");
        bool JumpButtonUp = Input.GetButtonUp("Jump");
        Vector3 direction = new Vector3(MoveValue.x, 0.0f, MoveValue.y).normalized;

        SimulateMovement(direction.x, direction.z, SprintAction.IsPressed());
        SimulateJump(JumpButtonDown, JumpButtonUp);
    }


    private void FixedUpdate()
    {

        Jump();
        move();
        _Grounded = false;
        _SlopeStop = false;
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

        if ((jumpBufferCounter > 0f) && _Grounded)
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
            jumpBufferCounter = 0f;

            float alignedSpeed = Vector3.Dot(rb.linearVelocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                jump = Mathf.Max(jump - alignedSpeed, 0f);
            }
            rb.AddForce(contactNormal * jump, ForceMode.Impulse);
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
        forwardMovement = GetPlayerVelocity(dirX, dirZ);
        backwardMovement = -forwardMovement;
    }

#if false
    bool IsGrounded()
    {
        RaycastHit rayHit;
       // NOTE: Hacky way (maybe the only way) to prevent the spherecast not detect the touching object
        Ray ray = new Ray(sphereCollider.bounds.center + Vector3.up * offset, Vector3.down);

        bool result = Physics.SphereCast(ray, sphereCollider.bounds.extents.y, out rayHit, extraHeightTest);

        result = result && rayHit.normal.y >= minGroundDotProduct;

        if (result)
        {
            contactNormal = rayHit.normal.normalized;
            if (_jump == false)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
        else
        {
            contactNormal = Vector3.up;
        }

        Debug.Log(rayHit.collider);
        return result;
    }

#endif

    void move()
    {

        Debug.Log(_SlopeStop);

        rb.AddForce(forwardMovement);


        float decceleration;

        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
        {
            hVel = hVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(hVel.x, rb.linearVelocity.y, hVel.z);
        }

        if (forwardMovement.magnitude < 0.01f)
        {
            Debug.Log("StopMoving!");
            if (_Grounded)
            {
                decceleration = groundDecceleration;
            }
            else if (!_SlopeStop)
            {
                decceleration = airDecceleration;
            }
            else
            {
                decceleration = 0f;
            }

            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, new Vector3(0f, rb.linearVelocity.y, 0f),
                decceleration * Time.fixedDeltaTime);
        }
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
            Debug.Log(normal.y);
            if (normal.y >= minGroundDotProduct)
            {
                _Grounded = true;
                contactNormal = normal;
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

}
