using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Takedown_Timeline : MonoBehaviour
{
    public string timelineName;
    public Takedown tD;
    Takedown_Cinematic main;

    [Range(0, 1)]
    public float perc;
    float delta;

    Vector3 targetPos;
    Transform targetTrans;

    public bool cinematicTakedown;
    public bool jumpCut;

    public WeaponReferenceBase takedownWeapon;

    public void Init(Takedown_Cinematic m, Takedown_References pl,Takedown_References en )
    {
        tD.player = pl;
        tD.enemy = en;

        if (main == null)
        {
            main = m;

            if (!cinematicTakedown)
            {
                tD.xRay = false;
            }
        }
    }

    public void Tick()
    {
        if (!tD.xRay)
        {
            if (tD.c_xRay)
                tD.c_xRay.gameObject.SetActive(false);
        }

        if (targetTrans)
            targetPos = targetTrans.position;


        main.camHelper.position = Vector3.Lerp(main.camHelper.position,
          targetPos , Time.deltaTime * 5);    // targetPos= targetTrans.position

        delta = main.tM.GetDelta();

        tD.player.anim.speed = main.tM.myTimeScale;
        tD.enemy.anim.speed = main.tM.myTimeScale;
        tD.cameraAnim.speed = main.tM.myTimeScale;

        perc += delta / tD.totalLength;

        if (perc > 1)
            perc = 1;

        if (perc < 0)
            perc = 0;

        Vector3 camPos = tD.camPath.GetPointAt(perc);

        if (camPos != Vector3.zero)
        {
            tD.cameraRig.transform.position = camPos;
        }

        tD.cameraRig.LookAt(main.camHelper);

    }

    IEnumerator ChangeTimeScale(float targetScale)
    {
        float target = targetScale;
        float cur = main.tM.myTimeScale;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 15;

            float v = Mathf.Lerp(cur, target, t);

            if (cinematicTakedown)
            {
                main.tM.myTimeScale = v;
            }

            yield return null;
        }
    }

    IEnumerator ChangeFOV(float targetValue)
    {
        float target = targetValue;
        float curValue=Camera.main.fieldOfView;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 15;

            float v = Mathf.Lerp(curValue, target, t);

            if (cinematicTakedown)
            {
                Camera.main.fieldOfView = v;

                if (tD.xRay)
                    tD.c_xRay.fieldOfView = v;
            }

            yield return null;
        }
    }
    public void PlayEvent(string e_name)
    {
        Invoke(e_name, 0);
    }

    public void BreakBone(string b_name)
    {
        BoneList bone = ReturnBone(b_name);

        if (bone != null)
        {
            bone.bone.SetActive(false);
            bone.destroyed = true;
        }
        PlayParticle();

    }
    public void PlayParticle(int i = 0)
    {
        foreach(ParticleSystem ps in tD.particles[i].particles)
        {
            ps.Play();
        }
    }
    BoneList ReturnBone(string target)
    {
        BoneList retVal = null;

        for(int i = 0; i < tD.enemy.boneList.Count; i++)
        {
            if (string.Equals(tD.enemy.boneList[i].boneId, target))
            {
                retVal = tD.enemy.boneList[i];
            }
        }
        return retVal;
    }

    public void ChangeDOF(int i)
    {
        if (COntrolDOF.GetInstance())
        {
            COntrolDOF.GetInstance().ChangeDOFValues(i);
        }
    }
    public void ChangeCameraTarget(int i)
    {
        Takedown_CamTargets tInfo = tD.camT[i];

        if (tInfo.assignBone)
        {
            if (tInfo.fromPlayer)
            {
                targetTrans = tD.player.anim.GetBoneTransform(tInfo.bone);
            }
            else
            {
                targetTrans = tD.enemy.anim.GetBoneTransform(tInfo.bone);
            }
        }
        else
        {
            targetTrans = tInfo.target;
        }

        if (tInfo.jumpTo)
        {
            main.camHelper.position = targetTrans.position;
        }
    }
    void OpenSkeleton_Enemy()
    {
        if (tD.xRay)
        {
            tD.enemy.OpenSkeleton();

            tD.player.ChangeLayer(main.xrayLayer);
        }
    }

    void CloseSkeleton()
    {
        tD.enemy.CloseSkeleton();
        tD.player.CloseSkeleton();
        tD.player.ChangeLayer(main.defaultLayer);
    }
}
