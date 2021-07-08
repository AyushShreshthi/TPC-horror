using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Speed : MonoBehaviour
{
    Time_Manager tM;

    ParticleSystem[] ps;

    private void Start()
    {
        tM = Time_Manager.GetInstance();
        ps = GetComponentsInChildren<ParticleSystem>();

    }

    private void Update()
    {
        for(int i = 0; i < ps.Length; i++)
        {
            ps[i].playbackSpeed = tM.myTimeScale;
        }
    }
}
