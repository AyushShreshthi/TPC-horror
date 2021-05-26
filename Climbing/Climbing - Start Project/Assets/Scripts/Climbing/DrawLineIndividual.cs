#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Climbing
{
    [ExecuteInEditMode]
    public class DrawLineIndividual : MonoBehaviour
    {
        public List<Neighbour> ConnectedPoints = new List<Neighbour>();

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
