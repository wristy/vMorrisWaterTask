using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;   // Movement speed of the player
    public float turnSpeed = 100.0f;
    public float gravity = -9.81f;

    // References to components
    private CharacterController controller;  // Player's CharacterController
    private Transform cameraTransform;       // Camera for mouse look


    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleGravity();
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

}
