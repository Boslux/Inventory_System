using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Mover : MonoBehaviour
{
    private global::Inputs inputs;
    private Rigidbody rb;
    private SimplePhysics simplePhysics;

    private Vector2 moveInput;
    private float sprintMultiplier = 1f;
    private bool isDefending;

    [SerializeField] private float speed = 4.5f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float defenceSpeedFactor = 0.5f;
    [SerializeField] private float dodgeSpeed = 9f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float gravity = 25f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundDistance = 0.15f;
    [SerializeField] private float maxFallSpeed = 35f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private CapsuleCollider capsuleCollider;


    private void Awake()
    {
        inputs = new global::Inputs();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Physics is handled by SimplePhysics, so prevent built-in gravity from doubling.
        rb.useGravity = false;
        rb.isKinematic = true;

        simplePhysics = new SimplePhysics(
            gravity,
            jumpForce,
            groundDistance,
            transform,
            capsuleCollider,
            groundMask,
            maxFallSpeed
        );
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
        inputs.Player.Dodge.performed += PerformDodge;
        inputs.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        inputs.Player.Disable();
        inputs.Player.Dodge.performed -= PerformDodge;
        inputs.Player.Jump.performed -= Jump;
    }

    private void Update()
    {
        moveInput = MovementVectorNormalized();

        // Sprint (holds button)
        sprintMultiplier = inputs.Player.Sprint.ReadValue<float>() > 0.5f ? sprintSpeedMultiplier : 1f;

        // Defence (holds button)
        isDefending = inputs.Player.Defence.ReadValue<float>() > 0.5f;

        // Jump (press)
        if (inputs.Player.Jump.triggered)
        {
            simplePhysics.Jump();
        }

        // Dodge (press)
        if (inputs.Player.Dodge.triggered)
        {
            PerformDodge();
        }

        PlayerAnim.instance.Walking(IsWalking());
    }

    private void FixedUpdate()
    {
        Movement();
    }

    #region Movement
    private Vector2 MovementVectorNormalized()
    {
        return inputs.Player.Movement.ReadValue<Vector2>().normalized;
    }

    private void Movement()
    {
        Vector3 moveDirection = GetCameraRelativeMoveDirection();
        float movementSpeed = speed * sprintMultiplier * (isDefending ? defenceSpeedFactor : 1f);
        simplePhysics.SetHorizontalVelocity(moveDirection * movementSpeed);

        simplePhysics.Update(Time.fixedDeltaTime);

        bool rootMotionDriving = PlayerAnim.instance != null && PlayerAnim.instance.IsRootMotionDrivingMotion();
        if (!rootMotionDriving)
        {
            Vector3 velocity = simplePhysics.GetVelocity();
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion smoothedRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(smoothedRotation);
        }
    }

    private void PerformDodge()
    {
        // Dodge in the current movement direction; if stationary, dodge forward.
        Vector3 dodgeDirection = GetCameraRelativeMoveDirection();
        if (dodgeDirection.sqrMagnitude <= 0.01f)
            dodgeDirection = transform.forward;

        simplePhysics.SetHorizontalVelocity(dodgeDirection.normalized * dodgeSpeed);
    }

    private void PerformDodge(InputAction.CallbackContext _)
    {
        PerformDodge();
    }
    public void Jump(InputAction.CallbackContext _)
    {
        simplePhysics.Jump();
    }

    public bool IsWalking()
    {
        return moveInput.sqrMagnitude > 0.01f;
    }

    private Vector3 GetCameraRelativeMoveDirection()
    {
        if (moveInput.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            return new Vector3(moveInput.x, 0f, moveInput.y);
        }

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraRight * moveInput.x) + (cameraForward * moveInput.y);
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection;
    }
    #endregion
}
