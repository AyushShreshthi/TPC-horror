using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DIsable_Game_Object : MonoBehaviour
{
    public GameObject targetObject;

    private void OnEnable()
    {
        targetObject.SetActive(true);
    }
    private void OnDisable()
    {
        targetObject.SetActive(false);
    }
}
