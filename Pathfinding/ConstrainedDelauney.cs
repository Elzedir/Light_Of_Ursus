using System.Collections.Generic;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pathfinding
{
    // public class ConstrainedDelauney
    // {
        
        //* With the terrain, constrained delauney is going to be better for terrain pathfinding.
        
        //The author of the report added a few optimizations to make the algorithm faster. For example, the report suggested that you should "rotate"
        //triangle (in the same way as when we found intersections with the constraints) to find if a constraint already exists in the triangulation.
        //But 700 lines of code is enough and it will make the tutorial more and more complicated, so I've ignored that optimization. If you think this
        //algortihm is too slow, you can add it.
        
    //     public static class ConstrainedDelaunay
    //     {
    //         public static List<Node_Triangle> GenerateTriangulation(List<Vector3> points, List<Vector3> constraints)
    //         {
    //             points.AddRange(constraints);
    //             
    //             List<Node_Triangle> delaunayTriangulation = Delaunay.TriangulateByFlippingEdges(points);
    //             
    //             var constrainedDelaunayTriangulation = _addConstraints(delaunayTriangulation, constraints);
    //
    //             return constrainedDelaunayTriangulation;
    //         }
    //
    //         static List<Node_Triangle> _addConstraints(List<Node_Triangle> triangulation, List<Vector3> constraints)
    //         {
    //             for (var i = 0; i < constraints.Count; i++)
    //             {
    //                 var firstConstraint = constraints[i];
    //                 var secondConstraint = constraints[Mathf.Clamp(i + 1, 0, constraints.Count)];
    //                 
    //                 if (_isEdgePartOfTriangulation(triangulation, firstConstraint, secondConstraint))
    //                     continue;
    //                 
    //                 var intersectingEdges = _findIntersectingEdges(triangulation, firstConstraint, secondConstraint);
    //                 
    //                 var newEdges = RemoveIntersectingEdges(firstConstraint, secondConstraint, intersectingEdges);
    //                 
    //                 _restoreDelaunayTriangulation(firstConstraint, secondConstraint, newEdges);
    //             }
    //             
    //             _removeSuperfluousTriangles(triangulation, constraints);
    //
    //             return triangulation;
    //         }
    //
    //         static List<Edge_Half> RemoveIntersectingEdges(Vector3 start, Vector3 end,
    //             Queue<Edge_Half> intersectingEdges)
    //         {
    //             var newEdges = new List<Edge_Half>();
    //
    //             var safety = 0;
    //             
    //             while (intersectingEdges.Count > 0)
    //             {
    //                 safety += 1;
    //
    //                 if (safety > 10000)
    //                 {
    //                     Debug.Log("Stuck in infinite loop when fixing constrained edges");
    //
    //                     break;
    //                 }
    //                 
    //                 var edge = intersectingEdges.Dequeue();
    //                 
    //                 var firstEdge = edge.Vertex.Position;
    //                 var secondEdge = edge.Previous.Vertex.Position;
    //                 var thirdEdge = edge.Next.Vertex.Position;
    //                 var oppositeEdge = edge.Opposite.Next.Vertex.Position;
    //                 
    //                 if (!Geometry.IsQuadrilateralConvex(firstEdge.XZ(), secondEdge.XZ(), thirdEdge.XZ(), oppositeEdge.XZ()))
    //                 {
    //                     intersectingEdges.Enqueue(edge);
    //                 }
    //                 else
    //                 {
    //                     Delaunay.FlipEdge(edge);
    //                     
    //                     var newFirstDiagonalEdge = edge.Vertex.Position;
    //                     var newSecondDiagonalEdge = edge.Previous.Vertex.Position;
    //                     
    //                     if (_isEdgeCrossingEdge(start, end, newFirstDiagonalEdge, newSecondDiagonalEdge))
    //                     {
    //                         intersectingEdges.Enqueue(edge);
    //                     }
    //                     else
    //                     {
    //                         newEdges.Add(edge);
    //                     }
    //                 }
    //             }
    //
    //             return newEdges;
    //         }
    //
    //         static void _restoreDelaunayTriangulation(Vector3 v_i, Vector3 v_j, List<Edge_Half> newEdges)
    //         {
    //             var iteration = 0;
    //             var flippedEdges = 0;
    //             
    //             while (true)
    //             {
    //                 iteration += 1;
    //
    //                 if (iteration > 100000) break;
    //                 var hasFlippedEdge = false;
    //                 
    //                 for (var j = 0; j < newEdges.Count; j++)
    //                 {
    //                     var edge = newEdges[j];
    //                     
    //                     var firstEdge = edge.Vertex.Position;
    //                     var secondEdge = edge.Previous.Vertex.Position;
    //
    //                     if ((firstEdge == v_i && secondEdge == v_j) || (secondEdge == v_i && firstEdge == v_j))
    //                     {
    //                         continue;
    //                     }
    //                     
    //                     var thirdEdgePosition = edge.Next.Vertex.Position;
    //                     var oppositeEdgePosition = edge.Opposite.Next.Vertex.Position;
    //
    //                     float circleTestValue = Geometry.IsPointInsideOutsideOrOnCircle(firstEdge.XZ(), secondEdge.XZ(),
    //                         thirdEdgePosition.XZ(), oppositeEdgePosition.XZ());
    //
    //                     if (circleTestValue < 0f)
    //                     {
    //                         if (Geometry.IsQuadrilateralConvex(firstEdge.XZ(), secondEdge.XZ(), thirdEdgePosition.XZ(),
    //                                 oppositeEdgePosition.XZ()))
    //                         {
    //                             if (Geometry.IsPointInsideOutsideOrOnCircle(oppositeEdgePosition.XZ(), secondEdge.XZ(),
    //                                     thirdEdgePosition.XZ(), firstEdge.XZ()) <= circleTestValue)
    //                             {
    //                                 continue;
    //                             }
    //                             
    //                             hasFlippedEdge = true;
    //
    //                             Delaunay.FlipEdge(edge);
    //
    //                             flippedEdges += 1;
    //                         }
    //                     }
    //                 }
    //
    //                 if (hasFlippedEdge) continue;
    //
    //                 break;
    //             }
    //         }
    //
    //         static void _removeSuperfluousTriangles(List<Node_Triangle> triangulation, List<Vector3> constraints)
    //         {
    //             if (constraints.Count < 3) return;
    //
    //             Node_Triangle borderTriangle = null;
    //
    //             var firstConstraint = constraints[0];
    //             var secondConstraint = constraints[1];
    //
    //             foreach (var triangle in triangulation)
    //             {
    //                 var firstEdge = triangle.EdgeHalf;
    //                 var secondEdge = firstEdge.Next;
    //                 var thirdEdge = secondEdge.Next;
    //                 
    //                 if (firstEdge.Vertex.Position == secondConstraint && firstEdge.Previous.Vertex.Position == firstConstraint)
    //                 {
    //                     borderTriangle = triangle;
    //
    //                     break;
    //                 }
    //
    //                 if (secondEdge.Vertex.Position == secondConstraint && secondEdge.Previous.Vertex.Position == firstConstraint)
    //                 {
    //                     borderTriangle = triangle;
    //
    //                     break;
    //                 }
    //
    //                 if (thirdEdge.Vertex.Position != secondConstraint ||
    //                     thirdEdge.Previous.Vertex.Position != firstConstraint) continue;
    //                 
    //                 borderTriangle = triangle;
    //
    //                 break;
    //             }
    //
    //             if (borderTriangle == null)
    //             {
    //                 return;
    //             }
    //             
    //             var trianglesToBeDeleted = new List<Node_Triangle>();
    //             var neighborsToCheck = new Queue<Node_Triangle>();
    //             
    //             neighborsToCheck.Enqueue(borderTriangle);
    //
    //             var iteration = 0;
    //
    //             while (true)
    //             {
    //                 iteration += 1;
    //
    //                 if (iteration > 10000) break;
    //                 
    //                 if (neighborsToCheck.Count == 0) break;
    //                 
    //                 var triangle = neighborsToCheck.Dequeue();
    //
    //                 trianglesToBeDeleted.Add(triangle);
    //
    //                 var firstEdge = triangle.EdgeHalf;
    //                 var secondEdge = firstEdge.Next;
    //                 var thirdEdge = secondEdge.Next;
    //
    //                 if (
    //                     firstEdge.Opposite != null &&
    //                     !trianglesToBeDeleted.Contains(firstEdge.Opposite.Triangle) &&
    //                     !neighborsToCheck.Contains(firstEdge.Opposite.Triangle) &&
    //                     !_isAnEdgeAConstraint(firstEdge.Vertex.Position, firstEdge.Previous.Vertex.Position, constraints))
    //                 {
    //                     neighborsToCheck.Enqueue(firstEdge.Opposite.Triangle);
    //                 }
    //
    //                 if (
    //                     secondEdge.Opposite != null &&
    //                     !trianglesToBeDeleted.Contains(secondEdge.Opposite.Triangle) &&
    //                     !neighborsToCheck.Contains(secondEdge.Opposite.Triangle) &&
    //                     !_isAnEdgeAConstraint(secondEdge.Vertex.Position, secondEdge.Previous.Vertex.Position, constraints))
    //                 {
    //                     neighborsToCheck.Enqueue(secondEdge.Opposite.Triangle);
    //                 }
    //
    //                 if (
    //                     thirdEdge.Opposite != null &&
    //                     !trianglesToBeDeleted.Contains(thirdEdge.Opposite.Triangle) &&
    //                     !neighborsToCheck.Contains(thirdEdge.Opposite.Triangle) &&
    //                     !_isAnEdgeAConstraint(thirdEdge.Vertex.Position, thirdEdge.Previous.Vertex.Position, constraints))
    //                 {
    //                     neighborsToCheck.Enqueue(thirdEdge.Opposite.Triangle);
    //                 }
    //             }
    //             
    //             foreach (var triangleToDelete in trianglesToBeDeleted)
    //             {
    //                 triangulation.Remove(triangleToDelete);
    //                 
    //                 var firstEdge = triangleToDelete.EdgeHalf;
    //                 var secondEdge = firstEdge.Next;
    //                 var thirdEdge = secondEdge.Next;
    //
    //                 if (firstEdge.Opposite != null)
    //                 {
    //                     firstEdge.Opposite.Opposite = null;
    //                 }
    //
    //                 if (secondEdge.Opposite != null)
    //                 {
    //                     secondEdge.Opposite.Opposite = null;
    //                 }
    //
    //                 if (thirdEdge.Opposite != null)
    //                 {
    //                     thirdEdge.Opposite.Opposite = null;
    //                 }
    //             }
    //         }
    //
    //         static bool _isAnEdgeAConstraint(Vector3 p1, Vector3 p2, List<Vector3> constraints)
    //         {
    //             for (var i = 0; i < constraints.Count; i++)
    //             {
    //                 var firstConstraint = constraints[i];
    //                 var secondConstraint = constraints[Mathf.Clamp(i + 1, 0,constraints.Count)];
    //
    //                 if ((p1 == firstConstraint && p2 == secondConstraint) || (p2 == firstConstraint && p1 == secondConstraint))
    //                 {
    //                     return true;
    //                 }
    //             }
    //
    //             return false;
    //         }
    //
    //         static List<Edge_Half> _findIntersectingEdges(List<Node_Triangle> triangulation, Vector3 p1, Vector3 p2)
    //         {
    //             var intersectingEdges = new List<Edge_Half>();
    //             Node_Triangle triangle = null;
    //
    //             foreach (var tri in triangulation)
    //             {
    //                 var firstEdge = tri.EdgeHalf;
    //                 var secondEdge = firstEdge.Next;
    //                 var thirdEdge = secondEdge.Next;
    //                 
    //                 if (firstEdge.Vertex.Position != p1 && secondEdge.Vertex.Position != p1 &&
    //                     thirdEdge.Vertex.Position != p1) continue;
    //                 
    //                 triangle = tri;
    //
    //                 break;
    //             }
    //
    //             if (triangle == null) return intersectingEdges;
    //             
    //             var iteration = 0;
    //             Edge_Half last = null;
    //             var startTriangle = triangle;
    //             var restart = false;
    //
    //             while (true)
    //             {
    //                 iteration += 1;
    //
    //                 if (iteration > 10000) break;
    //                 
    //                 var firstEdge = triangle.EdgeHalf;
    //                 var secondEdge = firstEdge.Next;
    //                 var thirdEdge = secondEdge.Next;
    //                 
    //                 Edge_Half edge_Doesnt_Include_P1;
    //
    //                 if (firstEdge.Vertex.Position != p1 && firstEdge.Previous.Vertex.Position != p1)
    //                 {
    //                     edge_Doesnt_Include_P1 = firstEdge;
    //                 }
    //                 else if (secondEdge.Vertex.Position != p1 && secondEdge.Previous.Vertex.Position != p1)
    //                 {
    //                     edge_Doesnt_Include_P1 = secondEdge;
    //                 }
    //                 else
    //                 {
    //                     edge_Doesnt_Include_P1 = thirdEdge;
    //                 }
    //                 
    //                 if (_isEdgeCrossingEdge(
    //                         edge_Doesnt_Include_P1.Vertex.Position, 
    //                         edge_Doesnt_Include_P1.Previous.Vertex.Position, 
    //                         p1, p2)) 
    //                     break;
    //                 
    //                 var includes_P1 = new List<Edge_Half>();
    //
    //                 if (firstEdge != edge_Doesnt_Include_P1)
    //                 {
    //                     includes_P1.Add(firstEdge);
    //                 }
    //
    //                 if (secondEdge != edge_Doesnt_Include_P1)
    //                 {
    //                     includes_P1.Add(secondEdge);
    //                 }
    //
    //                 if (thirdEdge != edge_Doesnt_Include_P1)
    //                 {
    //                     includes_P1.Add(thirdEdge);
    //                 }
    //                 
    //                 if (last == null)
    //                 {
    //                     last = includes_P1[0];
    //
    //                     if (last.Opposite == null || restart)
    //                     {
    //                         last = includes_P1[1];
    //                     }
    //                     
    //                     triangle = last.Opposite.Triangle;
    //                 }
    //                 else
    //                 {
    //                     last = includes_P1[0].Opposite != last ? includes_P1[0] : includes_P1[1];
    //
    //                     if (last.Opposite == null)
    //                     {
    //                         restart = true;
    //
    //                         triangle = startTriangle;
    //
    //                         last = null;
    //                     }
    //                     else
    //                     {
    //                         triangle = last.Opposite.Triangle;
    //                     }
    //                 }
    //             }
    //             
    //             var iteration2 = 0;
    //             last = null;
    //
    //             while (true)
    //             {
    //                 iteration2 += 1;
    //
    //                 if (iteration2 > 10000) break;
    //                 
    //                 var firstEdge = triangle.EdgeHalf;
    //                 var secondEdge = firstEdge.Next;
    //                 var thirdEdge = secondEdge.Next;
    //                 
    //                 if (firstEdge.Vertex.Position == p2 
    //                     || secondEdge.Vertex.Position == p2 
    //                     || thirdEdge.Vertex.Position == p2)
    //                     break;
    //                 
    //                 if (firstEdge.Opposite != last &&
    //                     _isEdgeCrossingEdge(firstEdge.Vertex.Position, firstEdge.Previous.Vertex.Position, p1, p2))
    //                 {
    //                     last = firstEdge;
    //                 }
    //                 else if (secondEdge.Opposite != last &&
    //                          _isEdgeCrossingEdge(secondEdge.Vertex.Position, secondEdge.Previous.Vertex.Position, p1, p2))
    //                 {
    //                     last = secondEdge;
    //                 }
    //                 else
    //                 {
    //                     last = thirdEdge;
    //                 }
    //                 
    //                 triangle = last.Opposite.Triangle;
    //                 
    //                 intersectingEdges.Add(last);
    //             }
    //
    //             return intersectingEdges;
    //         }
    //
    //         static void _tryAddEdgeToIntersectingEdges(Edge_Half edge, Vector3 point_1, Vector3 point_2,
    //             List<Edge_Half> intersectingEdges)
    //         {
    //             var start = edge.Vertex.Position;
    //             var end = edge.Previous.Vertex.Position;
    //
    //             if (!_isEdgeCrossingEdge(start, end, point_1, point_2)) return;
    //             
    //             foreach (var triangle in intersectingEdges)
    //             {
    //                 if (triangle == edge || triangle.Opposite == edge) return;
    //             }
    //                 
    //             intersectingEdges.Add(edge);
    //         }
    //
    //         static bool _isEdgeCrossingEdge(Vector3 start_1, Vector3 end_1, Vector3 start_2, Vector3 end_2)
    //         {
    //             if (start_1 == start_2 || start_1 == end_2 || end_1 == start_2 || end_1 == end_2)
    //                 return false;
    //             
    //             return Intersections.AreLinesIntersecting(start_1.XZ(), end_1.XZ(), start_2.XZ(), end_2.XZ(), false);
    //         }
    //
    //         static bool _isEdgePartOfTriangulation(List<Node_Triangle> triangulation, Vector3 p1, Vector3 p2)
    //         {
    //             foreach (var t in triangulation)
    //             {
    //                 var vertex_1 = t.Vertices[0].Position;
    //                 var vertex_2 = t.Vertices[1].Position;
    //                 var vertex_3 = t.Vertices[2].Position;
    //                 
    //                 if ((vertex_1 == p1 && vertex_2 == p2) || (vertex_1 == p2 && vertex_2 == p1))
    //                 {
    //                     return true;
    //                 }
    //
    //                 if ((vertex_2 == p1 && vertex_3 == p2) || (vertex_2 == p2 && vertex_3 == p1))
    //                 {
    //                     return true;
    //                 }
    //
    //                 if ((vertex_3 == p1 && vertex_1 == p2) || (vertex_3 == p2 && vertex_1 == p1))
    //                 {
    //                     return true;
    //                 }
    //             }
    //
    //             return false;
    //         }
    //     }
    // }
}