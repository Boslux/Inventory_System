using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lightweight kinematic character physics helper.
/// Handles grounding, gravity and jump velocity.
/// </summary>
public class SimplePhysics
{
    private readonly Transform transform;
    private readonly CapsuleCollider capsule;
    private readonly LayerMask groundMask;
    private readonly float gravity;
    private readonly float jumpForce;
    private readonly float groundCheckDistance;
    private readonly float maxFallSpeed;
    private readonly float skinWidth;

    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;

    public SimplePhysics(
        float gravityValue,
        float jumpForceValue,
        float groundDistanceValue,
        Transform playerTransform,
        CapsuleCollider playerCapsule,
        LayerMask groundLayerMask,
        float maxFallSpeedValue = 35f,
        float skinWidthValue = 0.02f)
    {
        gravity = gravityValue;
        jumpForce = jumpForceValue;
        groundCheckDistance = groundDistanceValue;
        transform = playerTransform;
        capsule = playerCapsule;
        groundMask = groundLayerMask;
        maxFallSpeed = Mathf.Max(1f, maxFallSpeedValue);
        skinWidth = Mathf.Max(0.001f, skinWidthValue);
    }

    public void Update(float deltaTime)
    {
        CheckGround(deltaTime);
        ApplyGravity(deltaTime);
    }

    public void Jump()
    {
        if (!isGrounded)
        {
            return;
        }

        isGrounded = false;
        velocity.y = jumpForce;
    }

    public void SetHorizontalVelocity(Vector3 newVelocity)
    {
        Vector3 planar = new Vector3(newVelocity.x, 0f, newVelocity.z);
        if (isGrounded)
        {
            planar = Vector3.ProjectOnPlane(planar, groundNormal);
        }

        velocity.x = planar.x;
        velocity.z = planar.z;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public bool IsGrounded => isGrounded;

    private void ApplyGravity(float deltaTime)
    {
        if (isGrounded)
        {
            // Kinematic movement has no collision resolution, so do not push down while grounded.
            if (velocity.y < 0f) velocity.y = 0f;
            return;
        }

        velocity.y -= gravity * deltaTime;
        if (velocity.y < -maxFallSpeed)
        {
            velocity.y = -maxFallSpeed;
        }
    }

    private void CheckGround(float deltaTime)
    {
        float scaleX = Mathf.Abs(transform.lossyScale.x);
        float scaleY = Mathf.Abs(transform.lossyScale.y);
        float scaleZ = Mathf.Abs(transform.lossyScale.z);
        float radialScale = Mathf.Max(scaleX, scaleZ);

        float radius = capsule.radius * radialScale;
        float halfHeight = Mathf.Max(capsule.height * scaleY * 0.5f, radius);

        Vector3 center = transform.TransformPoint(capsule.center);
        Vector3 bottomCenter = center + Vector3.down * (halfHeight - radius);
        float downwardTravel = Mathf.Max(0f, -velocity.y * deltaTime);
        float probeDistance = groundCheckDistance + downwardTravel;

        // Cast from just above feet downward so we can detect ground below the character.
        Vector3 castOrigin = bottomCenter + Vector3.up * skinWidth;
        float castDistance = probeDistance + skinWidth;

        bool hitGround = Physics.SphereCast(
            castOrigin,
            radius * 0.95f,
            Vector3.down,
            out RaycastHit hit,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);

        isGrounded = hitGround;
        groundNormal = hitGround ? hit.normal : Vector3.up;
    }
}
