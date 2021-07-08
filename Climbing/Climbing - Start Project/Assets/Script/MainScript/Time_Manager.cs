using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Time_Manager : MonoBehaviour
{
    float myDelta;
    float myFixedDelta;
    public float myTimeScale = 1;

    private void Start()
    {
        myTimeScale = 1;
    }
    private void FixedUpdate()
    {
        myFixedDelta = Time.fixedDeltaTime * myTimeScale;
    }
    private void Update()
    {
        myDelta = Time.deltaTime * myTimeScale;
    }

    public float GetDelta()
    {
        return myDelta;
    }
    public float GetFixDelta()
    {
        return myFixedDelta;
    }

    public static Time_Manager instance;
    public static Time_Manager GetInstance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
    }
}
