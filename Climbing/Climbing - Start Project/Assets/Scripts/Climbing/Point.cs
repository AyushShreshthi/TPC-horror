using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [System.Serializable]
    public class Point : MonoBehaviour
    {
        public PointType pointType;
        public bool dismountPoint;
        public bool doubleSided;
        public List<Neighbour> neighbours = new List<Neighbour>();
        public List<IKPositions> iks = new List<IKPositions>();

        public IKPositions ReturnIK(AvatarIKGoal goal)
        {
            IKPositions retVal = null;

            for(int i = 0; i < iks.Count; i++)
            {
                if (iks[i].ik == goal)
                {
                    retVal = iks[i];
                    break;
                }
            }
            return retVal;
        }

        public Neighbour ReturnNeighbour(Point target)
        {
            Neighbour retVal = null;

            for(int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours[i].target == target)
                {
                    retVal = neighbours[i];
                    break;
                }
            }
            return retVal;
        }

        public Neighbour ReturnNeighbour_FromDIrection(Vector3 dir)
        {
            Neighbour retval = null;

            for(int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours[i].direction == dir)
                {
                    retval = neighbours[i];
                    break;
                }
            }

            return retval;
        }

    }
    [System.Serializable]
    public class IKPositions
    {
        public AvatarIKGoal ik;
        public Vector3 ikPos;
        public Vector3 hintPos;
        public AvatarIKHint ikHint;
        public bool hasHint;
    }
    [System.Serializable]
    public class Neighbour
    {
        public Vector3 direction;
        public Point target;
        public ConnectionType cType;
        public bool customConnection;
    }
    public enum ConnectionType
    {
        inBetween,
        direct,
        dismount,
        fall,
        jumpBack,
        jumpBack_onManager,
        hanging_turn_around,
        hanging_jump_forward,
        hanging_jump_air,
        corner_in,
        corner_out
    }
    public enum PointType
    {
        braced,
        hanging
    }
}
