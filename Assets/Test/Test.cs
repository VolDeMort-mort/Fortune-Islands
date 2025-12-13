using UnityEngine;
using UnityEngine.InputSystem; 

public class CameraSwitcher : MonoBehaviour
{
    public GameObject[] cameras;

    void Start()
    {
        EnableCamera(0);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            EnableCamera(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            EnableCamera(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            EnableCamera(2);
        }
    }

    void EnableCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].SetActive(false);
        }

        if (cameras.Length > index)
        {
            cameras[index].SetActive(true);
        }
    }
}