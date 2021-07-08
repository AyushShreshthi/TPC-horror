using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public float Vertical,Horizontal;
    public Vector2 MouseInput;
    public bool Fire1,Fire2,Reload,IsWaliking,IsSprinting,IsCrouched,MouseWheelUp,MouseWheelDown;

    void Update()
    {
        Vertical = Input.GetAxis("Vertical");
        Horizontal = Input.GetAxis("Horizontal");
        MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Fire1 = Input.GetButton("Fire1");
        Fire2 = Input.GetButton("Fire2");
        Reload = Input.GetKey(KeyCode.R);

        IsWaliking = Input.GetKey(KeyCode.LeftAlt);
        IsSprinting = Input.GetKey(KeyCode.LeftShift);
        IsCrouched = Input.GetKey(KeyCode.C);

        MouseWheelUp = Input.GetAxis("Mouse ScrollWheel")>0;
        MouseWheelDown = Input.GetAxis("Mouse ScrollWheel")<0;
    }

}
