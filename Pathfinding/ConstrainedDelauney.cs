// using System.Collections.Generic;
// using FMOD;
// using UnityEngine;
// using Debug = UnityEngine.Debug;
//
// namespace Pathfinding
// {
//     public static class ConstrainedDelaunaySloan
//     {
//         private static HalfEdgeData2 AddConstraints(HalfEdgeData2 triangleData, List<MyVector2> constraints, bool shouldRemoveTriangles, System.Diagnostics.Stopwatch timer = null)
//         {
//             if (constraints == null)
//             {
//                 return triangleData;
//             }
//
//
//             //Get a list with all edges
//             //This is faster than first searching for unique edges
//             //The report suggest we should do a triangle walk, but it will not work if the mesh has holes
//             //The mesh has holes because we remove triangles while adding constraints one-by-one
//             //so maybe better to remove triangles after we added all constraints...
//             HashSet<HalfEdge2> edges = triangleData.edges;
//          
//
//             //The steps numbering is from the report
//             //Step 1. Loop over each constrained edge. For each of these edges, do steps 2-4 
//             for (int i = 0; i < constraints.Count; i++)
//             {
//                 //Let each constrained edge be defined by the vertices:
//                 MyVector2 c_p1 = constraints[i];
//                 MyVector2 c_p2 = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];
//
//                 //Check if this constraint already exists in the triangulation, 
//                 //if so we are happy and dont need to worry about this edge
//                 //timer.Start();
//                 if (IsEdgeInListOfEdges(edges, c_p1, c_p2))
//                 {
//                     continue;
//                 }
//                 //timer.Stop();
//
//                 //Step 2. Find all edges in the current triangulation that intersects with this constraint
//                 //Is returning unique edges only, so not one edge going in the opposite direction
//                 //timer.Start();
//                 Queue<HalfEdge2> intersectingEdges = FindIntersectingEdges_BruteForce(edges, c_p1, c_p2);
//                 //timer.Stop();
//
//                 //Debug.Log("Intersecting edges: " + intersectingEdges.Count);
//
//                 //Step 3. Remove intersecting edges by flipping triangles
//                 //This takes 0 seconds so is not bottleneck
//                 //timer.Start();
//                 List<HalfEdge2> newEdges = RemoveIntersectingEdges(c_p1, c_p2, intersectingEdges);
//                 //timer.Stop();
//
//                 //Step 4. Try to restore delaunay triangulation 
//                 //Because we have constraints we will never get a delaunay triangulation
//                 //This takes 0 seconds so is not bottleneck
//                 //timer.Start();
//                 RestoreDelaunayTriangulation(c_p1, c_p2, newEdges);
//                 //timer.Stop();
//             }
//
//             //Step 5. Remove superfluous triangles, such as the triangles "inside" the constraints  
//             if (shouldRemoveTriangles)
//             {
//                 RemoveSuperfluousTriangles(triangleData, constraints);
//             }
//
//             return triangleData;
//         }
//
//         static List<HalfEdge2> RemoveIntersectingEdges(MyVector2 v_i, MyVector2 v_j, Queue<HalfEdge2> intersectingEdges)
//         {
//             var newEdges = new List<HalfEdge2>();
//
//             var safety = 0;
//             
//             while (intersectingEdges.Count > 0)
//             {
//                 safety += 1;
//
//                 if (safety > 100000)
//                 {
//                     Debug.Log("Stuck in infinite loop when fixing constrained edges");
//
//                     break;
//                 }
//
//                 //Step 3.1. Remove an edge from the list of edges that intersects the constrained edge
//                 HalfEdge2 e = intersectingEdges.Dequeue();
//
//                 //The vertices belonging to the two triangles
//                 MyVector2 v_k = e.v.position;
//                 MyVector2 v_l = e.prevEdge.v.position;
//                 MyVector2 v_3rd = e.nextEdge.v.position;
//                 //The vertex belonging to the opposite triangle and isn't shared by the current edge
//                 MyVector2 v_opposite_pos = e.oppositeEdge.nextEdge.v.position;
//
//                 //Step 3.2. If the two triangles don't form a convex quadtrilateral
//                 //place the edge back on the list of intersecting edges (because this edge cant be flipped) 
//                 //and go to step 3.1
//                 if (!_Geometry.IsQuadrilateralConvex(v_k, v_l, v_3rd, v_opposite_pos))
//                 {
//                     intersectingEdges.Enqueue(e);
//
//                     continue;
//                 }
//                 else
//                 {
//                     //Flip the edge like we did when we created the delaunay triangulation
//                     HalfEdgeHelpMethods.FlipTriangleEdge(e);
//
//                     //The new diagonal is defined by the vertices
//                     MyVector2 v_m = e.v.position;
//                     MyVector2 v_n = e.prevEdge.v.position;
//
//                     //If this new diagonal intersects with the constrained edge, add it to the list of intersecting edges
//                     if (IsEdgeCrossingEdge(v_i, v_j, v_m, v_n))
//                     {
//                         intersectingEdges.Enqueue(e);
//                     }
//                     //Place it in the list of newly created edges
//                     else
//                     {
//                         newEdges.Add(e);
//                     }
//                 }
//             }
//
//             return newEdges;
//         }
//
//
//
//         //
//         // Try to restore the delaunay triangulation by flipping newly created edges
//         //
//
//         //This process is similar to when we created the original delaunay triangulation
//         //This step can maybe be skipped if you just want a triangulation and Ive noticed its often not flipping any triangles
//         private static void RestoreDelaunayTriangulation(MyVector2 c_p1, MyVector2 c_p2, List<HalfEdge2> newEdges)
//         {
//             int safety = 0;
//
//             int flippedEdges = 0;
//
//             //Repeat 4.1 - 4.3 until no further swaps take place
//             while (true)
//             {
//                 safety += 1;
//
//                 if (safety > 100000)
//                 {
//                     Debug.Log("Stuck in endless loop when delaunay after fixing constrained edges");
//
//                     break;
//                 }
//
//                 bool hasFlippedEdge = false;
//
//                 //Step 4.1. Loop over each edge in the list of newly created edges
//                 for (int j = 0; j < newEdges.Count; j++)
//                 {
//                     HalfEdge2 e = newEdges[j];
//
//                     //Step 4.2. Let the newly created edge be defined by the vertices
//                     MyVector2 v_k = e.v.position;
//                     MyVector2 v_l = e.prevEdge.v.position;
//
//                     //If this edge is equal to the constrained edge, then skip to step 4.1
//                     //because we are not allowed to flip the constrained edge
//                     if ((v_k.Equals(c_p1) && v_l.Equals(c_p2)) || (v_l.Equals(c_p1) && v_k.Equals(c_p2)))
//                     {
//                         continue;
//                     }
//
//                     //Step 4.3. If the two triangles that share edge v_k and v_l don't satisfy the delaunay criterion,
//                     //so that a vertex of one of the triangles is inside the circumcircle of the other triangle, flip the edge
//                     //The third vertex of the triangle belonging to this edge
//                     MyVector2 v_third_pos = e.nextEdge.v.position;
//                     //The vertice belonging to the triangle on the opposite side of the edge and this vertex is not a part of the edge
//                     MyVector2 v_opposite_pos = e.oppositeEdge.nextEdge.v.position;
//
//                     //Test if we should flip this edge
//                     if (ShouldFlipEdge(v_l, v_k, v_third_pos, v_opposite_pos))
//                     {
//                         //Flip the edge
//                         hasFlippedEdge = true;
//
//                         FlipTriangleEdge(e);
//
//                         flippedEdges += 1;
//                     }
//                 }
//
//                 //We have searched through all edges and havent found an edge to flip, so we cant improve anymore
//                 if (!hasFlippedEdge)
//                 {
//                     //Debug.Log("Found a constrained delaunay triangulation in " + flippedEdges + " flips");
//
//                     break;
//                 }
//             }
//         }
//
//         public static bool ShouldFlipEdge(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 d)
//         {
//             bool shouldFlipEdge = false;
//
//             //Use the circle test to test if we need to flip this edge
//             //We should flip if d is inside a circle formed by a, b, c
//             IntersectionCases intersectionCases = _Intersections.PointCircle(a, b, c, d);
//
//             if (intersectionCases == IntersectionCases.IsInside)
//             {
//                 //Are these the two triangles forming a convex quadrilateral? Otherwise the edge cant be flipped
//                 if (_Geometry.IsQuadrilateralConvex(a, b, c, d))
//                 {
//                     //If the new triangle after a flip is not better, then dont flip
//                     //This will also stop the algorithm from ending up in an endless loop
//                     IntersectionCases intersectionCases2 = _Intersections.PointCircle(b, c, d, a);
//
//                     if (intersectionCases2 == IntersectionCases.IsOnEdge || intersectionCases2 == IntersectionCases.IsInside)
//                     {
//                         shouldFlipEdge = false;
//                     }
//                     else
//                     {
//                         shouldFlipEdge = true;
//                     }
//                 }
//             }
//
//             return shouldFlipEdge;
//         }
//         
//         public static void FlipTriangleEdge(HalfEdge2 e)
//         {
//             var e_2 = e.nextEdge;
//             var e_3 = e.prevEdge;
//             var e_4 = e.oppositeEdge;
//             var e_5 = e_4.nextEdge;
//             var e_6 = e_4.prevEdge;
//             
//             var aPos = e.v.position;
//             var bPos = e_2.v.position;
//             var cPos = e_3.v.position;
//             var dPos = e_5.v.position;
//             
//             var a_Old = e.v;
//             var b_Old = e.nextEdge.v;
//             var c_Old = e.prevEdge.v;
//             var a_Opposite_Old = e_4.prevEdge.v;
//             var c_Opposite_Old = e_4.v;
//             var d_Old = e_4.nextEdge.v;
//
//             //Triangle 1: b-c-d
//             var b = b_Old;
//             var c = c_Old;
//             var d = d_Old;
//             //Triangle 1: b-d-a
//             var b_opposite = a_Opposite_Old;
//             b_opposite.position = bPos;
//             var d_opposite = c_Opposite_Old;
//             d_opposite.position = dPos;
//             var a = a_Old;
//             
//             e.nextEdge = e_3;
//             e.prevEdge = e_5;
//
//             e_2.nextEdge = e_4;
//             e_2.prevEdge = e_6;
//
//             e_3.nextEdge = e_5;
//             e_3.prevEdge = e;
//
//             e_4.nextEdge = e_6;
//             e_4.prevEdge = e_2;
//
//             e_5.nextEdge = e;
//             e_5.prevEdge = e_3;
//
//             e_6.nextEdge = e_2;
//             e_6.prevEdge = e_4;
//
//             //Half-edge - vertex connection
//             e.v = b;
//             e_2.v = b_opposite;
//             e_3.v = c;
//             e_4.v = d_opposite;
//             e_5.v = d;
//             e_6.v = a;
//
//             //Half-edge - face connection
//             HalfEdgeFace2 f1 = e.face;
//             HalfEdgeFace2 f2 = e_4.face;
//
//             e.face = f1;
//             e_3.face = f1;
//             e_5.face = f1;
//
//             e_2.face = f2;
//             e_4.face = f2;
//             e_6.face = f2;
//
//             //Face - half-edge connection
//             f1.edge = e_3;
//             f2.edge = e_4;
//
//             //Vertices connection, which should have a reference to a half-edge going away from the vertex
//             //Triangle 1: b-c-d
//             b.edge = e_3;
//             c.edge = e_5;
//             d.edge = e;
//             //Triangle 1: b-d-a
//             b_opposite.edge = e_4;
//             d_opposite.edge = e_6;
//             a.edge = e_2;
//
//             //Opposite-edges are not changing!
//             //And neither are we adding, removing data so we dont need to update the lists with all data
//         }
//
//
//         //
//         // Find edges that intersect with a constraint
//         //
//
//         //Method 1. Brute force by testing all unique edges
//         //Find all edges of the current triangulation that intersects with the constraint edge between p1 and p2
//         private static Queue<HalfEdge2> FindIntersectingEdges_BruteForce(HashSet<HalfEdge2> edges, MyVector2 c_p1, MyVector2 c_p2)
//         {
//             //Should be in a queue because we will later plop the first in the queue and add edges in the back of the queue 
//             Queue<HalfEdge2> intersectingEdges = new Queue<HalfEdge2>();
//
//             //We also need to make sure that we are only adding unique edges to the queue
//             //In the half-edge data structure we have an edge going in the opposite direction
//             //and we only need to add an edge going in one direction
//             HashSet<Edge2> edgesInQueue = new HashSet<Edge2>();
//
//             //Loop through all edges and see if they are intersecting with the constrained edge
//             foreach (HalfEdge2 e in edges)
//             {
//                 //The position the edge is going to
//                 MyVector2 e_p2 = e.v.position;
//                 //The position the edge is coming from
//                 MyVector2 e_p1 = e.prevEdge.v.position;
//
//                 //Has this edge been added, but in the opposite direction?
//                 if (edgesInQueue.Contains(new Edge2(e_p2, e_p1)))
//                 {
//                     continue;
//                 }
//
//                 //Is this edge intersecting with the constraint?
//                 if (IsEdgeCrossingEdge(e_p1, e_p2, c_p1, c_p2))
//                 {
//                     //If so add it to the queue of edges
//                     intersectingEdges.Enqueue(e);
//
//                     edgesInQueue.Add(new Edge2(e_p1, e_p2));
//                 }
//             }
//
//             return intersectingEdges;
//         }
//
//
//
//         //Method 2. Triangulation walk
//         //This assumes there are no holes in the mesh
//         //And that we have a super-triangle around the triangulation
//         private static void FindIntersectingEdges_TriangleWalk(HalfEdgeData2 triangleData, MyVector2 c_p1, MyVector2 c_p2, List<HalfEdge2> intersectingEdges)
//         {
//             //Step 1. Begin at a triangle connected to the constraint edges's vertex c_p1
//             HalfEdgeFace2 f = null;
//
//             foreach (HalfEdgeFace2 testFace in triangleData.faces)
//             {
//                 //The edges the triangle consists of
//                 HalfEdge2 e1 = testFace.edge;
//                 HalfEdge2 e2 = e1.nextEdge;
//                 HalfEdge2 e3 = e2.nextEdge;
//
//                 //Does one of these edges include the first vertex in the constraint edge
//                 if (e1.v.position.Equals(c_p1) || e2.v.position.Equals(c_p1) || e3.v.position.Equals(c_p1))
//                 {
//                     f = testFace;
//
//                     break;
//                 }
//             }
//
//
//             
//             //Step2. Walk around p1 until we find a triangle with an edge that intersects with the edge p1-p2
//            
//
//             //Step3. March from one triangle to the next in the general direction of p2
//            
//         }
//
//
//
//         //
//         // Edge stuff
//         //
//
//         //Are two edges the same edge?
//         private static bool AreTwoEdgesTheSame(MyVector2 e1_p1, MyVector2 e1_p2, MyVector2 e2_p1, MyVector2 e2_p2)
//         {
//             //Is e1_p1 part of this constraint?
//             if ((e1_p1.Equals(e2_p1) || e1_p1.Equals(e2_p2)))
//             {
//                 //Is e1_p2 part of this constraint?
//                 if ((e1_p2.Equals(e2_p1) || e1_p2.Equals(e2_p2)))
//                 {
//                     return true;
//                 }
//             }
//
//             return false;
//         }
//
//
//
//         //Is an edge (between p1 and p2) in a list with edges
//         private static bool IsEdgeInListOfEdges(HashSet<HalfEdge2> edges, MyVector2 p1, MyVector2 p2)
//         {
//             foreach (HalfEdge2 e in edges)
//             {
//                 //The vertices positions of the current triangle
//                 MyVector2 e_p2 = e.v.position;
//                 MyVector2 e_p1 = e.prevEdge.v.position;
//
//                 //Check if edge has the same coordinates as the constrained edge
//                 //We have no idea about direction so we have to check both directions
//                 //This is fast because we only need to test one coordinate and if that 
//                 //coordinate doesn't match the edges can't be the same
//                 //We can't use a dictionary because we flip edges constantly so it would have to change?
//                 if (AreTwoEdgesTheSame(p1, p2, e_p1, e_p2))
//                 {
//                     return true;
//                 }
//             }
//
//             return false;
//         }
//         
//         //Is an edge crossing another edge? 
//         private static bool IsEdgeCrossingEdge(MyVector2 e1_p1, MyVector2 e1_p2, MyVector2 e2_p1, MyVector2 e2_p2)
//         {
//             //We will here run into floating point precision issues so we have to be careful
//             //To solve that you can first check the end points 
//             //and modify the line-line intersection algorithm to include a small epsilon
//
//             //First check if the edges are sharing a point, if so they are not crossing
//             if (e1_p1.Equals(e2_p1) || e1_p1.Equals(e2_p2) || e1_p2.Equals(e2_p1) || e1_p2.Equals(e2_p2))
//             {
//                 return false;
//             }
//
//             //Then check if the lines are intersecting
//             if (!_Intersections.LineLine(new Edge2(e1_p1, e1_p2), new Edge2(e2_p1, e2_p2), includeEndPoints: false))
//             {
//                 return false;
//             }
//
//             return true;
//         }
//     }
//     
//     //A collection of classes that implements the Half-Edge Data Structure
//     //From https://www.openmesh.org/media/Documentations/OpenMesh-6.3-Documentation/a00010.html
//
//     //2D space
//     public class HalfEdgeData2
//     {
//         public HashSet<HalfEdgeVertex2> vertices;
//
//         public HashSet<HalfEdgeFace2> faces;
//
//         public HashSet<HalfEdge2> edges;
//
//
//
//         public HalfEdgeData2()
//         {
//             this.vertices = new HashSet<HalfEdgeVertex2>();
//
//             this.faces = new HashSet<HalfEdgeFace2>();
//
//             this.edges = new HashSet<HalfEdge2>();
//         }
//
//
//
//         //Get a list with unique edges
//         //Currently we have two half-edges for each edge, making it time consuming
//         //So this method is not always needed, but can be useful
//         //But be careful because it takes time to generate this list as well, so measure that the algorithm is faster by using this list
//         public HashSet<HalfEdge2> GetUniqueEdges()
//         {
//             HashSet<HalfEdge2> uniqueEdges = new HashSet<HalfEdge2>();
//
//             foreach (HalfEdge2 e in edges)
//             {
//                 MyVector2 p1 = e.v.position;
//                 MyVector2 p2 = e.prevEdge.v.position;
//
//                 bool isInList = false;
//
//                 //TODO: Put these in a lookup dictionary to improve performance
//                 foreach (HalfEdge2 eUnique in uniqueEdges)
//                 {
//                     MyVector2 p1_test = eUnique.v.position;
//                     MyVector2 p2_test = eUnique.prevEdge.v.position;
//
//                     if ((p1.Equals(p1_test) && p2.Equals(p2_test)) || (p2.Equals(p1_test) && p1.Equals(p2_test)))
//                     {
//                         isInList = true;
//
//                         break;
//                     }
//                 }
//
//                 if (!isInList)
//                 {
//                     uniqueEdges.Add(e);
//                 }
//             }
//
//             return uniqueEdges;
//         }
//     }
//
//
//
//     //A position
//     public class HalfEdgeVertex2
//     {
//         //The position of the vertex
//         public MyVector2 position;
//
//         //Each vertex references an half-edge that starts at this point
//         //Might seem strange because each halfEdge references a vertex the edge is going to?
//         public HalfEdge2 edge;
//
//
//
//         public HalfEdgeVertex2(MyVector2 position)
//         {
//             this.position = position;
//         }
//     }
//
//
//
//     //This face could be a triangle or whatever we need
//     public class HalfEdgeFace2
//     {
//         //Each face references one of the halfedges bounding it
//         //If you need the vertices, you can use this edge
//         public HalfEdge2 edge;
//
//
//
//         public HalfEdgeFace2(HalfEdge2 edge)
//         {
//             this.edge = edge;
//         }
//     }
//
//
//
//     //An edge going in a direction
//     public class HalfEdge2
//     {
//         //The vertex it points to
//         public HalfEdgeVertex2 v;
//
//         //The face it belongs to
//         public HalfEdgeFace2 face;
//
//         //The next half-edge inside the face (ordered clockwise)
//         //The document says counter-clockwise but clockwise is easier because that's how Unity is displaying triangles
//         public HalfEdge2 nextEdge;
//
//         //The opposite half-edge belonging to the neighbor
//         public HalfEdge2 oppositeEdge;
//
//         //(optionally) the previous halfedge in the face
//         //If we assume the face is closed, then we could identify this edge by walking forward
//         //until we reach it
//         public HalfEdge2 prevEdge;
//
//
//
//         public HalfEdge2(HalfEdgeVertex2 v)
//         {
//             this.v = v;
//         }
//     }
//     
//     //Unity loves to automatically cast beween Vector2 and Vector3
//     //Because theres no way to stop it, its better to use a custom struct 
//     [System.Serializable]
//     public struct MyVector2
//     {
//         public float x;
//         public float y;
//
//         public MyVector2(float x, float y)
//         {
//             this.x = x;
//             this.y = y;
//         }
//
//
//
//         //
//         // To make vector operations easier
//         //
//
//         //Test if this vector is approximately the same as another vector
//         public bool Equals(MyVector2 other)
//         {
//             //Using Mathf.Approximately() is not accurate enough
//             //Using Mathf.Abs is slow because Abs involves a root
//
//             float xDiff = this.x - other.x;
//             float yDiff = this.y - other.y;
//
//             float e = MathUtility.EPSILON;
//
//             //If all of the differences are around 0
//             if (
//                 xDiff < e && xDiff > -e && 
//                 yDiff < e && yDiff > -e)
//             {
//                 return true;
//             }
//             else
//             {
//                 return false;
//             }
//         }
//
//
//         //Vector operations
//         public static float Dot(MyVector2 a, MyVector2 b)
//         {
//             float dotProduct = (a.x * b.x) + (a.y * b.y);
//
//             return dotProduct;
//         }
//
//         // Length of vector a: ||a||
//         public static float Magnitude(MyVector2 a)
//         {
//             float magnitude = Mathf.Sqrt(SqrMagnitude(a));
//
//             return magnitude;
//         }
//
//         public static float SqrMagnitude(MyVector2 a)
//         {
//             float sqrMagnitude = (a.x * a.x) + (a.y * a.y);
//
//             return sqrMagnitude;
//         }
//
//         public static float Distance(MyVector2 a, MyVector2 b)
//         {
//             float distance = Magnitude(a - b);
//
//             return distance;
//         }
//
//         public static float SqrDistance(MyVector2 a, MyVector2 b)
//         {
//             float distance = SqrMagnitude(a - b);
//
//             return distance;
//         }
//
//         public static MyVector2 Normalize(MyVector2 v)
//         {
//             float v_magnitude = Magnitude(v);
//
//             MyVector2 v_normalized = new MyVector2(v.x / v_magnitude, v.y / v_magnitude);
//
//             return v_normalized;
//         }
//
//
//         //Operator overloads
//         public static MyVector2 operator +(MyVector2 a, MyVector2 b)
//         {
//             return new MyVector2(a.x + b.x, a.y + b.y);
//         }
//
//         public static MyVector2 operator -(MyVector2 a, MyVector2 b)
//         {
//             return new MyVector2(a.x - b.x, a.y - b.y);
//         }
//
//         public static MyVector2 operator *(MyVector2 a, float b)
//         {
//             return new MyVector2(a.x * b, a.y * b);
//         }
//
//         public static MyVector2 operator *(float b, MyVector2 a)
//         {
//             return new MyVector2(a.x * b, a.y * b);
//         }
//
//         public static MyVector2 operator -(MyVector2 a)
//         {
//             return a * -1f;
//         }
//     }
// }