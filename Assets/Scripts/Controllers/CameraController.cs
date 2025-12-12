using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public float lookSpeed = 15.0f;
    public float moveSpeed = 10.0f;
    public float sprintSpeed = 30.0f;

    [Header("View Limits")]
    [Tooltip("How far down can you look? (e.g. 90)")]
    public float maxVerticalAngle = 90.0f; 
    
    [Tooltip("How far up can you look? (e.g. -90)")]
    public float minVerticalAngle = -90.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationY = rot.y;
        rotationX = rot.x;

        if (rotationX > 180) rotationX -= 360;
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rotationY += Input.GetAxis("Mouse X") * lookSpeed;
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            
            rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDir += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveDir -= transform.forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDir -= transform.right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDir += transform.right;

        if (Input.GetKey(KeyCode.E)) moveDir += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) moveDir -= Vector3.up;

        transform.position += moveDir * currentSpeed * Time.deltaTime;
    }
}