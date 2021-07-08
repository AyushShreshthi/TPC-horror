using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPCamera : MonoBehaviour
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
    
    Transform cameraLookTarget;
    PlayerForStieve localPlayer;

    private void Awake()
    {
        GameManager.Instance.OnLocalPlayerJoined += HandleOnLocalPlayerJoined;

    }

    private void HandleOnLocalPlayerJoined(PlayerForStieve player)
    {
        localPlayer = player;
        cameraLookTarget = localPlayer.transform.Find("AimingPivot");

        if (cameraLookTarget == null)
            cameraLookTarget = localPlayer.transform;
    }
    private void LateUpdate()
    {

        if (localPlayer == null)
            return;

        CameraRig cameraRig = defaultCamera;

        if (localPlayer.PlayerState.WeaponState == PlayerState.EWeaponState.AIMING ||
            localPlayer.PlayerState.WeaponState == PlayerState.EWeaponState.AIMEDFIRING)
            cameraRig = aimCamera;

        float targetHeight = cameraRig.CameraOffset.y + (localPlayer.PlayerState.MoveState == PlayerState.EMoveState.CROUCHING ? cameraRig.CrouchHeight : 0);
        
        Vector3 targetPosition = cameraLookTarget.position + localPlayer.transform.forward * cameraRig.CameraOffset.z +
            localPlayer.transform.up *targetHeight+
            localPlayer.transform.right * cameraRig.CameraOffset.x;

       // Quaternion targetRotation = cameraLookTarget.rotation;

        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraRig.Damping * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraLookTarget.rotation, cameraRig.Damping*Time.deltaTime);
    }
}
