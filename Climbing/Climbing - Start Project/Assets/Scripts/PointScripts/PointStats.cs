using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    public class PointStats : MonoBehaviour
    {
        public List<HipPos> hipPos = new List<HipPos>();
        public List<IKPositions> ikPos = new List<IKPositions>();

        public IKPositions GetIKPos(AvatarIKGoal goal)
        {
            IKPositions r = null;

            for(int i = 0; i < ikPos.Count; i++)
            {
                if (ikPos[i].ik == goal)
                {
                    r = ikPos[i];
                    break;
                }
            }
            return r;
        }

        public HipPos GetHip(PointType t)
        {
            HipPos r = null;

            for(int i = 0; i < hipPos.Count; i++)
            {
                if (hipPos[i].type == t)
                {
                    r = hipPos[i];
                    break;
                }
            }
            return r;
        }
    }
    [System.Serializable]
    public class HipPos
    {
        public PointType type;
        public Vector3 hipPos;
    }
}
