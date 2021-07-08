using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[Serializable]
public class Crosshair : MonoBehaviour 
{
	public string CrosshairName; //we can use this as a tag to sort through our crosshairs if needed

	//self explanatory
	public float defaultSpread = 15;
	public float maxSpread = 50;
	public float wiggleSpread = 50;
	public float wiggleSpreadMaxTimer =60;

	//internal helper variables
	[HideInInspector]
	public float currentSpread = 0;
	private float targetSpread =0;
	private float spreadT =0;
	private Quaternion defaultRotation;
	private bool isSpreadWorking = true;

	//self explanatory
	public float spreadSpeed = 0.2f;
	public float rotationSpeed = 0.5f;
	public bool allowRotating = true;

	//more internal variables
	private float rotationTimer =0;
	private bool isRotating = false;

	//more self explanatory variables
	public bool spreadWhileRotating = false;
	public float rotationSpread = 0;

	public bool allowSpread = true;

	private bool wiggle = false;
	private float wiggleTimer =0;

	//an array to store each part of our crosshair
	public CrosshairPart[] parts;

	void Start()
	{
		//store our default rotation and the default spread
		defaultRotation = transform.rotation;
		currentSpread = defaultSpread;

		//change the crosshair spread
		ChangeCursorSpread(defaultSpread);
	}

	void Update()
	{
		if(isSpreadWorking) //if we want the crosshair to spread
		{
			//add to the timer
			spreadT += Time.deltaTime / spreadSpeed;

			//calculate our spread value with an equtation
			float spreadValue = AccelDecelInterpolation(currentSpread,targetSpread,spreadT);

			//if the timer is higher than one
			if(spreadT >1)
			{
				//we reached our target spread
				spreadValue = targetSpread;
				spreadT = 0;

				if(wiggle) // and if we want it to wiggle
				{
					//add to the wiggle timer
					if(wiggleTimer < wiggleSpreadMaxTimer)
					{
						wiggleTimer += Time.deltaTime;
					}
					else
					{
						wiggleTimer = 0;
						wiggle = false;
						//our targetSpread is the defaultSpread
						targetSpread = defaultSpread;
					}
				}
				else
				{
					isSpreadWorking = false;
				}
			}
			else//if(spreadT >1)
			{
				ChangeCursorSpread(defaultSpread);
			}

			currentSpread = spreadValue;
			
			ApplySpread();
		}//if(isSpreadWorking)

		if(isRotating) //if we want it to rotate
		{
			if(rotationTimer > 0) //and the rotation timer is higher than 0
			{
				rotationTimer -= Time.deltaTime; //decrease the timer

				//and apply the rotation
				transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
				                                      transform.rotation.eulerAngles.y,
				                                      transform.rotation.eulerAngles.z + (360 * Time.deltaTime * rotationSpeed));
				                                 
			}
			else
			{
				isRotating = false;
				transform.rotation = defaultRotation; //give the default rotation

				if(spreadWhileRotating)
				{
					ChangeCursorSpread(defaultSpread);//give the default spread if we changed it
				}
			}
		}
	}



	public void ApplySpread() //function that applies the spread
	{
		foreach(CrosshairPart im in parts)
		{
			im.image.rectTransform.anchoredPosition = im.up * currentSpread;
		}
	}

	public void WiggleCrosshair() //function that wiggles the crosshair
	{
		if(allowSpread)
		{
			ChangeCursorSpread(wiggleSpread);
			wiggle = true;
		}
	}

	public void ChangeCursorSpread(float value) //function that changes the spread
	{
		if(allowSpread)
		{
			isSpreadWorking = true;
			targetSpread = value;
			spreadT =0;
		}
	}

	public void rotateCursor(float seconds) //rotates it
	{
		if(allowRotating)
		{
			isRotating = true;
			rotationTimer = seconds;

			if(spreadWhileRotating)
			{
				ChangeCursorSpread(rotationSpeed);
			}
		}
	}


	//constructor for the array of our parts
	[Serializable]
	public class CrosshairPart
	{
		public Image image;
		public Vector2 up;
	}

	//our interpolation equation, this is just math
	public static float AccelDecelInterpolation(float start, float end, float t)
	{
		float x = end - start;

		float newT = (Mathf.Cos((t+1) * Mathf.PI)/2) + 0.5f;

		x *= newT;

		float retVal = start +x;

		return retVal;
	}

}











