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
        public float directThreshold = 1.5f;
        public bool updateConnections;
        public bool resetConnections;
        public bool unparentIKs;

        List<Point> allPoints = new List<Point>();
        Vector3[] availableDirections = new Vector3[8];

        public List<GameObject> dismountPoints = new List<GameObject>();

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
                ClearGarbage();
                GetPoints();
                CreateDirections();
                CreateConnections();
                FindDismountCanidates();
                FindFallCanidates();
                FindHangingPoints();
                RefreshAll();

                updateConnections = false;
            }
            if (resetConnections)
            {
                ClearGarbage();
                GetPoints();
                for(int p = 0; p < allPoints.Count; p++)
                {
                    List<Neighbour> customConnections = new List<Neighbour>();

                    for(int i = 0; i < allPoints[p].neighbours.Count; i++)
                    {
                        if (allPoints[p].neighbours[i].customConnection)
                        {
                            customConnections.Add(allPoints[p].neighbours[i]);
                        }
                    }

                    allPoints[p].neighbours.Clear();

                    allPoints[p].neighbours.AddRange(customConnections);

                }
                RefreshAll();
                resetConnections = false;
            }
        }

        private void FindHangingPoints()
        {
            HandlePoints[] hp = GetComponentsInChildren<HandlePoints>();

            List<Point> canidates = new List<Point>();

            for(int i = 0; i < hp.Length; i++)
            {
                if (hp[i].hangingPoints)
                {
                    canidates.AddRange(hp[i].pointsInOrder);
                }
            }
            Point[] ps = GetComponentsInChildren<Point>();

            foreach(Point p in ps)
            {
                if (p.pointType == PointType.hanging)
                {
                    if (!canidates.Contains(p))
                    {
                        canidates.Add(p);
                    }
                }
            }

            if (canidates.Count > 0)
            {
                foreach(Point p in canidates)
                {
                    p.pointType = PointType.hanging;

                   // Vector3 targetP = Vector3.zero;
                    //targetP.y = -1f;
                    //p.transform.localPosition = targetP;
                }
            }
        }

        private void ClearGarbage()
        {
            foreach(GameObject go in dismountPoints)
            {
                DestroyImmediate(go);
            }
            dismountPoints.Clear();
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

            Point[] disPoint = GetComponentsInChildren<Point>();

            for(int i = 0; i < hp.Length; i++)
            {
                if (hp[i].dismountPoint)
                {
                    canidates.AddRange(hp[i].pointsInOrder);
                }
            }
            for(int i = 0; i < disPoint.Length; i++)
            {
                if (disPoint[i].dismountPoint)
                {
                    if (!canidates.Contains(disPoint[i]))
                    {
                        canidates.Add(disPoint[i]);
                    }
                }
            }

            if (canidates.Count > 0)
            {
                GameObject parentObj = new GameObject();
                parentObj.name = "Dismount Points";
                parentObj.transform.parent = transform;
                parentObj.transform.localPosition = Vector3.zero;
                parentObj.transform.position = canidates[0].transform.position;

                foreach(Point p in canidates)
                {
                    Transform worldP = p.transform.parent;
                    GameObject dismountObject = Instantiate(dismountPrefab, worldP.position, worldP.rotation) as GameObject;

                    Vector3 targetPosition = worldP.position + ((worldP.forward / 1.6f) + Vector3.up * 1.2f);
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
                    p.neighbours.Add(n2);

                    dismountPoint.dismountPoint = true;

                    dismountObject.transform.parent = parentObj.transform;

                    RaycastHit hit;
                    if (Physics.Raycast(dismountObject.transform.position, -Vector3.up, out hit, 2))
                    {
                        Vector3 gp = hit.point;
                        gp.y += 0.04f + Mathf.Abs(dismountPoint.transform.localPosition.y);
                        dismountObject.transform.position = gp;
                    }

                    dismountPoints.Add(dismountObject);
                }
            }
        }

        void FindFallCanidates()
        {
            Point[] ps = GetComponentsInChildren<Point>();

            foreach(Point p in ps)
            {
                Neighbour down = p.ReturnNeighbour_FromDIrection(-Vector3.up);

                if (down == null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(p.transform.position, -Vector3.up, out hit, 3))
                    {

                        Neighbour n = new Neighbour();
                        n.direction = -Vector3.up;
                        n.target = p;
                        n.cType = ConnectionType.fall;
                        p.neighbours.Add(n);
                    }
                }
            }

           /* HandlePoints[] hp = GetComponentsInChildren<HandlePoints>();

            List<Point> canidates = new List<Point>();

            for(int i = 0; i < hp.Length; i++)
            {
                if (hp[i].fallPoint)
                {
                    canidates.AddRange(hp[i].pointsInOrder);
                }
            }
            if (canidates.Count > 0)
            {
                foreach(Point p in canidates)
                {
                    Neighbour n = new Neighbour();
                    n.direction = -Vector3.up;
                    n.target = p;
                    n.cType = ConnectionType.fall;
                    p.neighbours.Add(n);
                }
            }*/
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
