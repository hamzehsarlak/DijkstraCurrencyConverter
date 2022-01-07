using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Algorithms.Searching.Graph
{
    public class Graph<T, TEdgeData> : IGraph<T, TEdgeData>
    {
        private List<IVertex<T>> Vertices { get; }   
        private List<IEdge<T, TEdgeData>> Edges { get; }              
        private ConcurrentDictionary<string, List<IEdge<T, TEdgeData>>[]> PathCacheValues { get; }

        public Graph()
        {
            Vertices = new List<IVertex<T>>();
            Edges = new List<IEdge<T, TEdgeData>>();
            PathCacheValues = new ConcurrentDictionary<string, List<IEdge<T, TEdgeData>>[]>();
        }


        public void AddEdge(T fromVertex, T toVertex, int weight = 0, TEdgeData data = default)
        {
            var fromVertexObj = new Vertex<T>(fromVertex);
            var toVertexObj = new Vertex<T>(toVertex);

            if (!Vertices.Contains(fromVertexObj))
                Vertices.Add(fromVertexObj);

            if (!Vertices.Contains(toVertexObj))
                Vertices.Add(toVertexObj);

            var edge = new Edge<T, TEdgeData>(fromVertexObj, toVertexObj, weight, data);
            if (Edges.IndexOf(edge) != -1) return;
            Edges.Add(edge);
            Vertices[Vertices.IndexOf(fromVertexObj)].AddAdjacentVertex(
                Vertices[Vertices.IndexOf(toVertexObj)]);
        }


        private void SetupDijkstraSearch(T fromVertex)
        {
            foreach (var vertex in Vertices)
            {
                vertex.Weight = vertex.Value.Equals(fromVertex) ? 0 : int.MaxValue;
            }
        }

        public List<IEdge<T, TEdgeData>>[] FindShortestPath(T fromVertex, T toVertex)
        {
            var cacheName = $"{fromVertex}_{toVertex}";
            if (PathCacheValues.TryGetValue(cacheName, out var value))
            {
                return value;
            }

            var shortestPaths = new List<IEdge<T, TEdgeData>>[3];    
            var queue = new List<IVertex<T>>(Vertices);  
            int shortestPathValue = int.MaxValue,       
                shortestPathValue2 = int.MaxValue,        
                shortestPathValue3 = int.MaxValue;        
            var shortestVertices = new Vertex<T>[3];       

            SetupDijkstraSearch(fromVertex);
            ClearPreviousVertices();

            while (queue.Count != 0)
            {
                queue.Sort();
                var currentVertex = queue[0];
                queue.RemoveAt(0);

                if (currentVertex.Equals(new Vertex<T>(toVertex)) && queue.Count != 0)
                {
                    queue.Sort();
                    var newVertex = queue[0];
                    queue.RemoveAt(0);
                    queue.Add(currentVertex);
                    currentVertex = newVertex;
                }

                foreach (var adjacentVertex in currentVertex.AdjacentVertices)
                {
                    if (!queue.Contains(adjacentVertex)) continue;

                    var currentVertexValue = currentVertex.Weight;
                    var indexOf = Edges.IndexOf(new Edge<T, TEdgeData>(currentVertex, adjacentVertex));
                    if(indexOf==-1) continue;
                    var edgeValue = Edges[indexOf].Weight;
                    var currentPathValue = currentVertexValue + edgeValue;

                    if (currentPathValue < adjacentVertex.Weight)
                    {
                        UpdateVertex(adjacentVertex, currentVertex);

                        if (adjacentVertex.Equals(new Vertex<T>(toVertex)))
                        {
                            shortestVertices[0] = new Vertex<T>(adjacentVertex);
                            shortestPathValue = currentVertexValue + edgeValue;
                        }
                    }
                    else if (currentPathValue > shortestPathValue &&
                             currentPathValue < shortestPathValue2)
                    {

                        if (ContainsVertex(shortestVertices[1], adjacentVertex))
                        {
                            shortestVertices[2] = new Vertex<T>(shortestVertices[1]);
                            shortestPathValue3 = shortestVertices[2].Weight;
                        }

                        shortestVertices[1] = new Vertex<T>(shortestVertices[0]);
                        UpdateVertex(shortestVertices[1], currentVertex);
                        shortestPathValue2 = currentPathValue;
                    }
                    else if (currentPathValue > shortestPathValue2 &&
                             currentPathValue < shortestPathValue3)
                    {

                        if (ContainsVertex(shortestVertices[1], adjacentVertex))
                        {
                            shortestVertices[2] = new Vertex<T>(shortestVertices[1]);
                        }

                        shortestVertices[2] = new Vertex<T>(shortestVertices[1]);
                        UpdateVertex(shortestVertices[2], currentVertex);
                        shortestPathValue3 = currentPathValue;
                    }
                }

                if (queue.Count == 0) continue;
                var vertex = queue[0];
                queue.RemoveAt(0);
                queue.Add(vertex);
            }

            for (var i = 0; i < 3; i++)
            {
                shortestPaths[i] = GetShortestPathEdges(shortestVertices[i]);
            }

            PathCacheValues.TryAdd(cacheName, shortestPaths);
            return shortestPaths;
        }


        private static bool ContainsVertex(IVertex<T> currentVertex, IVertex<T> checkContainment)
        {
            if (currentVertex == null)
                return false;

            IVertex<T> current = new Vertex<T>(currentVertex);

            while (current != null)
            {
                if (current.Equals(checkContainment))
                    return true;
                current = current.PreviousVertex;
            }
            return false;
        }


        private void ClearPreviousVertices()
        {
            foreach (var vertex in Vertices)
                vertex.PreviousVertex = null;
        }


        private void UpdateVertex(IVertex<T> adjacentVertex, IVertex<T> currentVertex)
        {
            var index = Edges.IndexOf(new Edge<T, TEdgeData>(currentVertex, adjacentVertex));
            if(index==-1) return;
            adjacentVertex.Weight = currentVertex.Weight + Edges[index].Weight;
            adjacentVertex.PreviousVertex = currentVertex;
        }

        private List<IEdge<T, TEdgeData>> GetShortestPathEdges(IVertex<T> toVertex)
        {
            var path = new List<IEdge<T, TEdgeData>>();
            while (true)
            {
                if (toVertex?.PreviousVertex == null) return path;

                var index = Edges.IndexOf(new Edge<T, TEdgeData>(toVertex.PreviousVertex, toVertex));
                if (index != -1) path.Add(Edges[index]);
                toVertex = toVertex.PreviousVertex;
            }
        }
    }
}
