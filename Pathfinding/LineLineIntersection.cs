using UnityEngine;

namespace Pathfinding
{
    public class LineLineIntersection
    {
        public static bool AreLinesIntersecting(Vector2 start_1, Vector2 end_1, Vector2 start_2, Vector2 end_2, bool shouldIncludeEndPoints)
        {
            const float epsilon = 0.00001f;

            var isIntersecting = false;

            var denominator = (end_2.y - start_2.y) * (end_1.x - start_1.x) - (end_2.x - start_2.x) * (end_1.y - start_1.y);

            if (denominator == 0f) return false;
            
            var u_A = ((end_2.x - start_2.x) * (start_1.y - start_2.y) - (end_2.y - start_2.y) * (start_1.x - start_2.x)) / denominator;
            var u_B = ((end_1.x - start_1.x) * (start_1.y - start_2.y) - (end_1.y - start_1.y) * (start_1.x - start_2.x)) / denominator;
            
            if (shouldIncludeEndPoints)
            {
                if (u_A is >= 0f + epsilon and <= 1f - epsilon && u_B is >= 0f + epsilon and <= 1f - epsilon)
                {
                    isIntersecting = true;
                }
            }
            else
            {
                if (u_A is > 0f + epsilon and < 1f - epsilon && u_B is > 0f + epsilon and < 1f - epsilon)
                {
                    isIntersecting = true;
                }
            }

            return isIntersecting;
        }
        
        bool _isIntersecting2D(Vector2 start_1, Vector2 end_1, Vector2 start_2, Vector2 end_2)
        {
            var denominator = (end_2.y - start_2.y) * (end_1.x - start_1.x) - (end_2.x - start_2.x) * (end_1.y - start_1.y);

            if (denominator == 0) return false;
            
            var u_A = ((end_2.x - start_2.x) * (start_1.y - start_2.y) - (end_2.y - start_2.y) * (start_1.x - start_2.x)) / denominator;
            var u_B = ((end_1.x - start_1.x) * (start_1.y - start_2.y) - (end_1.y - start_1.y) * (start_1.x - start_2.x)) / denominator;
            
            return u_A is >= 0 and <= 1 && u_B is >= 0 and <= 1;
        }
        
        public static bool AreLineSegmentsIntersectingDotProduct(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            return _isPointsOnDifferentSides(p1, p2, p3, p4) && _isPointsOnDifferentSides(p3, p4, p1, p2);
        }

        static bool _isPointsOnDifferentSides(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            var lineDir = p2 - p1;

            var lineNormal = new Vector3(-lineDir.z, lineDir.y, lineDir.x);
            
            var dot1 = Vector3.Dot(lineNormal, p3 - p1);
            var dot2 = Vector3.Dot(lineNormal, p4 - p1);

            return dot1 * dot2 < 0f;
        }
        
        public static Vector2 GetLineLineIntersectionPoint(Vector2 start_1, Vector2 end_1, Vector2 start_2, Vector2 end_2)
        {
            var denominator = (end_2.y - start_2.y) * (end_1.x - start_1.x) - (end_2.x - start_2.x) * (end_1.y - start_1.y);

            var u_A = ((end_2.x - start_2.x) * (start_1.y - start_2.y) - (end_2.y - start_2.y) * (start_1.x - start_2.x)) / denominator;

            var intersectionPoint = start_1 + u_A * (end_1 - start_1);

            return intersectionPoint;
        }
        
        bool IsIntersecting_2D(Vector2 start_1, Vector2 end_1, Vector2 start_2, Vector2 end_2)
        {
            var direction_1 = (end_1 - start_1).normalized;
            var direction_2 = (end_2 - start_2).normalized;
            
            var normal_1 = new Vector2(-direction_1.y, direction_1.x);
            var normal_2 = new Vector2(-direction_2.y, direction_2.x);
            
            var a = normal_1.x;
            var b = normal_1.y;

            var c = normal_2.x;
            var d = normal_2.y;
            
            var k_1 = (a * start_1.x) + (b * start_1.y);
            var k_2 = (c * start_2.x) + (d * start_2.y);
            
            if (_isParallel(normal_1, normal_2))
            {
                Debug.Log("The lines are parallel so no solutions!");

                return false;
            }
            
            if (_isSameLine(start_1, start_2, normal_1))
            {
                Debug.Log("Same line so infinite amount of solutions!");
                
                return false;
            }
            
            var x_Intersect = (d * k_1 - b * k_2) / (a * d - b * c);
            var y_Intersect = (-c * k_1 + a * k_2) / (a * d - b * c);

            var intersectPoint = new Vector2(x_Intersect, y_Intersect);

            return _isBetween(start_1, end_1, intersectPoint) && _isBetween(start_2, end_2, intersectPoint);
        }

        static bool _isSameLine(Vector2 start_1, Vector2 start_2, Vector2 normal_1)
        {
            return _isOrthogonal(start_1 - start_2, normal_1);
        }

        static bool _isParallel(Vector2 v1, Vector2 v2)
        {
            return Vector2.Angle(v1, v2) == 0f || Mathf.Approximately(Vector2.Angle(v1, v2), 180f);
        }

        static bool _isOrthogonal(Vector2 v1, Vector2 v2)
        {
            return Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f;
        }
        
        static bool _isBetween(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = b - a;
            var ac = c - a;

            return Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude;
        }
    }
}