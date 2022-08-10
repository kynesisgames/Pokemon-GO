using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kynesis.Utilities
{
    public static class VectorExtensions
    {
        public static Vector2 Average(this IEnumerable<Vector2> enumerable)
        {
            if(enumerable.Count() == 0)
                return Vector2.zero;
            
            float x = enumerable.Average(vector => vector.x);
            float y = enumerable.Average(vector => vector.y);
            return new Vector2(x, y);
        }

        public static bool Approximately(this Vector3 vector1, Vector3 vector2, float threshold = 0.001f)
            => Vector3.Distance(vector1, vector2) < threshold;

        public static Vector3 FlattenY(this Vector3 vector) => Vector3.Scale(vector, new Vector3(1, 0, 1));

        public static float Magnitude(this IEnumerable<Vector3> enumerable)
        {
            float magnitude = 0;
            
            for (int i = 0; i < enumerable.Count() - 1; i++)
            {
                Vector3 current = enumerable.ElementAt(i);
                Vector3 next = enumerable.ElementAt(i + 1);
                magnitude += (next - current).magnitude;
            }

            return magnitude;
        }
    }
}
