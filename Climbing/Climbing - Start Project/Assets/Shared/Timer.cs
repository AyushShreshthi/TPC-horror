﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private class TimedEvent
    {
        public float TimeToExecute;
        public Callback Method;

    }
    private List<TimedEvent> events;
    public delegate void Callback();

    private void Awake()
    {
        events = new List<TimedEvent>();

    }

    public void Add(Callback method,float inseconds)
    {
        events.Add(new TimedEvent
        {
            Method = method,
            TimeToExecute=Time.time+inseconds
        });
    }

    private void Update()
    {
        if (events.Count == 0)
            return;

        for (int i = 0; i < events.Count; i++)
        {
            var timedEvent = events[i];
            if (timedEvent.TimeToExecute <= Time.time)
            {
                timedEvent.Method();
                events.Remove(timedEvent);
            }
        }
    }
}
