using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Takedown_Cinematic : MonoBehaviour
{
    [HideInInspector]
    public Time_Manager tM;

    public bool runTakedown;
    bool initTakedown;

    public int t_index;
    public bool xray;
    public List<TakedownHolder> takedownList = new List<TakedownHolder>();

    public Transform camHelper;

    Takedown_Timeline curTimeLine;

    public bool debugMode;
    public int debugTD;

    public int xrayLayer;
    public int defaultLayer;

    public GameObject mainCameraRig;
    public Transform mainCamera;
    float curFov;

    public Takedown_References playerRef;
    public Takedown_References enemyRef;

    Vector3 storeCamPosition;

    private void Start()
    {
        tM = Time_Manager.GetInstance();
        camHelper = new GameObject().transform;

        InitTakeDownHolders();

        if (debugMode)
        {
            playerRef.Init();
            enemyRef.Init();
        }
    }

    private void Update()
    {
        if (runTakedown)
        {
            Takedown t = takedownList[t_index].timeLine.tD;

            curTimeLine = takedownList[t_index].timeLine;
            InitTakedown(takedownList[t_index]);
            curTimeLine.tD.xRay = xray;
            curTimeLine.Init(this, playerRef, enemyRef);

            if (!initTakedown)
            {
                curTimeLine.ChangeCameraTarget(0);

                InitParticles(t);

                t.enemy.transform.rotation = t.player.transform.rotation;     // remove if there is an AI
                Vector3 worldPos = t.enemy.transform.TransformDirection(t.info.offset);
                worldPos += t.enemy.transform.position;

                StartCoroutine(LerpToTargetPos_andPlayAnims(worldPos, t));

                initTakedown = true;
            }
            else
            {
                if (curTimeLine)
                {
                    if (curTimeLine.cinematicTakedown)
                    {
                        curTimeLine.Tick();
                    }
                }
            }
        }
    }

    IEnumerator LerpToTargetPos_andPlayAnims(Vector3 targetPos,Takedown _t)
    {
        Vector3 dest = targetPos;
        Vector3 from = _t.player.transform.position;

        float perc = 0;

        while (perc < 1)
        {
            if (curTimeLine.jumpCut)
                perc = 1;
            else
                perc += Time.deltaTime * 5;

            _t.player.transform.position = Vector3.Lerp(from, dest, perc);


            yield return null;
        }

        _t.cameraAnim.enabled = true;
        _t.cameraAnim.Play(curTimeLine.timelineName);

        _t.player.anim.CrossFade(_t.info.p_anim, _t.info.p_crossfade_timer);

        yield return new WaitForSeconds(_t.info.e_delay);

        _t.enemy.anim.CrossFade(_t.info.e_anim,
            _t.info.e_crossfade_timer);
    }

    private void InitParticles(Takedown t)
    {
        for(int i =0; i < t.particles.Length; i++)
        {
            ParticlesForTakedowns p = t.particles[i];

            GameObject go = p.particleGO;

            if (go == null)
            {
                go = Instantiate(p.particlePrefab,
                    transform.position,
                    Quaternion.identity);
            }
            if (p.particles.Length == 0)
            {
                p.particles = go.GetComponentsInChildren<ParticleSystem>();

            }
            p.particleGO = go;

            if (p.placeOnBone)
            {
                p.targetTrans = (p.playerBone) ?
                    t.player.anim.GetBoneTransform(p.bone) :
                    t.enemy.anim.GetBoneTransform(p.bone);
            }

            p.particleGO.transform.parent = p.targetTrans;
            p.particleGO.transform.localPosition = p.targetPos;
            p.particleGO.transform.rotation = Quaternion.Euler(Vector3.zero);

        }
    }

    void InitTakedown(TakedownHolder th)
    {
        if (th.timeLine.cinematicTakedown)
        {
            Cursor.lockState = CursorLockMode.Confined;

            storeCamPosition = mainCamera.transform.localPosition;
            mainCamera.transform.parent = curTimeLine.transform;
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

            if (curTimeLine.tD.xRay)
            {
                Transform xray = Camera_References.GetInstance().xray.transform;

                xray.parent = curTimeLine.transform;
                xray.localPosition = Vector3.zero;
                xray.localRotation = Quaternion.Euler(Vector3.zero);
                xray.gameObject.SetActive(true);

                curTimeLine.tD.c_xRay = Camera_References.GetInstance().xray;
            }

            mainCameraRig.SetActive(false);
        }
        th.holder.SetActive(true);
    }
    private void InitTakeDownHolders()
    {
        for(int i = 0; i < takedownList.Count; i++)
        {
            TakedownHolder t = takedownList[i];

            t.timeLine = t.holder.GetComponentInChildren<Takedown_Timeline>();

            t.holder.SetActive(false);
        }
    }

    public void CloseTakedown()
    {
        mainCameraRig.SetActive(true);

        Camera.main.fieldOfView = 60;
        Camera_References.GetInstance().xray.fieldOfView = 60;

        mainCamera.transform.parent = FreeCameraLook.GetInstance().pivot.GetChild(0);
        mainCamera.transform.localPosition = storeCamPosition;
        mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Camera_References.GetInstance().xray.transform.parent = FreeCameraLook.GetInstance().pivot.GetChild(0);
        Camera_References.GetInstance().xray.transform.localPosition = Vector3.zero;
        Camera_References.GetInstance().xray.transform.localRotation = Quaternion.Euler(Vector3.zero);
        
        Camera_References.GetInstance().xray.gameObject.SetActive(false);
        CloseAllTakedowns();
        initTakedown = false;
        runTakedown = false;
        curTimeLine.perc = 0;
        curTimeLine = null;

    }

    public void CloseAllTakedowns()
    {
        foreach(TakedownHolder t in takedownList)
        {
            t.holder.SetActive(false);
        }
    }
}
[System.Serializable]
public class TakedownHolder
{
    public string id;
    public Takedown_Timeline timeLine;
    public GameObject holder;
}

[System.Serializable]
public class Takedown
{
    public string id;
    public float totalLength;
    public Takedown_Info info;
    public Animator cameraAnim;
    public BezierCurve camPath;
    public Takedown_References player;
    public Takedown_References enemy;
    [HideInInspector]
    public int cam_t_index;
    public Takedown_CamTargets[] camT;
    public bool xRay;
    public Transform cameraRig;
    public Camera c_xRay;
    public ParticlesForTakedowns[] particles;
}

[System.Serializable]
public class Takedown_Info
{
    public string p_anim;
    public float p_crossfade_timer = 0.2f;
    public string e_anim;
    public float e_crossfade_timer = 0.2f;
    public float e_delay;
    public Vector3 offset;
}

[System.Serializable]
public class Takedown_CamTargets
{
    public Transform target;
    public bool assignBone=true;
    public HumanBodyBones bone;
    public bool fromPlayer;
    public bool jumpTo;
}
[System.Serializable]
public class ParticlesForTakedowns
{
    public GameObject particlePrefab;
    [HideInInspector]
    public GameObject particleGO;
    public bool placeOnBone;
    public bool playerBone;
    public HumanBodyBones bone;
    public Transform targetTrans;
    [HideInInspector]
    public ParticleSystem[] particles;
    public Vector3 targetPos;
    public Vector3 targetRot;
}
