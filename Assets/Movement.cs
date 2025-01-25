using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    
    [SerializeField] private float minVerticalAngle = -60f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [SerializeField] private Rigidbody rb;
    private Vector3 movement;

    private float pitch = 0f;
    private float yaw = 0f; 

    void Start()
    {
        if(rb == null)
            rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); 
        float moveVertical = Input.GetAxis("Vertical");     
        movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;                         
        pitch -= mouseY;                        
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        transform.eulerAngles = new Vector3(0f, yaw, 0f);
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.TransformDirection(movement) * Time.fixedDeltaTime * movementSpeed;
        rb.MovePosition(rb.position + moveDirection);
    }
}
