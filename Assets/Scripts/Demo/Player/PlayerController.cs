using UnityEngine;
using UnityEngine.PlayerLoop;
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerAnim))]
[RequireComponent(typeof(PlayerPickup))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private float dodgeSpeed = 8f;
    [SerializeField] private float defenceSpeedFactor = 0.5f;
    [SerializeField] private LayerMask layer;

    private SimplePhysics physics;
    private CapsuleCollider capsuleCollider;
    private bool isDefending;

    void OnEnable()
    {
        InitializationComponent();
        RigidBodySet();
    }

    private void InitializationComponent()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        physics = new SimplePhysics(gravity, jumpForce, groundDistance, transform, capsuleCollider, layer);
    }

    private void RigidBodySet()
    {
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        physics.Update(Time.fixedDeltaTime);
        Vector3 velocity = physics.GetVelocity();
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void Jump()
    {
        physics.Jump();
    }

    public void SetVelocity(Vector3 velocity)
    {
        if (isDefending)
            velocity *= defenceSpeedFactor;

        physics.SetHorizontalVelocity(velocity);
    }

    public void Dodge()
    {
        physics.SetHorizontalVelocity(transform.forward * dodgeSpeed);
    }

    public void SetDefence(bool active)
    {
        isDefending = active;
    }

    public bool IsGrounded => physics.IsGrounded;
}
