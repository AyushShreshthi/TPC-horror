using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class StateManagerShoot : MonoBehaviour
{
    public bool aiming;
    public bool canRun;
    public bool dontRun;
    public bool walk;
    public bool shoot;
    public bool actualShooting;
    public bool reloading;
    public bool onGround;

    public bool down;
    [HideInInspector]
    public LastStand lastStand;

    public bool crouching;
    public float stance;

    public float coverPercentage;
    public CoverPosition coverPos;
    public bool inCover;
    public int coverDirection;
    public bool canAim;
    public bool crouchCover;
    public bool aimAtSides;

    public bool vaulting;
    bool climb;
    public BezierCurve vaultCurve;
    public BezierCurve climbCurve;
    Vector3 curvePos;
    bool initVault;

    public bool meleeWeapon;

    public float horizontal;
    public float vertical;
    public Vector3 lookPosition;
    public Vector3 lookHitPosition;
    public LayerMask layerMask;

    public CharacterAudioManager audioManager;

    [HideInInspector]
    public int weaponAnimType;

    [HideInInspector]
    public Handle_Animations handleAnimations;
    [HideInInspector]
    public Handle_Shooting handleShooting;
    [HideInInspector]
    public Weapon_Manager weaponManager;
    [HideInInspector]
    public IKHandler ikHandler;
    [HideInInspector]
    public Player_Movement cMovement;

    [HideInInspector]
    public GameObject model;

    public bool switchCharacter;
    public int targetChar;

    [HideInInspector]
    public Time_Manager tm;
    public float myDelta;

    public bool dummyModel;

    private void Start()
    {
        tm = Time_Manager.GetInstance();

        model = transform.GetChild(0).gameObject;

        handleAnimations = GetComponent<Handle_Animations>();
        audioManager = GetComponent<CharacterAudioManager>();
        handleShooting = GetComponent<Handle_Shooting>();
        weaponManager = GetComponent<Weapon_Manager>();
        ikHandler = GetComponent<IKHandler>();
        cMovement = GetComponent<Player_Movement>();
        lastStand = GetComponent<LastStand>();

        if (lastStand)
            lastStand.Init(this);

        if (vaultCurve)
            vaultCurve.transform.parent = null;
        if (climbCurve)
            climbCurve.transform.parent = null;
    }

    private void FixedUpdate()
    {
       myDelta = tm.GetFixDelta();
       // myDelta = Time.deltaTime;

        if (switchCharacter)
        {
            switchCharacter = false;

            if (Resource_Manager.getInstance() != null)
            {
                Resource_Manager.getInstance().SwitchCharacterModelWithIndex(this, targetChar);
            }
        }

        onGround = IsOnGround();

        walk = canWalk();

        HandleStance();
        HandleVault();

        if(down && lastStand)
        {
            ikHandler.bypassAngleClamp = true;
            ikHandler.Update();
            lastStand.Tick();

            handleShooting.Update();
            audioManager.Update();
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            print("goli lgi");
        }
    }
    private bool canWalk()
    {
        if (!inCover)
        {
            if (!dontRun)
                walk = (Input_Handler.GetInstance().fire3 > 0);
            else
                walk = true;

        }
        if (inCover)
        {
            walk = true;
        }
        return walk;
    }

    private void HandleVault()
    {
        if (vaulting)
        {
            BezierCurve curve = (climb) ? climbCurve : vaultCurve;

            float lineLength = curve.length;

            float speedModifier = handleAnimations.anim.GetFloat("CurveSpeed");

            float speed = (climb) ? 4 * speedModifier : 6;

            float movement = speed * Time.deltaTime;

            float lerpMovement = movement / lineLength;

            percentage += lerpMovement;

            if (percentage > 1)
            {
                vaulting = false;
            }

            Vector3 targetPosition = curve.GetPointAt(percentage);

            transform.position = targetPosition;
        }
    }

    float targetStance;
    void HandleStance()
    {
        if (!crouching)
        {
            targetStance = 1;
        }
        else
        {
            targetStance = 0;
        }
        stance = Mathf.Lerp(stance, targetStance, Time.deltaTime * 3f);
    }
    public void GetInCover(CoverPosition cover)
    {
        float disFromPos1 = Vector3.Distance(transform.position, cover.curvePath.GetPointAt(0));

        coverPercentage = disFromPos1 / cover.length;

        Vector3 targetPos = cover.curvePath.GetPointAt(coverPercentage);

        StartCoroutine(LerpToCoverPositionPercentage(targetPos));

        coverPos = cover;
        inCover = true;
    }

    IEnumerator LerpToCoverPositionPercentage(Vector3 targetPos)
    {
        Vector3 startingPos = transform.position;
        Vector3 tPos = targetPos;
        tPos.y = transform.position.y;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5;

            transform.position = Vector3.Lerp(startingPos, tPos, t);
            yield return null;
        }
    }
    private bool IsOnGround()
    {
        bool retVal = false;

        Vector3 origin = transform.position + new Vector3(0, 0.05f, 0);
        RaycastHit hit;

        if(Physics.Raycast(origin,-Vector3.up,out hit, 0.5f, layerMask))
        {
            retVal = true;
        }

        return retVal;
    }

    public void Vault(bool climb=false)
    {
        this.climb = false;
        this.climb = climb;

        BezierCurve curve = (climb) ? climbCurve : vaultCurve;

        curve.transform.rotation = transform.rotation;
        curve.transform.position = transform.position;

        string desiredAnimation = (climb) ? "Climb" : "Vault";

        handleAnimations.anim.CrossFade(desiredAnimation, 0.2f);
        curve.close = false;
        percentage = 0;
        vaulting = true;
    }

    float percentage;
    bool ignoreVault;

    public void DisableController()
    {
        dummyModel = true;
        cMovement.rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
    }
    public void EnableController()
    {
        dummyModel = false;
        cMovement.rb.isKinematic = false;
        GetComponent<Collider>().isTrigger = false;
    }
}
