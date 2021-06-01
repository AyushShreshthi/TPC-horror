#if  UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;

namespace Climbing {
    [ExecuteInEditMode]
    public class ConnectionHelper : MonoBehaviour
    {
        public Point targetPoint;
        public Vector3 targetDirection;
        public ConnectionType connectionType;
        public bool addAsCustom;
        public bool makeConnection;

        private void Update()
        {
            if (makeConnection)
            {
                makeConnection = false;
                CreateConnection();
                targetPoint = null;

            }
        }

        private void CreateConnection()
        {
            Point thisPoint = GetComponent<Point>();

            if(thisPoint==null || targetPoint == null)
            {
                print("one of the points in null");
                return;

            }

            Neighbour n1 = new Neighbour();
            n1.target = targetPoint;
            n1.direction = targetDirection;
            n1.customConnection = addAsCustom;
            n1.cType = connectionType;

            thisPoint.neighbours.Add(n1);

            Neighbour n2 = new Neighbour();
            n2.target = thisPoint;
            n2.direction = -targetDirection;
            n2.customConnection = addAsCustom;
            n2.cType = connectionType;

            targetPoint.neighbours.Add(n2);

            UnityEditor.EditorUtility.SetDirty(thisPoint);
            UnityEditor.EditorUtility.SetDirty(targetPoint);

        }
    } 
}
#endif
