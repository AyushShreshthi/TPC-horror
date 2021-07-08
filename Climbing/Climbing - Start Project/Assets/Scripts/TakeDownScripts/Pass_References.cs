using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pass_References : MonoBehaviour
{
    public Takedown_References tr;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Takedown_Player>())
        {
            other.GetComponent<Takedown_Player>().enRef = tr;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Takedown_Player>())
        {
            if(other.GetComponent<Takedown_Player>().enRef == tr)
            {
                 other.GetComponent<Takedown_Player>().enRef = null;
                    
            }
        }
    }
}
