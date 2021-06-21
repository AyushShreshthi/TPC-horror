#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Climbing;
using System;

[ExecuteInEditMode]

public class PointSetup_Utility : MonoBehaviour
{
    public bool savePointSetup;
    public List<HipPosUtility> hipPos = new List<HipPosUtility>();
    public List<HelpersIK> helpersIK = new List<HelpersIK>();

    private void Update()
    {
        if (savePointSetup)
        {
            CreatePoint();
            savePointSetup = false;
        }
    }

    private void CreatePoint()
    {
        GameObject go = new GameObject();
        go.AddComponent<PointStats>();
        PointStats ps = go.GetComponent<PointStats>();
        go.name = "PointStats";

        //Store Hip Positions
        foreach(HipPosUtility h in hipPos)
        {
            if (h.hipPos == null)
            {
                continue;
            }
            HipPos hp = new HipPos();
            hp.hipPos = h.hipPos.localPosition;
            hp.type = h.type;
            ps.hipPos.Add(hp);
        }

        //Store Ik positions
        for(int i = 0; i < helpersIK.Count; i++)
        {
            HelpersIK h = helpersIK[i];
            if (h.targetHint == null)
                continue;

            IKPositions ik = new IKPositions();
            ik.ikPos = h.ikTarget.localPosition;
            ik.ik = h.ikGoal;

            if (h.targetHint)
            {
                ik.hasHint = true;
                ik.hintPos = h.targetHint.localPosition;
                ik.ikHint = h.ikHint;
            }

            ps.ikPos.Add(ik);

            Debug.Log("Point Stats saved, don;t forget to apply it to the controller");

        }
    }
    [System.Serializable]
    public class HipPosUtility
    {
        public PointType type;
        public Transform hipPos;
    }
    [System.Serializable]
    public class HelpersIK
    {
        public AvatarIKGoal ikGoal;
        public Transform ikTarget;
        public Transform targetHint;
        public AvatarIKHint ikHint;
    }
}


#endif
