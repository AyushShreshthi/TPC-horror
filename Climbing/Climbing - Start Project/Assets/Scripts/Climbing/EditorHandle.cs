#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Climbing
{
    [CustomEditor(typeof(DrawWireCube))]
    public class DrawWireCubeEditor:Editor
    {
        void OnSceneGUI()
        {
            DrawWireCube t = target as DrawWireCube;

            if (t.ikPos.Count == 0)
            {
                t.ikPos = t.transform.GetComponent<Point>().iks;
            }
            for(int i = 0; i < t.ikPos.Count; i++)
            {
                if (t.ikPos[i].target != null)
                {
                    Color targetColor = Color.red;

                    switch (t.ikPos[i].ik)
                    {
                        case AvatarIKGoal.LeftFoot:
                            targetColor = Color.magenta;
                            break;

                        case AvatarIKGoal.LeftHand:
                            targetColor = Color.cyan;
                            break;

                        case AvatarIKGoal.RightFoot:
                            targetColor = Color.green;
                            break;
                        case AvatarIKGoal.RightHand:
                            targetColor = Color.yellow;
                            break;
                    }
                    Handles.color = targetColor;

                    Handles.CubeCap(0, t.ikPos[i].target.position, t.ikPos[i].target.rotation, 0.05f);

                    if (t.ikPos[i].hint != null)
                    {
                        Handles.CubeCap(0, t.ikPos[i].hint.position, t.ikPos[i].hint.rotation, 0.05f);

                    }
                }
                else
                {
                    t.ikPos = t.transform.GetComponent<Point>().iks;
                }
            }
        }
    }

    [CustomEditor(typeof(DrawLine))]
    public class EditorVis : Editor
    {
        private void OnSceneGUI()
        {
            DrawLine t = target as DrawLine;

            if (t == null)
                return;

            if (t.ConnectedPoints.Count == 0)
            {
                t.ConnectedPoints.AddRange(t.transform.GetComponent<HandlePointConnections>().GetAllConnections());
            }
            for (int i = 0; i < t.ConnectedPoints.Count; i++)
            {
                Vector3 pos1 = Vector3.zero;
                Vector3 pos2 = Vector3.zero;

                if (t.ConnectedPoints[i].target1 == null || t.ConnectedPoints[i].target2 == null)
                    continue;

                switch (t.lineOrigin)
                {
                    case DrawLine.LineOrigin.hips:
                        pos1 = t.ConnectedPoints[i].target1.transform.position;
                        pos2 = t.ConnectedPoints[i].target2.transform.position;
                        break;
                    case DrawLine.LineOrigin.hands:
                        pos1 = t.ConnectedPoints[i].target1.transform.parent.position;
                        pos2 = t.ConnectedPoints[i].target2.transform.parent.position;
                        break;
                    case DrawLine.LineOrigin.root:
                        pos1 = t.ConnectedPoints[i].target1.transform.position;
                        pos2 = t.ConnectedPoints[i].target2.transform.position;
                        pos1.y += -0.86f;
                        pos2.y += -0.86f;
                        break;
                    default:
                        break;


                }

                switch (t.ConnectedPoints[i].cType)
                {
                    case ConnectionType.direct:
                        Handles.color = Color.red;
                        break;
                    case ConnectionType.inBetween:
                        Handles.color = Color.green;
                        break;

                }

                Handles.DrawLine(pos1, pos2);
                t.refresh = false;

            }
        }
    }

    [CustomEditor(typeof(DrawLineIndividual))]
    public class DrawLineVis : Editor
    {
        private void OnSceneGUI()
        {
            DrawLineIndividual t = target as DrawLineIndividual;

            if (t == null)
                return;

            if (t.ConnectedPoints.Count == 0)
            {
                t.ConnectedPoints.AddRange(t.transform.GetComponent<Point>().neighbours);
            }
            for(int i = 0; i < t.ConnectedPoints.Count; i++)
            {
                if (t.ConnectedPoints[i].target == null)
                {
                    continue;
                }

                Vector3 pos1 = Vector3.zero;
                Vector3 pos2 = Vector3.zero;

                switch (t.lineOrigin)
                {
                    case DrawLine.LineOrigin.hips:
                        pos1 = t.transform.position;
                        pos2 = t.ConnectedPoints[i].target.transform.position;
                        break;
                    case DrawLine.LineOrigin.hands:
                        pos1 = t.transform.parent.position;
                        pos2 = t.ConnectedPoints[i].target.transform.parent.position;
                        break;
                    case DrawLine.LineOrigin.root:
                        pos1 = t.transform.position;
                        pos2 = t.ConnectedPoints[i].target.transform.position;
                        pos1.y += -0.86f;
                        pos2.y += -0.86f;
                        break;
                    default:
                        break;


                }
                switch (t.ConnectedPoints[i].cType)
                {
                    case ConnectionType.direct:
                        Handles.color = Color.red;
                        break;
                    case ConnectionType.inBetween:
                        Handles.color = Color.green;
                        break;

                }

                Handles.DrawLine(pos1, pos2);
                t.refresh = false;

            }
        }
    }
}

#endif
