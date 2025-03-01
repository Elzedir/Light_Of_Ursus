namespace Pathfinding
{
    public class Edge_Half
    {
        public Vertex Vertex;
        
        public Node_Triangle Triangle;
        
        public Edge_Half Next;
        public Edge_Half Previous;
        
        public Edge_Half Opposite;
        
        public Edge_Half(Vertex vertex)
        {
            Vertex = vertex;
        }
    }
}