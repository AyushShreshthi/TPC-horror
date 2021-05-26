using UnityEngine;
using System.Collections;
using Controller;

[RequireComponent(typeof(StateManager))]
[RequireComponent(typeof(HandleMovement))]
public class InputHandler : MonoBehaviour {

    StateManager states;

    [HideInInspector]
    public Transform camHolder;

	void Start () {
        states = GetComponent<StateManager>();
        camHolder = Camera.main.transform;
    }
	
	void Update () {
        HandleAxis();

    }

    void HandleAxis()
    {
        states.horizontal = Input.GetAxis("Horizontal");
        states.vertical = Input.GetAxis("Vertical");
    }
}
