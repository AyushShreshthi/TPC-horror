#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Climbing
{
    [ExecuteInEditMode]
    public class DrawLine : MonoBehaviour
    {
        public List<Connection> ConnectedPoints = new List<Connection>();

        public bool refresh;

        private void Update()
        {
            if (refresh)
            {
                ConnectedPoints.Clear();
                refresh = false;
            }
        }
    }
}
#endif
