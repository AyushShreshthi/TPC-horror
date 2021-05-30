#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    [ExecuteInEditMode]
    public class HandlePoints : MonoBehaviour
    {
        [Header("Helper Properties")]
        public bool dismountPoint;
        public bool fallPoint;
        public bool hangingPoints;
        public bool singlePoint;

        [Header("Click this after every change")]
        public bool updatePoints;

        [Header("Helper Utilities")]
        public bool deleteAll;
        public bool createIndicators;

        public GameObject pointPrefab;
        float posInterval = 0.5f;

        public Point furthestLeft;
        public Point furthestRight;

        [HideInInspector]
        public List<Point> pointsInOrder;

        void HandlePrefab()
        {
            pointPrefab = Resources.Load("Point") as GameObject;

            if (pointPrefab == null)
            {
                Debug.Log("no point prefab found");
            }

        }
        private void Update()
        {
            if (updatePoints)
            {
                HandlePrefab();
                UpdatePoints();
                updatePoints = false;
            }
            if (createIndicators)
            {
                HandlePrefab();

                if (!singlePoint)
                    CreateIndicators();
                else
                    CreateIndicator_Single();

                createIndicators = false;
            }
            if (deleteAll)
            {
                DeleteAll();
                deleteAll = false;
            }
        }

        private void DeleteAll()
        {
            Point[] ps = GetComponentsInChildren<Point>();

            for(int i = 0; i < ps.Length; i++)
            {
                DestroyImmediate(ps[i].transform.parent.gameObject);
            }
        }

        private void CreateIndicator_Single()
        {
            GameObject leftPoint = Instantiate(pointPrefab) as GameObject;
            leftPoint.transform.parent = transform;
            leftPoint.transform.localPosition = Vector3.zero;
            leftPoint.transform.localEulerAngles = Vector3.zero;
        }

        private void CreateIndicators()
        {
            GameObject leftPoint = Instantiate(pointPrefab) as GameObject;
            GameObject rightPoint = Instantiate(pointPrefab) as GameObject;

            leftPoint.transform.parent = transform;
            leftPoint.transform.localPosition = -(Vector3.right / 2);

            rightPoint.transform.parent = transform;
            rightPoint.transform.localPosition = (Vector3.right / 2);

            leftPoint.transform.localEulerAngles = Vector3.zero;
            rightPoint.transform.localEulerAngles = Vector3.zero;

            furthestLeft = leftPoint.GetComponentInChildren<Point>();
            furthestRight = rightPoint.GetComponentInChildren<Point>();
        }

        Vector3 GetPos(Point p)
        {
            return p.transform.parent.position;
        }
        private void UpdatePoints()
        {
            return;
            Point[] ps = GetComponentsInChildren<Point>();

            if (singlePoint)
            {
                pointsInOrder = new List<Point>();

                for(int i = 0; i < ps.Length; i++)
                {
                    pointsInOrder.Add(ps[i]);
                }

                return;
            }
            if(ps.Length < 1)
            {
                Debug.Log("no edge point indicator found");
                return;
            }
            DeletePrevious(ps, furthestLeft, furthestRight);

            ps = null;
            ps = GetComponentsInChildren<Point>();

            CreatePoints( furthestLeft, furthestRight);
        }

        private void DeletePrevious(Point[] ps, Point furthestLeft, Point furthestRight)
        {
            for(int i = 0; i < ps.Length; i++)
            {
                if(ps[i]!=furthestLeft && ps[i] != furthestRight)
                {
                    DestroyImmediate(ps[i].gameObject.transform.parent.gameObject);
                }
            }
        }

        private void CreatePoints( Point furthestLeft, Point furthestRight)
        {
            float disLtoR = Vector3.Distance(GetPos(furthestLeft), GetPos(furthestRight));
            int pointCount = Mathf.FloorToInt(disLtoR / posInterval);
            Vector3 direction = GetPos(furthestRight) - GetPos(furthestLeft);
            direction.Normalize();
            Vector3[] positions = new Vector3[pointCount];

            float interval = 0;
            pointsInOrder = new List<Point>();
            pointsInOrder.Add(furthestLeft);

            for(int i = 0; i < pointCount; i++)
            {
                interval += posInterval;
                positions[i] = GetPos(furthestLeft) + (direction * interval);

                if (Vector3.Distance(positions[i], GetPos(furthestRight)) > posInterval)
                {
                    GameObject p = Instantiate(pointPrefab, positions[i], Quaternion.identity) as GameObject;
                    p.transform.parent = transform;
                    pointsInOrder.Add(p.GetComponentInChildren<Point>());

                }
                else
                {
                    furthestRight.transform.parent.transform.localPosition
                         = transform.InverseTransformPoint(positions[i]);

                    break;
                }
            }
            pointsInOrder.Add(furthestRight);
        }
    }
}
#endif
