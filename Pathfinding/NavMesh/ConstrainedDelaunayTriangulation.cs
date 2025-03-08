using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding.NavMesh
{
    public abstract class ConstrainedDelaunayTriangulation
    {
        public static List<(Vector3, Vector3, Vector3)> TriangulateIntersection(
            Node_Triangle triangle1, Node_Triangle triangle2, Vector3[] vertices)
        {
            var boundaryEdges = _getIntersectionBoundary(triangle1, triangle2);
            
             var delaunayTriangulation_Vector3 = DelaunayTriangulation_Vector3(vertices);
            
             delaunayTriangulation_Vector3 = _enforceConstraints(delaunayTriangulation_Vector3, boundaryEdges);
            
             var finalTriangles = _filterTrianglesInIntersection(delaunayTriangulation_Vector3, boundaryEdges);

            return finalTriangles;
        }

        static List<(Vector3, Vector3)> _getIntersectionBoundary(
            Node_Triangle triangle1, Node_Triangle triangle2)
        {
            var boundaryEdges = new List<(Vector3, Vector3)>();
            
            var t1V1 = triangle1.A.Position;
            var t1V2 = triangle1.B.Position;
            var t1V3 = triangle1.C.Position;

            var t2V1 = triangle2.A.Position;
            var t2V2 = triangle2.B.Position;
            var t2V3 = triangle2.C.Position;
            
            var triangle1Edges = new[] { (t1V1, t1V2), (t1V2, t1V3), (t1V3, t1V1) };
            var triangle2Edges = new[] { (t2V1, t2V2), (t2V2, t2V3), (t2V3, t2V1) };
            
            var intersectionPoints = new List<Vector3>();

            foreach (var edge1 in triangle1Edges)
            {
                foreach (var edge2 in triangle2Edges)
                {
                    if (_tryGetEdgeIntersection(edge1.Item1, edge1.Item2, edge2.Item1, edge2.Item2,
                            out var intersection))
                    {
                        intersectionPoints.Add(intersection);
                    }
                }
            }
            
            foreach (var edge in triangle1Edges)
            {
                if (Node_Triangle.IsPointInsideTriangle(edge.Item1, t2V1, t2V2, t2V3))
                {
                    FindBoundarySegment(edge.Item1, edge.Item2, triangle2, boundaryEdges, intersectionPoints);
                }
                else if (Node_Triangle.IsPointInsideTriangle(edge.Item2, t2V1, t2V2, t2V3))
                {
                    FindBoundarySegment(edge.Item2, edge.Item1, triangle2, boundaryEdges, intersectionPoints);
                }
            }
            
            foreach (var edge in triangle2Edges)
            {
                if (Node_Triangle.IsPointInsideTriangle(edge.Item1, t1V1, t1V2, t1V3))
                {
                    FindBoundarySegment(edge.Item1, edge.Item2, triangle1, boundaryEdges, intersectionPoints);
                }
                else if (Node_Triangle.IsPointInsideTriangle(edge.Item2, t1V1, t1V2, t1V3))
                {
                    FindBoundarySegment(edge.Item2, edge.Item1, triangle1, boundaryEdges, intersectionPoints);
                }
            }
            
            if (intersectionPoints.Count >= 2)
            {
                OrganizeBoundaryEdges(intersectionPoints, boundaryEdges);
            }

            return boundaryEdges;
        }

        static void FindBoundarySegment(
            Vector3 insidePoint, Vector3 outsidePoint, Node_Triangle triangle,
            List<(Vector3, Vector3)> boundaryEdges, List<Vector3> intersectionPoints)
        {
            var t1 = triangle.A.Position;
            var t2 = triangle.B.Position;
            var t3 = triangle.C.Position;
            
            var exitPoint = Vector3.zero;
            var foundExit = false;
            
            if (_tryGetEdgeIntersection(insidePoint, outsidePoint, t1, t2, out var intersection1))
            {
                exitPoint = intersection1;
                foundExit = true;
            }
            else if (_tryGetEdgeIntersection(insidePoint, outsidePoint, t2, t3, out var intersection2))
            {
                exitPoint = intersection2;
                foundExit = true;
            }
            else if (_tryGetEdgeIntersection(insidePoint, outsidePoint, t3, t1, out var intersection3))
            {
                exitPoint = intersection3;
                foundExit = true;
            }

            if (!foundExit) return;
            
            boundaryEdges.Add((insidePoint, exitPoint));
            
            if (!intersectionPoints.Contains(exitPoint))
            {
                intersectionPoints.Add(exitPoint);
            }
        }

        static void OrganizeBoundaryEdges(
            List<Vector3> intersectionPoints, List<(Vector3, Vector3)> boundaryEdges)
        {
            if (boundaryEdges.Count != 0 || intersectionPoints.Count < 2) return;
            
            var orderedPoints = GetConvexHull(intersectionPoints);
                
            for (var i = 0; i < orderedPoints.Count; i++)
            {
                var next = (i + 1) % orderedPoints.Count;
                boundaryEdges.Add((orderedPoints[i], orderedPoints[next]));
            }
        }
        
        static List<(Vector3, Vector3, Vector3)> DelaunayTriangulation_Vector3(Vector3[] vertices)
        {
            var superTriangleNode = Graph_NavMesh.CreateSuperTriangle(vertices.ToHashSet());
            var superTriangle = (superTriangleNode.A.Position, superTriangleNode.B.Position, superTriangleNode.C.Position);

            var triangulation = new List<(Vector3, Vector3, Vector3)> { superTriangle };
            
            foreach (var vertex in vertices)
            {
                var badTriangles = triangulation
                    .Where(triangle => Node_Triangle.IsPointInsideCircumcircle(triangle.Item1, triangle.Item2, triangle.Item3, vertex))
                    .ToList();
                
                var polygon = new List<(Vector3, Vector3)>();
                
                foreach (var triangle in badTriangles)
                {
                    var edges = new[]
                    {
                        (triangle.Item1, triangle.Item2),
                        (triangle.Item2, triangle.Item3),
                        (triangle.Item3, triangle.Item1)
                    };

                    foreach (var edge in edges)
                    {
                        var isShared = badTriangles.Count(t =>
                            Node_Triangle.SharesEdge(t, (edge.Item1, edge.Item2))) > 1;

                        if (!isShared)
                        {
                            polygon.Add(edge);
                        }
                    }
                }
                
                foreach (var triangle in badTriangles)
                {
                    triangulation.Remove(triangle);
                }
                
                foreach (var edge in polygon)
                {
                    triangulation.Add((vertex, edge.Item1, edge.Item2));
                }
            }

            foreach (var triangulate in triangulation.ToList())
            {
                if (Node_Triangle.SharesVertexPosition(superTriangle, triangulate))
                    triangulation.Remove(triangulate);
            }
            
            return triangulation;
        }

        static List<(Vector3, Vector3, Vector3)> _enforceConstraints(
            List<(Vector3, Vector3, Vector3)> triangulation, List<(Vector3, Vector3)> constraints)
        {
            foreach (var constraint in constraints)
            {
                var intersectingTriangles = triangulation
                    .Where(triangle => DoesConstraintIntersectTriangle(constraint, triangle))
                    .ToList();

                if (intersectingTriangles.Count <= 0) continue;
                
                var polygon = new List<Vector3>();
                
                foreach (var triangle in intersectingTriangles)
                {
                    if (!polygon.Contains(triangle.Item1)) polygon.Add(triangle.Item1);
                    if (!polygon.Contains(triangle.Item2)) polygon.Add(triangle.Item2);
                    if (!polygon.Contains(triangle.Item3)) polygon.Add(triangle.Item3);
                }
                
                foreach (var triangle in intersectingTriangles)
                {
                    triangulation.Remove(triangle);
                }
                
                var newTriangles = _triangulatePolygonWithConstraint(polygon, constraint);
                triangulation.AddRange(newTriangles);

            }

            return triangulation;
        }

        static List<(Vector3, Vector3, Vector3)> _triangulatePolygonWithConstraint(
            List<Vector3> polygon, (Vector3, Vector3) constraint)
        {
            // This is a simplified implementation - in a real scenario,
            // you would use a more sophisticated constrained Delaunay algorithm

            var result = new List<(Vector3, Vector3, Vector3)>();
            
            if (!polygon.Contains(constraint.Item1))
                polygon.Add(constraint.Item1);
            if (!polygon.Contains(constraint.Item2))
                polygon.Add(constraint.Item2);
            
            var ears = FindEars(polygon, constraint);

            while (polygon.Count > 3)
            {
                if (ears.Count == 0) break;

                var ear = ears[0];
                var prev = (ear - 1 + polygon.Count) % polygon.Count;
                var next = (ear + 1) % polygon.Count;

                result.Add((polygon[prev], polygon[ear], polygon[next]));

                polygon.RemoveAt(ear);
                ears = FindEars(polygon, constraint);
            }

            if (polygon.Count == 3)
            {
                result.Add((polygon[0], polygon[1], polygon[2]));
            }

            return result;
        }

        static List<int> FindEars(List<Vector3> polygon, (Vector3, Vector3) constraint)
        {
            var ears = new List<int>();

            for (int i = 0; i < polygon.Count; i++)
            {
                var prev = (i - 1 + polygon.Count) % polygon.Count;
                var next = (i + 1) % polygon.Count;

                var triangle = (polygon[prev], polygon[i], polygon[next]);
                
                var isEar = true;
                
                for (var j = 0; j < polygon.Count; j++)
                {
                    if (j == prev || j == i || j == next)
                        continue;

                    if (!Node_Triangle.IsPointInsideTriangle(polygon[j], triangle.Item1, triangle.Item2,
                            triangle.Item3)) continue;
                    
                    isEar = false;
                    break;
                }
                
                var containsConstraint =
                    (Vector3.Distance(polygon[i], constraint.Item1) < 0.0001f &&
                     Vector3.Distance(polygon[prev], constraint.Item2) < 0.0001f) ||
                    (Vector3.Distance(polygon[i], constraint.Item2) < 0.0001f &&
                     Vector3.Distance(polygon[prev], constraint.Item1) < 0.0001f) ||
                    (Vector3.Distance(polygon[next], constraint.Item1) < 0.0001f &&
                     Vector3.Distance(polygon[i], constraint.Item2) < 0.0001f) ||
                    (Vector3.Distance(polygon[next], constraint.Item2) < 0.0001f &&
                     Vector3.Distance(polygon[i], constraint.Item1) < 0.0001f);

                if (isEar && !containsConstraint)
                {
                    ears.Add(i);
                }
            }

            return ears;
        }

        static List<(Vector3, Vector3, Vector3)> _filterTrianglesInIntersection(
            List<(Vector3, Vector3, Vector3)> triangulation, List<(Vector3, Vector3)> boundaryEdges)
        {
            if (boundaryEdges.Count == 0)
                return triangulation;

            var result = new List<(Vector3, Vector3, Vector3)>();
            
            var boundaryPoints = new HashSet<Vector3>();
            foreach (var edge in boundaryEdges)
            {
                boundaryPoints.Add(edge.Item1);
                boundaryPoints.Add(edge.Item2);
            }
            
            foreach (var triangle in triangulation)
            {
                var centroid = (triangle.Item1 + triangle.Item2 + triangle.Item3) / 3;

                var allVerticesOnBoundary =
                    boundaryPoints.Contains(triangle.Item1) &&
                    boundaryPoints.Contains(triangle.Item2) &&
                    boundaryPoints.Contains(triangle.Item3);

                if (allVerticesOnBoundary || _isPointInPolygon(centroid, boundaryEdges))
                {
                    result.Add(triangle);
                }
            }

            return result;
        }

        static bool _isPointInPolygon(Vector3 point, List<(Vector3, Vector3)> edges)
        {
            if (edges.Count == 0)
                return false;
            
            var intersectionCount = 0;
            
            foreach (var edge in edges)
            {
                if ((edge.Item1.y > point.y) != (edge.Item2.y > point.y) &&
                    point.x < edge.Item1.x + (point.y - edge.Item1.y) / (edge.Item2.y - edge.Item1.y) *
                    (edge.Item2.x - edge.Item1.x))
                {
                    intersectionCount++;
                }
            }

            return intersectionCount % 2 == 1;
        }

        static bool _tryGetEdgeIntersection(
            Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, out Vector3 intersection)
        {
            intersection = Vector3.zero;
            
            var normal1 = Vector3.Cross(a2 - a1, b2 - b1);
            if (normal1.magnitude < 0.0001f)
                return false; // Parallel lines
            
            var d = -Vector3.Dot(normal1, a1);
            
            var dist1 = Vector3.Dot(normal1, b1) + d;
            var dist2 = Vector3.Dot(normal1, b2) + d;

            if ((dist1 > 0 && dist2 > 0) || (dist1 < 0 && dist2 < 0))
                return false; // No intersection

            var axis = 0;
            if (Mathf.Abs(normal1.y) > Mathf.Abs(normal1.x)) axis = 1;
            if (Mathf.Abs(normal1.z) > Mathf.Abs(normal1[axis])) axis = 2;

            var axis1 = (axis + 1) % 3;
            var axis2 = (axis + 2) % 3;
            
            var a1X = a1[axis1];
            var a1Y = a1[axis2];
            var a2X = a2[axis1];
            var a2Y = a2[axis2];
            var b1X = b1[axis1];
            var b1Y = b1[axis2];
            var b2X = b2[axis1];
            var b2Y = b2[axis2];

            var denominator = (b2Y - b1Y) * (a2X - a1X) - (b2X - b1X) * (a2Y - a1Y);
            if (Mathf.Abs(denominator) < 0.0001f)
                return false; // Parallel or colinear

            var ua = ((b2X - b1X) * (a1Y - b1Y) - (b2Y - b1Y) * (a1X - b1X)) / denominator;
            var ub = ((a2X - a1X) * (a1Y - b1Y) - (a2Y - a1Y) * (a1X - b1X)) / denominator;

            if (ua is < 0 or > 1 || ub is < 0 or > 1) return false;
            
            intersection = a1 + ua * (a2 - a1);
            return true;

        }

        static bool DoesConstraintIntersectTriangle(
            (Vector3, Vector3) constraint, (Vector3, Vector3, Vector3) triangle)
        {
            var edges = new[]
            {
                (triangle.Item1, triangle.Item2),
                (triangle.Item2, triangle.Item3),
                (triangle.Item3, triangle.Item1)
            };

            foreach (var edge in edges)
            {
                if (_tryGetEdgeIntersection(constraint.Item1, constraint.Item2, edge.Item1, edge.Item2, out _))
                    return true;
            }

            return false;
        }
        
        static List<Vector3> GetConvexHull(List<Vector3> points)
        {
            if (points.Count <= 3)
                return new List<Vector3>(points);
            
            var lowestPoint = points.OrderBy(p => p.y).ThenBy(p => p.x).First();
            
            var sortedPoints = points
                .Where(p => p != lowestPoint)
                .OrderBy(p => Mathf.Atan2(p.y - lowestPoint.y, p.x - lowestPoint.x))
                .ToList();

            sortedPoints.Insert(0, lowestPoint);
            
            var hull = new List<Vector3>
            {
                sortedPoints[0],
                sortedPoints[1]
            };

            for (var i = 2; i < sortedPoints.Count; i++)
            {
                while (hull.Count > 1 && !IsLeftTurn(hull[^2], hull[^1], sortedPoints[i]))
                {
                    hull.RemoveAt(hull.Count - 1);
                }

                hull.Add(sortedPoints[i]);
            }

            return hull;
        }
        
        static bool IsLeftTurn(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) > 0;
        }
    }
}