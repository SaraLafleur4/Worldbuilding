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


    void Start(){
        moveSpeedFactor = 1f;
        rb = GetComponent<Rigidbody>();
        eye = gameObject.GetComponent<Transform>().GetChild(0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            _cameraEnabled = !_cameraEnabled;
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = _cameraEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (_cameraEnabled) {
            if (Input.GetMouseButtonDown(2)) {
                moveSpeedFactor = 1f;
            }

            moveSpeedFactor *= Mathf.Pow(2f, Input.GetAxis("Mouse ScrollWheel"));
            HandleMovement();
        } else {
            rb.velocity= Vector3.zero;
        }
    }

    private void HandleMovement(){
        _rotationX -=Input.GetAxis("Mouse Y")*sensitivity;
        _rotationY +=Input.GetAxis("Mouse X") *sensitivity;

        _rotationX = Mathf.Clamp(_rotationX,-90,90);

        eye.rotation = Quaternion.Euler(_rotationX,_rotationY,0);

        
        Vector3 moveDirection = eye.forward * Input.GetAxis("Vertical") * moveSpeed * moveSpeedFactor;
        moveDirection += eye.right * Input.GetAxis("Horizontal") * moveSpeed;

        // Apply movement
        rb.velocity = moveDirection;
    }
}
