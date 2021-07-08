using System;
using UnityEngine;

public class FreeCameraLook : Pivot {

	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float turnSpeed = 1.5f;
	[SerializeField] private float turnsmoothing = .1f;
	[SerializeField] private float tiltMax = 75f;
	[SerializeField] private float tiltMin = 45f;
	[SerializeField] private bool lockCursor = false;

	public float lookAngle;
	private float tiltAngle;

	private const float LookDistance = 100f;

	private float smoothX = 0;
	private float smoothY = 0;
	private float smoothXvelocity = 0;
	private float smoothYvelocity = 0;

    public float crosshairOffsetWiggle = 0.2f;
    CrosshairManager crosshairManager;

    //add cover limits on the lookAngle
    public float coverAngleMax;
    public float coverAngleMin;
    public bool inCover;
    public int coverDirection;

    //add the singleton
    public static FreeCameraLook instance;
    
    public static FreeCameraLook GetInstance()
    {
        return instance;
    }

	protected override void Awake()
	{
        instance = this;

		base.Awake();

		cam = GetComponentInChildren<Camera>().transform;
		pivot = cam.parent.parent; //take the correct pivot
	}

    protected override void Start()
    {
        base.Start();

        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        crosshairManager = CrosshairManager.GetInstance();

        newTargetPosition = target.position;
    }
	
	// Update is called once per frame
    protected override	void Update ()
	{
		base.Update();

		HandleRotationMovement();

	}

    public bool overrideTarget;
    public Vector3 newTargetPosition;


	protected override void Follow (float deltaTime)
	{
        Vector3 tp = target.position;

        if (overrideTarget)
        {
            tp = newTargetPosition;
        }
        else
        {
            newTargetPosition = tp;
        }

        Vector3 targetPosition = Vector3.Lerp(transform.position, tp, deltaTime * moveSpeed);
        transform.position = targetPosition;
	}

	void HandleRotationMovement()
	{
        HandleOffsets();

		float x = Input.GetAxis("Mouse X") + offsetX;
		float y = Input.GetAxis("Mouse Y") + offsetY;

        if (turnsmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnsmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnsmoothing);
        }
        else
        {
            smoothX = x;
            smoothY = y;
        }

        if (!inCover)
        {
            lookAngle += smoothX * turnSpeed;
        }
        else
        {
            float angleFromWorldForward = Vector3.Angle(transform.forward, Vector3.forward);

            int dot = DotOrientation(angleFromWorldForward);

            float maxAngle = (angleFromWorldForward * dot) + coverAngleMax;
            float minAngle = (angleFromWorldForward * dot) + coverAngleMin;

            lookAngle += smoothX * turnsmoothing*10;

            lookAngle = Mathf.Clamp(lookAngle, minAngle, maxAngle);

        }

        if (lookAngle > 360)
            lookAngle = 0;
        if (lookAngle < -360)
            lookAngle = 0;


		transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

		tiltAngle -= smoothY * turnSpeed;
		tiltAngle = Mathf.Clamp (tiltAngle, -tiltMin, tiltMax);

		pivot.localRotation = Quaternion.Euler(tiltAngle,0,0);

        if (x > crosshairOffsetWiggle || x < -crosshairOffsetWiggle || y > crosshairOffsetWiggle || y < -crosshairOffsetWiggle)
        {
            WiggleCrosshairAndCamera(0);
        }
	}

    private int DotOrientation(float angleFromWorldForward)
    {
        float NSdot = Vector3.Dot(target.forward, Vector3.forward);

        float WEdot = Vector3.Dot(target.forward, Vector3.right);

        int retVal = 0;

        // first we will chek for north
        if (NSdot > 0)
        {

            //if we are looking towards the north, then 
            //we need to see which is closer, north or west/east
            //basically if its over 45 degrees its not the  north anymore
            if (angleFromWorldForward > 45)
            {
                retVal = WestOrEast(WEdot);
            }
            else // if it is under 45 then its the north
            {
                retVal = -coverDirection;
            }
        }
        else//for the south
        {

            //if its over 45
            if (angleFromWorldForward > 45)
            {
                retVal = WestOrEast(WEdot);
            }
            else// we are looking the south
            {
                retVal = -coverDirection;
            }
        }

        return retVal;
    }

    private int WestOrEast(float WEdot)
    {
        int retVal = 0;

        if (WEdot < 0)
        {
            if (coverDirection > 0)
                retVal = -coverDirection;
            else
                retVal = coverDirection;
        }
        else
        {
            if (coverDirection < 0)
            {
                retVal = -coverDirection;
            }
            else
                retVal = coverDirection;

        }

        return retVal;
    }

    float offsetX;
    float offsetY;


    void HandleOffsets()
    {
        if (offsetX != 0)
        {
            offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
        }

        if (offsetY != 0)
        {
            offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
        }
    }

    public void WiggleCrosshairAndCamera(float kickback)
    { 
        crosshairManager.activeCrosshair.WiggleCrosshair();

        offsetY = kickback;
    }


}
