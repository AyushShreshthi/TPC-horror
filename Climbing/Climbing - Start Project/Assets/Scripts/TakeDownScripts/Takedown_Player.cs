using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Takedown_Player : MonoBehaviour
{
    StateManagerShoot states;
    Input_Handler ih;

    Takedown_Cinematic tdManager;
    Takedown_References plRef;
    
    [HideInInspector]
    public Takedown_References enRef;

    GameObject crossCanvas;
    WeaponReferenceBase prevWeapon;

    bool initText;
    Text UItext;

    public int takedown;
    public bool xray;

    private void Start()
    {
        tdManager = GetComponentInChildren<Takedown_Cinematic>();
        UItext = CrosshairManager.GetInstance().pickItemText;

        plRef = GetComponent<Takedown_References>();
        plRef.Init();

        tdManager.mainCameraRig = FreeCameraLook.GetInstance().gameObject;
        tdManager.mainCamera = Camera.main.transform;

        crossCanvas = CrosshairManager.GetInstance().transform.parent.gameObject;

        states = GetComponent<StateManagerShoot>();
        ih = GetComponent<Input_Handler>();
    }

    private void FixedUpdate()
    {
        if (enRef)
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                if (!tdManager.runTakedown)
                {
                    tdManager.t_index = takedown;
                    tdManager.xray = xray;

                    ih.enabled = false;
                    states.dummyModel = true;
                    crossCanvas.SetActive(false);
                    states.cMovement.rb.velocity = Vector3.zero;

                    prevWeapon = states.weaponManager.ReturnCurrentWeapon();

                    states.weaponManager.SwitchWeaponWithTargetWeapon(tdManager.takedownList[tdManager.t_index].timeLine.takedownWeapon);

                    plRef.Init();

                    tdManager.playerRef = plRef;
                    tdManager.enemyRef = enRef;
                    tdManager.runTakedown = true;
                }
            }
            if (!initText)
            {
                UItext.gameObject.SetActive(true);
                UItext.text = "Press X for TakeDown";
                initText = true;
            }
        }
        else
        {
            if (initText)
            {
                UItext.gameObject.SetActive(false);
                initText = false;
            }
        }
    }

    public void EndTakeDown()
    {
        print("ending");
        tdManager.CloseTakedown();

        ih.enabled = true;
        states.dummyModel = false;
        states.weaponManager.SwitchWeaponWithTargetWeapon(prevWeapon);

        tdManager.runTakedown = false;
        tdManager.enemyRef = null;

        crossCanvas.SetActive(true);
        Time_Manager.GetInstance().myTimeScale = 1;

        
        Cursor.lockState = CursorLockMode.Locked;
    }
}
