using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public float lookSpeed = 15.0f;
    public float moveSpeed = 10.0f;
    public float sprintSpeed = 30.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationY = rot.y;
        rotationX = rot.x;
    }

    void Update()
    {
        // 1. Поворот камери (Права кнопка миші затиснута)
        if (Input.GetMouseButton(1)) 
        {
            // Приховуємо курсор для зручності
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rotationY += Input.GetAxis("Mouse X") * lookSpeed;
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            
            // Обмежуємо погляд вгору/вниз, щоб не зламати шию
            rotationX = Mathf.Clamp(rotationX, -90, 90);

            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
        else
        {
            // Повертаємо курсор, коли кнопка відпущена
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. Рух (WASD)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDir += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow)) moveDir -= transform.forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.UpArrow)) moveDir -= transform.right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.UpArrow)) moveDir += transform.right;

        // 3. Рух вгору/вниз (Q / E)
        if (Input.GetKey(KeyCode.E)) moveDir += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) moveDir -= Vector3.up;

        transform.position += moveDir * currentSpeed * Time.deltaTime;
    }
}