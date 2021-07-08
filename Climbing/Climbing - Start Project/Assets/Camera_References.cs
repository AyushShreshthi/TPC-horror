using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_References : MonoBehaviour
{

    public Camera normalCamera;
    public Camera xray;

    private void Start()
    {
        xray.gameObject.SetActive(false);
    }

    public static Camera_References instance;
    public static Camera_References GetInstance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
    }
}
