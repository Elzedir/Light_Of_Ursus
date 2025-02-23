using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pathfinding
{
    public class Node_Triangle
    {
        ulong _id;
        public ulong ID => _id != 0
        ? _id
        : _id = GetTriangleID(_a, _b, _c);
        
        readonly Vector3 _a, _b, _c;
        public Vector3[] Vertices => new[] { _a, _b, _c };

        public Dictionary<MoverType, float> MoverCosts;

        public Node_Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            _a = a;
            _b = b;
            _c = c;
        }
        
        public void SetMoverCosts(Dictionary<MoverType, float> costs)
        {
            MoverCosts = costs;
        }

        public Vector3 Centroid => new(
                (_a.x + _b.x + _c.x) / 3,
                (_a.y + _b.y + _c.y) / 3,
                (_a.z + _b.z + _c.z) / 3
            );
        
        public bool IsInsideCircumcircle(Vector3 point)
        {
            float ax = _a.x, az = _a.z, bx = _b.x, bz = _b.z, cx = _c.x, cz = _c.z;
            
            var circumcenter = _calculateCircumcenter(ax, az, bx, bz, cx, cz);
            
            var distanceToPoint = Vector3.SqrMagnitude(circumcenter - point);
            var radius = Vector3.SqrMagnitude(circumcenter - _a);
            
            var insideCircumcircle = distanceToPoint < radius;

            Manager_Game.S_Instance.StartCoroutine(_visualizeCircumcircle(point, circumcenter,
                Vector3.Distance(circumcenter, _a), insideCircumcircle));

            return insideCircumcircle;
        }
        
        a
            //* Sometimes we still get NaN for circumcenter or circumradius, check if their D adds up to 0 like last time.
        
        static Vector3 _calculateCircumcenter(float ax, float az, float bx, float bz, float cx, float cz)
        {
            var mx_Ab = (ax + bx) / 2;
            var mz_Ab = (az + bz) / 2;
            var mx_BC = (bx + cx) / 2;
            var mz_BC = (bz + cz) / 2;
            
            var slope_Ab = (bx - ax) == 0 
                ? float.MaxValue 
                : -(ax - bx) / (az - bz);
            var slope_BC = (cx - bx) == 0 
                ? float.MaxValue 
                : -(bx - cx) / (bz - cz);
            
            var x = (slope_Ab * mx_Ab - slope_BC * mx_BC + mz_BC - mz_Ab) / (slope_Ab - slope_BC);
            var z = slope_Ab * (x - mx_Ab) + mz_Ab;

            return new Vector3(x, 0, z);
        }


        static IEnumerator _visualizeCircumcircle(Vector3 point, Vector3 circumcenter, float circumradius, bool insideCircumcircle)
        {
            if (circumcenter == Vector3.zero)
            {
                Debug.LogError("Circumcenter is undefined, circumcircle cannot be visualized.");
                yield break;
            }

            if (insideCircumcircle) yield break;
            
            var circumcenterVoxel =
                Visualise_Voxel.Show_Voxel(new Vector3(circumcenter.x, 5, circumcenter.z), Vector3.one, 2);
            var voxel = Visualise_Voxel.Show_Voxel(point, Vector3.one, 1);

            var circle = Visualise_Circle.Show_Circle(
                position: new Vector3(circumcenter.x, circumcenter.y + 1, circumcenter.z),
                radius: circumradius, 100, 0);
                
            yield return new WaitForSeconds(0.2f);
                
            Object.Destroy(circumcenterVoxel);
            Object.Destroy(voxel);
            Object.Destroy(circle);
        }
        
        // LineRenderer _circumcircleRenderer;
        // public LineRenderer CircumcircleRenderer => _circumcircleRenderer ??= InitializeLineRenderer();
        //
        // LineRenderer InitializeLineRenderer() => 
        //     _circumcircleRenderer ??= GameObject.Find("LineRenderer").GetComponent<LineRenderer>();
        
        // const int _segments = 100;
        // const float _angleStep = 360f / _segments;
        //
        // void _drawCircumcircle(LineRenderer lineRenderer, Vector3 center, float radius)
        // {
        //     lineRenderer.positionCount = _segments + 1;
        //     for (var i = 0; i < _segments; i++)
        //     {
        //         var angle = i * _angleStep * Mathf.Deg2Rad;
        //         var position = new Vector3(center.x + radius * Mathf.Cos(angle), center.y, center.z + radius * Mathf.Sin(angle));
        //         lineRenderer.SetPosition(i, position);
        //     }
        //
        //     lineRenderer.SetPosition(_segments, lineRenderer.GetPosition(0));
        // }
        //
        // public IEnumerator HideCircumcircleAfterTime(float time)
        // {
        //     yield return new WaitForSeconds(time);
        //     _circumcircleRenderer.positionCount = 0;
        // }

        public List<Node_Edge> GetEdges()
        {
            return new List<Node_Edge>
            {
                new(_a, _b),
                new(_b, _c),
                new(_c, _a)
            };
        }
        
        public List<Node_Triangle> GetAdjacentTriangles(Dictionary<ulong, Node_Triangle> allTriangles)
        {
            var adjacentTriangles = new List<Node_Triangle>();

            // Iterate through all triangles to check for shared edges
            foreach (var triangle in allTriangles.Values)
            {
                if (triangle != this) // Don't compare the triangle with itself
                {
                    var commonEdges = 0;

                    foreach (var edge in GetEdges())
                    {
                        if (triangle.GetEdges().Contains(edge))
                        {
                            commonEdges++;
                        }
                    }

                    if (commonEdges > 0) // If one or more edges are shared, it's an adjacent triangle
                    {
                        adjacentTriangles.Add(triangle);
                    }
                }
            }

            return adjacentTriangles;
        }
        
        public static ulong GetTriangleID(Vector3 a, Vector3 b, Vector3 c)
        {
            var hash = 14695981039346656037UL;

            hash = _fnv1aHashVector(a, hash);
            hash = _fnv1aHashVector(b, hash);
            hash = _fnv1aHashVector(c, hash);

            return hash;
        }

        static ulong _fnv1aHashVector(Vector3 vector, ulong hash)
        {
            hash = _fnv1aHashComponent(vector.x, hash);
            hash = _fnv1aHashComponent(vector.y, hash);
            hash = _fnv1aHashComponent(vector.z, hash);

            return hash;
        }

        static ulong _fnv1aHashComponent(float component, ulong hash)
        {
            var bytes = BitConverter.GetBytes(component);
    
            foreach (var byt in bytes)
            {
                hash ^= byt;
                hash *= 1099511628211UL;
            }
    
            return hash;
        }
    }
}