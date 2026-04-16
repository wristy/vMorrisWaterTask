using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;   // Movement speed of the player
    public float turnSpeed = 100.0f;
    public float gravity = -9.81f;

    [Header("Arena Bounds")]
    public bool enforceArenaBounds = true;
    public Transform arenaCenter;
    public float boundaryBuffer = 0.25f;

    // References to components
    private CharacterController controller;  // Player's CharacterController
    private Transform cameraTransform;       // Camera for mouse look
    private bool isFrozen = false; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!isFrozen)
        {
            HandleLook();
            HandleMovement();
            HandleGravity();
        }

        ClampToArenaBounds();
    }

    void HandleLook()
    {
        // Get input from A/D or left/right arrow keys for rotation
        float rotateInput = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows

        // Rotate the player around the Y-axis (left and right)
        transform.Rotate(Vector3.up, rotateInput * turnSpeed * Time.deltaTime);
    }

    void HandleMovement()
    {
        // Get input from W or up arrow key to move forward
        float moveForward = Input.GetAxis("Vertical"); // W or Up arrow

        if (moveForward > 0) // Only move forward, not backward
        {
            // Move the player forward based on its current rotation (Z axis)
            Vector3 move = transform.forward * moveForward;

            // Apply movement using the CharacterController
            controller.Move(move * moveSpeed * Time.deltaTime);
        }
    }

    void HandleGravity()
    {
        gravity -= 9.81f * Time.deltaTime;
        controller.Move( new Vector3(0, gravity, 0) );
        if ( controller.isGrounded ) gravity = 0;
    }

    public void FreezePlayer()
    {
        isFrozen = true;
    }
    public void UnfreezePlayer()
    {
        isFrozen = false;
    }

    public Vector3 ClampPositionToArena(Vector3 position)
    {
        if (!enforceArenaBounds)
            return position;

        float maxRadius = GetMaxArenaRadius();
        if (maxRadius <= 0f)
            return position;

        Vector3 center = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Vector2 offset = new Vector2(position.x - center.x, position.z - center.z);
        float sqrDist = offset.sqrMagnitude;
        float maxSqr = maxRadius * maxRadius;

        if (sqrDist <= maxSqr)
            return position;

        Vector2 clamped = offset.normalized * maxRadius;
        return new Vector3(center.x + clamped.x, position.y, center.z + clamped.y);
    }

    void ClampToArenaBounds()
    {
        Vector3 clamped = ClampPositionToArena(transform.position);
        Vector3 delta = clamped - transform.position;
        if (delta.sqrMagnitude < 0.0001f)
            return;

        if (controller != null && controller.enabled)
        {
            controller.enabled = false;
            transform.position = clamped;
            controller.enabled = true;
        }
        else
        {
            transform.position = clamped;
        }
    }

    float GetMaxArenaRadius()
    {
        float maxRadius = GameSettings.circleRadius - boundaryBuffer;
        float controllerRadius = 0f;
        if (controller != null)
            controllerRadius = controller.radius;

        maxRadius -= controllerRadius;
        return Mathf.Max(0f, maxRadius);
    }

}
