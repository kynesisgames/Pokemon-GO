using System.Collections.Generic;
using UnityEngine;

namespace Kynesis.Utilities
{
    public static class Bezier 
    {
        public static Vector3 GetExtrapolatedPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) 
        {
            return Vector3.LerpUnclamped
            (
                Vector3.LerpUnclamped(p0, p1, t), 
                Vector3.LerpUnclamped(p1, p2, t), 
                t
            );
        }
        
        public static List<Vector3> GetPath
            (Vector3 startPosition, Vector3 midPosition, Vector3 endPosition, float points)
        {
            return GetExtrapolatedPath(startPosition, midPosition, endPosition, 0f, 1f, points);
        }

        public static List<Vector3> GetExtrapolatedPath
            (Vector3 startPosition, Vector3 midPosition, Vector3 endPosition, float start, float end, float points)
        {
            if (end < start)
            {
                Debug.Log("End interpolation must be greater than start interpolation.");
                return null;
            }
            
            float interpolationLength = end - start;
            float step = interpolationLength / points;
            
            List<Vector3> path = new List<Vector3>();
            
            for (float interpolation = start; interpolation < end; interpolation += step)
            {
                Vector3 point = GetExtrapolatedPoint(startPosition, midPosition, endPosition, interpolation);
                path.Add(point);
            }
            
            return path;
        }
    }
}