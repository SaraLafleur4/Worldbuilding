using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float _rotationX;
    private float _rotationY;

    private bool _cameraEnabled = true;
    public float sensitivity = 5f;
    public float moveSpeed = 12f;
    private float moveSpeedFactor;

    private Rigidbody rb;
    private Transform eye;

    // Called once at the start of the game to initialize variables and lock the cursor
    void Start()
    {
        moveSpeedFactor = 1f;
        rb = GetComponent<Rigidbody>();
        eye = gameObject.GetComponent<Transform>().GetChild(0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called once per frame to handle input and camera movement
    void Update()
    {
        // Toggles camera control and cursor visibility when the right mouse button is pressed
        if (Input.GetMouseButtonDown(1))
        {
            _cameraEnabled = !_cameraEnabled;
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = _cameraEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (_cameraEnabled)
        {
            // Resets the movement speed factor when the middle mouse button is pressed
            if (Input.GetMouseButtonDown(2))
            {
                moveSpeedFactor = 1f;
            }

            // Adjusts movement speed based on the mouse scroll wheel
            moveSpeedFactor *= Mathf.Pow(2f, Input.GetAxis("Mouse ScrollWheel"));
            // Handles camera and character movement
            HandleMovement();
        }
        else
        {
            // Stops movement when the camera is disabled
            rb.velocity = Vector3.zero;
        }
    }

    // Handles camera rotation and player movement based on input
    private void HandleMovement()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * sensitivity;
        _rotationY += Input.GetAxis("Mouse X") * sensitivity;

        _rotationX = Mathf.Clamp(_rotationX, -90, 90);

        // Rotates the camera based on mouse movement
        eye.rotation = Quaternion.Euler(_rotationX, _rotationY, 0);

        // Calculates movement direction based on input
        Vector3 moveDirection = eye.forward * Input.GetAxis("Vertical") * moveSpeed * moveSpeedFactor;
        moveDirection += eye.right * Input.GetAxis("Horizontal") * moveSpeed;

        // Apply movement
        rb.velocity = moveDirection;
    }
}
