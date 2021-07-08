using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraRig
    {
        public Vector3 CameraOffset;
        public float CrouchHeight;
        public float Damping;
    }

    [SerializeField] CameraRig defaultCamera;
    [SerializeField] CameraRig aimCamera;
    //[SerializeField] CameraRig crouchCamera;
    Transform cameraLookTarget;
    PlayerForStieve localPlayer;


    private void Awake()
    {
        GameManager.Instance.OnLocalPlayerJoined += HandleOnLocalPlayerJoined;
    }
    void HandleOnLocalPlayerJoined(PlayerForStieve player)
    {
        localPlayer = player;
        cameraLookTarget = localPlayer.transform.Find("cameraLooktarget");

        if (cameraLookTarget == null)
        {
            cameraLookTarget = localPlayer.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (localPlayer == null)
            return;

        CameraRig cameraRig = defaultCamera;

        if (localPlayer.PlayerState.WeaponState == PlayerState.EWeaponState.AIMING || localPlayer.PlayerState.WeaponState == PlayerState.EWeaponState.AIMEDFIRING)
        {
            cameraRig = aimCamera;

            float targetHeight = cameraRig.CameraOffset.y + (localPlayer.PlayerState.MoveState == PlayerState.EMoveState.CROUCHING ? cameraRig.CrouchHeight : 0);

            Vector3 targetposition = cameraLookTarget.position + localPlayer.transform.forward * cameraRig.CameraOffset.z +
               localPlayer.transform.up * targetHeight +
               localPlayer.transform.right * cameraRig.CameraOffset.x;


            //Quaternion targetRotation = Quaternion.LookRotation(cameraLookTarget.position, Vector3.up);
            transform.position = Vector3.Lerp(transform.position, targetposition, cameraRig.Damping * Time.deltaTime);
            //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, cameraRig.Damping * Time.deltaTime);

        }
        else
        {
            float targetHeight = cameraRig.CameraOffset.y + (localPlayer.PlayerState.MoveState == PlayerState.EMoveState.CROUCHING ? cameraRig.CrouchHeight : 0);

            Vector3 targetposition = cameraLookTarget.position + localPlayer.transform.forward * cameraRig.CameraOffset.z +
               localPlayer.transform.up * targetHeight +
               localPlayer.transform.right * cameraRig.CameraOffset.x;


            Quaternion targetRotation = Quaternion.LookRotation(cameraLookTarget.position - targetposition, Vector3.up);
            transform.position = Vector3.Lerp(transform.position, targetposition, cameraRig.Damping * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, cameraRig.Damping * Time.deltaTime);

        }



    }

}
