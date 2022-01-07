using System.Collections.Generic;

namespace Algorithms.Searching.Graph
{
    public interface IGraph<T, TEdgeData>
    {
        public void AddEdge(T fromVertex, T toVertex, int weight = 0, TEdgeData data = default);
        public List<IEdge<T, TEdgeData>>[] FindShortestPath(T fromVertex, T toVertex);

    }
}
