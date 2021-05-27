#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Climbing
{
    [ExecuteInEditMode]
    public class HandlePointConnections : MonoBehaviour
    {
        public float minDistance = 2.5f;
        public float directThreshold = 1;
        public bool updateConnections;
        public bool resetConnections;

        List<Point> allPoints = new List<Point>();
        Vector3[] availableDirections = new Vector3[8];

        void CreateDirections()
        {
            availableDirections[0] = new Vector3(1, 0, 0);
            availableDirections[1] = new Vector3(-1, 0, 0);
            availableDirections[2] = new Vector3(0, 1, 0);
            availableDirections[3] = new Vector3(0, -1, 0);
            availableDirections[4] = new Vector3(-1, -1, 0);
            availableDirections[5] = new Vector3(1, 1, 0);
            availableDirections[6] = new Vector3(1, -1, 0);
            availableDirections[7] = new Vector3(-1, 1, 0);
        }
        private void Update()
        {
            if (updateConnections)
            {
                GetPoints();
                CreateDirections();
                CreateConnections();
                FindDismountCanidates();
                RefreshAll();

                updateConnections = false;
            }
            if (resetConnections)
            {
                GetPoints();
                for(int p = 0; p < allPoints.Count; p++)
                {
                    allPoints[p].neighbours.Clear();

                }
                RefreshAll();
                resetConnections = false;
            }
        }


        void GetPoints()
        {
            allPoints.Clear();
            Point[] hp = GetComponentsInChildren<Point>();
            allPoints.AddRange(hp);
        }
        void CreateConnections()
        {
            for(int p = 0; p < allPoints.Count; p++)
            {
                Point curPoint = allPoints[p];

                for(int d = 0; d < availableDirections.Length; d++)
                {
                    List<Point> canidatePoints = CanidatePointsOnDirection(availableDirections[d], curPoint);

                    Point closest = ReturnClosest(canidatePoints, curPoint);
                   
                    if (closest != null)
                    {
                        if (Vector3.Distance(curPoint.transform.position, closest.transform.position) < minDistance)
                        {
                            //slip diagonal jumping

                            if(Mathf.Abs(availableDirections[d].y)>0 &&
                                Mathf.Abs(availableDirections[d].x) > 0)
                            {
                                if (Vector3.Distance(curPoint.transform.position, closest.transform.position) > directThreshold)
                                {
                                    continue;
                                }
                            }
                            //remove if u want to diagonal jump
                            
                            AddNeighbour(curPoint, closest, availableDirections[d]);
                        }
                    }
                }
            }
        }

        private void AddNeighbour(Point from , Point target, Vector3 targetDir)
        {
            Neighbour n = new Neighbour();
            n.direction = targetDir;
            n.target = target;
            n.cType = (Vector3.Distance(from.transform.position, target.transform.position) < directThreshold) ?
                ConnectionType.inBetween : ConnectionType.direct;

            from.neighbours.Add(n);

            UnityEditor.EditorUtility.SetDirty(from); 
        }

        private Point ReturnClosest(List<Point> l, Point from)
        {
            Point retVal = null;

            float minDist = Mathf.Infinity;

            for(int i = 0; i < l.Count; i++)
            {
                float tempDist = Vector3.Distance(l[i].transform.position, from.transform.position);

                if (tempDist < minDist && l[i] != from)
                {
                    minDist = tempDist;
                    retVal = l[i];
                }
            }
            return retVal;
        }

        private List<Point> CanidatePointsOnDirection(Vector3 targetDirection, Point from)
        {
            List<Point> retVal = new List<Point>();

            for(int p = 0; p < allPoints.Count; p++)
            {
                Point targetPoint = allPoints[p];

                Vector3 direction = targetPoint.transform.position - from.transform.position;
                Vector3 relativeDirection = from.transform.InverseTransformDirection(direction);

                if (IsDirectionValid(targetDirection, relativeDirection))
                {
                    retVal.Add(targetPoint);
                }
            }
            return retVal;
        }

        private bool IsDirectionValid(Vector3 targetDirection, Vector3 canidate)
        {
            bool retVal = false;

            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.y) * Mathf.Rad2Deg;
            float angle = Mathf.Atan2(canidate.x, canidate.y) * Mathf.Rad2Deg;

            if(angle<targetAngle+22.5f && angle > targetAngle - 22.5f)
            {
                retVal = true;
            }
            return retVal;
        }

        private void RefreshAll()
        {
            DrawLine dl = transform.GetComponent<DrawLine>();

            if (dl != null)
            {
                dl.refresh = true;
            }
            for(int i = 0; i < allPoints.Count; i++)
            {
                DrawLineIndividual d = allPoints[i].transform.GetComponent<DrawLineIndividual>();

                if (d != null)
                    d.refresh = true;
            }
        }

        private void FindDismountCanidates()
        {
            GameObject dismountPrefab = Resources.Load("Dismount") as GameObject;

            if (dismountPrefab == null)
            {
                Debug.Log("no dismount preab found");
                return;
            }

            HandlePoints[] hp = GetComponentsInChildren<HandlePoints>();

            List<Point> canidates = new List<Point>();

            for(int i = 0; i < hp.Length; i++)
            {
                if (hp[i].dismountPoint)
                {
                    canidates.AddRange(hp[i].pointsInOrder);
                }
            }

            if (canidates.Count > 0)
            {
                GameObject parentObj = new GameObject();
                parentObj.name = "Dismount Points";
                parentObj.transform.parent = transform;
                parentObj.transform.localPosition = Vector3.zero;
                parentObj.transform.position = canidates[0].transform.localPosition;

                foreach(Point p in canidates)
                {
                    Transform worldP = p.transform.parent;
                    GameObject dismountObject = Instantiate(dismountPrefab, worldP.position, worldP.rotation) as GameObject;

                    Vector3 targetPosition = worldP.position + ((worldP.forward / 1.6f) + Vector3.up * 1.7f);
                    dismountObject.transform.position = targetPosition;

                    Point dismountPoint = dismountObject.GetComponentInChildren<Point>();

                    Neighbour n = new Neighbour();
                    n.direction = Vector3.up;
                    n.target = dismountPoint;
                    n.cType = ConnectionType.dismount;
                    p.neighbours.Add(n);

                    Neighbour n2 = new Neighbour();
                    n2.direction = -Vector3.up;
                    n2.target = p;
                    n2.cType = ConnectionType.dismount;
                    dismountPoint.neighbours.Add(n2);

                    dismountObject.transform.parent = parentObj.transform;
                }
            }
        }

        public List<Connection> GetAllConnections()
        {
            List<Connection> retVal = new List<Connection>();

            for(int p = 0; p < allPoints.Count; p++)
            {
                for(int n = 0; n < allPoints[p].neighbours.Count; n++)
                {
                    Connection con = new Connection();
                    con.target1 = allPoints[p];
                    con.target2 = allPoints[p].neighbours[n].target;
                    con.cType = allPoints[p].neighbours[n].cType;

                    if (!ContainsConnection(retVal, con))
                    {
                        retVal.Add(con);
                    }
                }
            }
            return retVal;
        }

        private bool ContainsConnection(List<Connection> l, Connection c)
        {
            bool retVal = false;

            for(int i = 0; i < l.Count; i++)
            {
                if(l[i].target1==c.target1 && l[i].target2==c.target2
                    ||l[i].target2==c.target1 && l[i].target1 == c.target2)
                {
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }
    }
    public class Connection
    {
        public Point target1;
        public Point target2;
        public ConnectionType cType;
    }
}

#endif
