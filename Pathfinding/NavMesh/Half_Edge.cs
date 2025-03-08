using UnityEngine;

namespace Pathfinding
{
    public class Half_Edge
    {
        public Vertex Vertex;

        public Node_Triangle Triangle;
        
        public Half_Edge Opposite, Next, Previous;

        public Half_Edge(Vertex vertex)
        {
            Vertex = vertex;
        }

        public int CompareTo(Half_Edge other)
        {
            if (other == null) return 1;

            var thisOrigin = Vertex.Position;
            var thisDestination = Next.Vertex.Position;
            var otherOrigin = other.Vertex.Position;
            var otherDestination = other.Next.Vertex.Position;

            var (thisSmaller, thisLarger) = _normalizeEdgeVertices(thisOrigin, thisDestination);
            var (otherSmaller, otherLarger) = _normalizeEdgeVertices(otherOrigin, otherDestination);

            var smallerComparison = CompareVector3(thisSmaller, otherSmaller);
            
            return smallerComparison != 0 ? smallerComparison : CompareVector3(thisLarger, otherLarger);
        }

        static (Vector3, Vector3) _normalizeEdgeVertices(Vector3 v1, Vector3 v2)
        {
            return CompareVector3(v1, v2) < 0 ? (v1, v2) : (v2, v1);
        }

        public static int CompareVector3(Vector3 a, Vector3 b)
        {
            if (!Mathf.Approximately(a.x, b.x)) return a.x.CompareTo(b.x);

            return !Mathf.Approximately(a.z, b.z) ? a.z.CompareTo(b.z) : a.y.CompareTo(b.y);
        }

        public override bool Equals(object obj)
        {
            if (obj is not Half_Edge other) return false;

            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            var origin = Vertex.Position;
            var destination = Next.Vertex.Position;
            var (smaller, larger) = _normalizeEdgeVertices(origin, destination);

            return smaller.GetHashCode() ^ larger.GetHashCode();
        }
    }
}