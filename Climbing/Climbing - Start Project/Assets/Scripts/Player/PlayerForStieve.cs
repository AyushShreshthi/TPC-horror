using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerHealth))]

public class PlayerForStieve : MonoBehaviour
{
    [System.Serializable]
    public class MouseInput
    {
        public Vector2 Damping;
        public Vector2 Sensitivity;
        public bool LockMouse;
    }

    [SerializeField] SwatSoldier settings;

    [SerializeField] MouseInput MouseControl;
    [SerializeField] AudioController footSteps;
    [SerializeField] float minimumThreshold;

    public PlayerAim playerAim;

    Vector3 previousPosition;

    
    
    private PlayerState m_PlayerState;
    public PlayerState PlayerState
    {
        get
        {
            if (m_PlayerState == null)
               m_PlayerState = GetComponentInChildren<PlayerState>();
            return m_PlayerState;
        }
    }
    private PlayerHealth m_PlayerHealth;
    public PlayerHealth PlayerHealth
    {
        get
        {
            if (m_PlayerHealth == null)
                m_PlayerHealth = GetComponentInChildren<PlayerHealth>();
            return m_PlayerHealth;
        }
    }

    InputController playerInput;
    Vector2 mouseInput;

   private CharacterController m_MoveController;
    public CharacterController MoveController
    {
        get
        {
            if (m_MoveController == null)
            {
                m_MoveController = GetComponent<CharacterController>();
            }
            return m_MoveController;
        }
    }
    private PlayerShoot m_PlayerShoot;
    public PlayerShoot PlayerShoot
    {
        get
        {
            if (m_PlayerShoot == null)
                m_PlayerShoot = GetComponent<PlayerShoot>();

            return m_PlayerShoot;
        }
    }
    
    private void Awake()
    {
        playerInput = GameManager.Instance.InputController;
        GameManager.Instance.LocalPlayer = this;
        if (MouseControl.LockMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void Update()
    {
        if (!PlayerHealth.IsAlive)
            return;
        Move();
        LookAround();
    }

    private void LookAround()
    {
        mouseInput.x = Mathf.Lerp(mouseInput.x, playerInput.MouseInput.x, MouseControl.Damping.x);
        mouseInput.y = Mathf.Lerp(mouseInput.y, playerInput.MouseInput.y, MouseControl.Damping.y);

        transform.Rotate(Vector3.up * mouseInput.x * MouseControl.Sensitivity.x);
        
        playerAim.SetRotation(mouseInput.y * MouseControl.Sensitivity.y);
    }

    void Move()
    {
        float moveSpeed = settings.RunSpeed;

        if (playerInput.IsWaliking)
            moveSpeed = settings.WalkSpeed;

        if (playerInput.IsSprinting)
            moveSpeed = settings.SprintSpeed;

        if (playerInput.IsCrouched)
            moveSpeed = settings.CrouchSpeed;



        Vector2 direction = new Vector2(playerInput.Vertical *moveSpeed, playerInput.Horizontal * moveSpeed);
        MoveController.SimpleMove(transform.forward * direction.x + transform.right * direction.y);
       
        
        if (Vector3.Distance(transform.position,previousPosition)>minimumThreshold)
            footSteps.Play();

        previousPosition = transform.position;
    }
}
