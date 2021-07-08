using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasingSound : MonoBehaviour
{
    public AudioSource ad;
    public AudioClip onConcrete;
    
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9)
        {
            ad.PlayOneShot(onConcrete);
        }
        // different layers different sounds
    }
}
