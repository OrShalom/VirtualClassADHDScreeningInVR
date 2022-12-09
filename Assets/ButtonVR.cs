using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonVR : MonoBehaviour
{
    public UnityEvent ButtonPress;
    public UnityEvent ButtonReleased;
    GameObject presser = null;
    bool isPressed;

    void Start()
    {
        isPressed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && presser == null)
        {
            presser = other.gameObject;
            isPressed = true;
            ButtonPress.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            isPressed = false;
            presser = null;
            ButtonReleased.Invoke();
        }
    }
}