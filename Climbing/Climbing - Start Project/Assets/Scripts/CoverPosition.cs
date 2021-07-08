using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPosition : MonoBehaviour
{
    public BezierCurve curvePath;
    public bool blockPos1;
    public bool blockPos2;

    public float length;

    public Covertype covertype;

    public enum Covertype
    {
        full,
        half
    }

    private void Start()
    {
        curvePath = GetComponentInChildren<BezierCurve>();
        length = curvePath.length;
    }
}
